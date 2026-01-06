using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

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
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "EditBtnClick");
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