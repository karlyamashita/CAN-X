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
using System.Windows.Shapes;

namespace USB_CAN_Interface
{
    /// <summary>
    /// Interaction logic for Window2CopyMsg.xaml
    /// </summary>
    public partial class Window2CopyMsg : Window
    {
        public Window2CopyMsg()
        {
            InitializeComponent();
        }

        private void Window2_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window2_Deactivated(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
