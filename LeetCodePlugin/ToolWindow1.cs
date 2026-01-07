using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace LeetCodePlugin
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("5195e6d7-2308-4177-8d32-c1fad63c8c60")]
    public class ToolWindow1 : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1"/> class.
        /// </summary>
        Window window;
        public ToolWindow1() : base(null)
        {
            this.Caption = "LeetCodeExt";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new ToolWindow1Control();

            window = new Window
            {
                Title = "SetCookie",
                Width = 400,
                Height = 300,
                Content = new SetCookieWindow(), // 把 UserControl 放进去
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            SetCookieWindow cookieWin = (SetCookieWindow)window.Content;
            cookieWin.setParentWindow(window);

            ToolWindow1Control toolWindow = (ToolWindow1Control)this.Content;
            toolWindow.setCookieWindow(window);
        }
    }
}
