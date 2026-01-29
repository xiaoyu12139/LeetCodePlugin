using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

// 如果也需要 EnvDTE.Window，可按需：
using DteWindow = EnvDTE.Window;
using IOPath = System.IO.Path;
using SDProcess = System.Diagnostics.Process;
using WpfWindow = System.Windows.Window;

namespace LeetCodePlugin
{
    public static class VsFileOps
    {

        public static IEnumerable<string> FindFilesByName(string root, string nameOrPattern)
        {
            // 支持通配符："*.rc"、"app.config"、"*.toml"
            if (nameOrPattern.IndexOfAny(new[] { '*', '?' }) >= 0)
                return Directory.EnumerateFiles(root, nameOrPattern, SearchOption.AllDirectories);

            // 精确名称（不区分大小写）
            return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                            .Where(p => string.Equals(IOPath.GetFileName(p), nameOrPattern,
                                                      StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<string> GetCurrentEditFilePathAsync(IAsyncServiceProvider provider, string nameOrPattern)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = (DTE2)await provider.GetServiceAsync(typeof(DTE));
            var proj = GetActiveProject(dte);
            if (proj == null) return null;

            var root = GetProjectRoot(proj);
            string result = null;

            // 先走文件系统遍历（快，覆盖所有磁盘文件）
            if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
            {
                foreach (var p in FindFilesByName(root, nameOrPattern))
                {
                    result = p;
                    break;
                }
            }

            return result;
        }


        public static async Task CreateFileInActiveProjectAsync(AsyncPackage pkg, string relativePath, string content)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = (DTE2)await pkg.GetServiceAsync(typeof(DTE));
            Project proj = GetActiveProject(dte);
            if (proj == null) return;

 
            var root = GetProjectRoot(proj);
            var fullPath = IOPath.Combine(root, relativePath);

            // 目录判空与创建
            var dir = IOPath.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(fullPath))
                File.WriteAllText(fullPath, content);

            // 如果选中了某个“物理文件夹”，优先加到该文件夹
            ProjectItem targetFolder = GetSelectedPhysicalFolder(dte);
            if (targetFolder != null)
                targetFolder.ProjectItems.AddFromFile(fullPath);
            else
                proj.ProjectItems.AddFromFile(fullPath);

            dte.ItemOperations.OpenFile(fullPath);
        }
        private static string TryGetProp(Project proj, string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var props = proj.Properties;
                if (props == null) return null;
                var p = props.Item(name);                 // 某些项目类型上会抛 E_INVALIDARG
                return p?.Value as string;
            }
            catch
            {
                return null;
            }
        }

        private static string GetProjectRoot(Project proj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // 优先取目录型属性
            var root =
                TryGetProp(proj, "FullPath")      // C#/VB/VC，SDK 项目通常返回目录
             ?? TryGetProp(proj, "ProjectDir")    // 某些扩展/工具链会提供
             ?? TryGetProp(proj, "LocalPath");    // 旧式项目可能存在

            if (!string.IsNullOrEmpty(root)) return root;

            // 回退：proj.FullName 是工程文件绝对路径，取其目录
            var fullName = proj.FullName;
            if (!string.IsNullOrEmpty(fullName))
                return IOPath.GetDirectoryName(fullName);

            // 最后兜底：用解决方案目录
            return IOPath.GetDirectoryName(proj.DTE?.Solution?.FullName);
        }

        private static Project GetActiveProject(DTE2 dte)
        {
            var se = dte.ToolWindows.SolutionExplorer;
            var selected = (Array)se.SelectedItems;
            if (selected != null && selected.Length > 0)
            {
                var item = (UIHierarchyItem)selected.GetValue(0);
                if (item.Object is Project p) return p;
                if (item.Object is ProjectItem pi) return pi.ContainingProject;
            }
            var actives = (Array)dte.ActiveSolutionProjects;
            if (actives != null && actives.Length > 0) return (Project)actives.GetValue(0);
            return null;
        }

