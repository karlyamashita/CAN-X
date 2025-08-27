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

namespace CAN_X_CAN_Analyzer
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            UpdateText();
        }

        private void UpdateText()
        {
            Label_version.Content = Properties.Settings.Default.version;
            Label_copyright.Content = Properties.Settings.Default.copyright_year;
        }
        private void button_ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
