﻿﻿﻿using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace LeetCodePlugin
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ToolWindow1Command
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b7bf9b3c-5cc1-412d-bc27-c294f5f6b4e0");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Command"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ToolWindow1Command(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
            
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ToolWindow1Command Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ToolWindow1Command's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ToolWindow1Command(package, commandService);
        }

        public static bool checkSomeThingBeforePluginRun()
        {
            bool checkFlag = false;
            //检查环境是否正常，
            //1.是否存在leetcode.exe
            bool commandExist = CommandExists("leetcode");
            if (!commandExist)
            {
                MessageBox.Show("command leetcode is not exist.", "PreCheck");
            }
            //2.指定目录下是否存在配置文件
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            bool dirExist = CheckDirectoryExists(Path.Combine(userDir, ".leetcode"));
            string dotleetcodeDir = Path.Combine(userDir, ".leetcode");
            string configFilePath = Path.Combine(dotleetcodeDir, "leetcode.toml");
            if (!dirExist)
            {
                MessageBox.Show(string.Format("dir: {0} not found.", dotleetcodeDir), "PreCheck");
                Directory.CreateDirectory(Path.Combine(userDir, ".leetcode"));
            }
            bool fileExist = CheckFileExists(configFilePath);
            if (!fileExist)
            {
                MessageBox.Show(string.Format("file: {0} not found.", configFilePath), "PreCheck");
                CopyWpfResource("Resources/leetcode.toml", configFilePath, typeof(ToolWindow1Command).Assembly);
            }
            //checkFlag = commandExist && dirExist && fileExist;
            //command不存在不打开插件窗口，配置文件不存在生成默认的配置文件
            checkFlag = commandExist;
            return checkFlag;
        }

        static void CopyWpfResource(string relativePath, string targetPath, Assembly asm)
        {
            if (asm == null) asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var asmName = asm.GetName().Name;
            var resourceName = asmName + "." + relativePath.Replace('/', '.');
            Stream stream = asm.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                var baseDir = Path.GetDirectoryName(asm.Location);
                var diskPath = Path.Combine(baseDir ?? "", relativePath.Replace('/', '\\'));
                if (File.Exists(diskPath))
                    stream = File.OpenRead(diskPath);
            }
            if (stream == null) return;

            var dir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            using (var input = stream)
            using (var output = File.Create(targetPath))
                input.CopyTo(output);
        }

        public static bool CommandExists(string command)
        {
            string checker = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "where"
                : "which";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = checker,
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }

        // 封装的检查目录是否存在的函数
        public static bool CheckDirectoryExists(string dirPath)
        {
            return Directory.Exists(dirPath);
        }

        // 封装的检查文件是否存在的函数
        public static bool CheckFileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(ToolWindow1), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }
            if (!checkSomeThingBeforePluginRun())
            {
                return;
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
