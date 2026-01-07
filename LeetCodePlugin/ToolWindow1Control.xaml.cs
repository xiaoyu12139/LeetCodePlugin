using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace LeetCodePlugin
{
    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class ToolWindow1Control : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Control"/> class.
        /// </summary>
        Window cookieWindow;
        public ToolWindow1Control()
        {
            this.InitializeComponent();
        }

        public void setCookieWindow(Window cookieWindow)
        {
            this.cookieWindow = cookieWindow;
        }

        private string execLCCommand(string command)
        {
            // 创建 Process 实例
            Process process = new Process();

            // 配置启动信息
            process.StartInfo.FileName = "leetcode";
            process.StartInfo.Arguments = command; // /C 表示执行完命令后关闭 cmd
            process.StartInfo.RedirectStandardOutput = true;  // 重定向标准输出
            process.StartInfo.UseShellExecute = false;  // 不使用操作系统的 shell 启动
            process.StartInfo.CreateNoWindow = true;    // 不显示命令行窗口

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

        private void EditBtnClick(object sender, RoutedEventArgs e)
        {
            int num = 0;
            string command = "edit" + num;
            execLCCommand(command);
            problemContent.Text = "1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n1234\n";
        }

        private void PreviousBtnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "PreviousBtnClick");
        }

        private void NextBtnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "NextBtnClick");
        }

    }
}