        private static ProjectItem GetSelectedPhysicalFolder(DTE2 dte)
        {
            var se = dte.ToolWindows.SolutionExplorer;
            var selected = (Array)se.SelectedItems;
            if (selected == null || selected.Length == 0) return null;
            var item = (UIHierarchyItem)selected.GetValue(0);
            if (item.Object is ProjectItem pi)
            {
                // 仅当选中的是“物理文件夹”时返回
                if (pi.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder) return pi;
            }
            return null;
        }
    }

    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class ToolWindow1Control : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Control"/> class.
        /// </summary>
        /// 
        private AsyncPackage _package;
        public void SetPackage(AsyncPackage package) => _package = package;
        WpfWindow cookieWindow;
        public ToolWindow1Control()
        {
            this.InitializeComponent();
            Loaded += (s, e) => editNum.TextChanged += editNumTextChanged;
            editNum.Text = LeetcodeTomlUtil.Instance.last_edit_num;
            //var item = langCombobox.Items.Cast<object>().FirstOrDefault(o => (o as Language)?.Name == LeetcodeTomlUtil.Instance.last_lang);
            //langCombobox.SelectedItem = item;
            //langCombobox.SelectedValue = LeetcodeTomlUtil.Instance.last_lang;
            langCombobox.Loaded += (s, e) =>
            {
                var src = langCombobox.ItemsSource as IEnumerable<Language>;
                var item = src?.FirstOrDefault(l =>
                    string.Equals(l.Name, LeetcodeTomlUtil.Instance.last_lang, StringComparison.OrdinalIgnoreCase));
                if (item != null) langCombobox.SelectedItem = item;
            };
            string output = execLCCommand("pick " + editNum.Text);
            problemContent.Text = output;
        }

        public void setCookieWindow(WpfWindow cookieWindow)
        {
            this.cookieWindow = cookieWindow;
        }

        private string execLCCommand(string command)
        {
            // 创建 Process 实例
            SDProcess process = new SDProcess();

            // 配置启动信息
            process.StartInfo.FileName = "leetcode";
            process.StartInfo.Arguments = command; // /C 表示执行完命令后关闭 cmd
            process.StartInfo.RedirectStandardOutput = true;  // 重定向标准输出
            process.StartInfo.UseShellExecute = false;  // 不使用操作系统的 shell 启动
            process.StartInfo.CreateNoWindow = true;    // 不显示命令行窗口
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            // 启动进程
            process.Start();

            // 读取并输出命令行执行结果
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(output);

            // 等待进程执行完毕
            process.WaitForExit();
            return output;
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]

        private void SetCookieBtnClick(object sender, RoutedEventArgs e)
        {
            cookieWindow.Show(); // 非模态
        }

        private async void EditBtnClick(object sender, RoutedEventArgs e)
        {
            var item = langCombobox.SelectedItem;   // 选中的对象
            var id = langCombobox.SelectedValue;  // 来自 SelectedValuePath
            var index = langCombobox.SelectedIndex;  // 索引（未选中为 -1）
            var st = editNum.Text;           // 文本（可编辑下拉框时）
            var ed = langCombobox.Text;
            string command = "edit " + st;
            execLCCommand(command);
            string output = execLCCommand("pick " + st);
            problemContent.Text = output;
            string targetFileName = "";
            string targetFileContent = "";
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string dotleetcodeDir = IOPath.Combine(userDir, ".leetcode");
            string codeDir = IOPath.Combine(dotleetcodeDir, "code");

            //遍历codeDir路径下的所有文件，当文件的名称以st开头，文件后缀为ed时读取该文件的内容到targetFileContent；文件名保存到targetFileName
            string ext = ed.StartsWith(".") ? ed : "." + ed;
            if (Directory.Exists(codeDir))
            {
                foreach (var file in Directory.EnumerateFiles(codeDir, "*", SearchOption.AllDirectories))
                {
                    var name = IOPath.GetFileName(file);
                    if (name.StartsWith(st, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(IOPath.GetExtension(name), ext, StringComparison.OrdinalIgnoreCase))
                    {
                        targetFileName = name;
                        targetFileContent = File.ReadAllText(file, Encoding.UTF8);
                        break;
                    }
                }
            }
            if (targetFileName == "" || targetFileContent == "")
                return;
            await VsFileOps.CreateFileInActiveProjectAsync(_package, @"Resources\" + targetFileName, targetFileContent);
            Tabs.SelectedIndex = 0;
        }

        private void PreviousBtnClick(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(
            //    string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
            //    "PreviousBtnClick");
            editNum.Text = (int.Parse(editNum.Text) - 1).ToString();
        }

        private void NextBtnClick(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(
            //    string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
            //    "NextBtnClick");
            editNum.Text = (int.Parse(editNum.Text) + 1).ToString();
        }

        private void editNumTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded || problemContent == null) return;
            var tb = (TextBox)sender;
            string text = tb.Text;
            // 处理逻辑
            string output = execLCCommand("pick " + text);
            problemContent.Text = output;
            Tabs.SelectedIndex = 0;
        }

        private async void testBtnClick(object sender, RoutedEventArgs e)
        {
            var st = editNum.Text;           // 文本（可编辑下拉框时）
            var ed = langCombobox.Text;
            var pattern = st + "*." + ed; // 或来自文本框
            string path = await VsFileOps.GetCurrentEditFilePathAsync(_package, pattern);
            bool exists = File.Exists(path);
            if (exists)
            {
                var name = IOPath.GetFileName(path);
                string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string dotleetcodeDir = IOPath.Combine(userDir, ".leetcode");
                string codeDir = IOPath.Combine(dotleetcodeDir, "code");
                string lcFile = IOPath.Combine(codeDir, name);
                File.WriteAllText(lcFile, File.ReadAllText(path, Encoding.UTF8), Encoding.UTF8);
            }
            string text = editNum.Text;
            // 处理逻辑
            string output = execLCCommand("test " + text);
            resultContent.Text = output;
            Tabs.SelectedIndex = 1;
        }

        private async void submitBtnClick(object sender, RoutedEventArgs e)
        {
            var st = editNum.Text;           // 文本（可编辑下拉框时）
            var ed = langCombobox.Text;
            var pattern = st + "*." + ed; // 或来自文本框
            string path = await VsFileOps.GetCurrentEditFilePathAsync(_package, pattern);
            bool exists = File.Exists(path);
            if (exists)
            {
                var name = IOPath.GetFileName(path);
                string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string dotleetcodeDir = IOPath.Combine(userDir, ".leetcode");
                string codeDir = IOPath.Combine(dotleetcodeDir, "code");
                string lcFile = IOPath.Combine(codeDir, name);
                File.WriteAllText(lcFile, File.ReadAllText(path, Encoding.UTF8), Encoding.UTF8);
            }
            string text = editNum.Text;
            // 处理逻辑
            string output = execLCCommand("exec " + text);
            resultContent.Text = output;
            Tabs.SelectedIndex = 1;
        }

        private void langComboboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}