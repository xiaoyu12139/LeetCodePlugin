using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LeetCodePlugin
{
    /// <summary>
    /// Interaction logic for SetCookieWindow.xaml
    /// </summary>
    public partial class SetCookieWindow : UserControl
    {

        Window _parentWindow;

        public string csrftoken { get; set; } = "123";
        public string LEETCODE_SESSION { get; set; } = "123";
        public SetCookieWindow()
        {
            InitializeComponent();
            DataContext = this;
            csrftoken = LeetcodeTomlUtil.Instance.csrftoken;
            LEETCODE_SESSION = LeetcodeTomlUtil.Instance.LEETCODE_SESSION;
        }
        public void setParentWindow(Window window)
        {
            _parentWindow = window;
        }

        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            _parentWindow.Close();
            LeetcodeTomlUtil.Instance.modifyCsrftokenValue(csrftoken);
            LeetcodeTomlUtil.Instance.modifyLEETCODE_SESSIONValue(csrftoken);
            LeetcodeTomlUtil.Instance.saveAllValue();
        }

        private void CancelBtnClick(object sender, RoutedEventArgs e)
        {
            _parentWindow.Close();
        }
    }
}
