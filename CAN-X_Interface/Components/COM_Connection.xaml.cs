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

namespace CAN_X_CAN_Analyzer.Components
{
    /// <summary>
    /// Interaction logic for COM_Connection.xaml
    /// </summary>
    public partial class COM_Connection : UserControl
    {
        MainWindow mainWindow;
        public COM_Connection()
        {
            InitializeComponent();

            mainWindow = Application.Current.MainWindow as MainWindow;
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            // TODO create an event callback instead of calling it through mainWindow
            mainWindow.ButtonConnect(); 
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.ButtonDisconnect();          
        }

        private void ComboBoxAPB1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainWindow.CalculateBTR();
        }

        private void ComboBoxBaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainWindow.CalculateBTR();
        }

        private void ComboBoxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                mainWindow.CalculateBTR();
            }
            catch (Exception ex)
            {

            }
            
        }

        private void ButtonBtrValue_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.ButtonBtrClicked();
        }

        private void CheckBoxBlind_Click(object sender, RoutedEventArgs e)
        {
            CAN_X_CAN_Analyzer.Properties.Settings.Default.imBlind = (bool)CheckBoxBlind.IsChecked;
            CAN_X_CAN_Analyzer.Properties.Settings.Default.Save();
            mainWindow.ResizeDataGridRx();
        }

        private void CheckBoxAscii_Click(object sender, RoutedEventArgs e)
        {
            CAN_X_CAN_Analyzer.Properties.Settings.Default.ascii = (bool)CheckBoxAscii.IsChecked;
            CAN_X_CAN_Analyzer.Properties.Settings.Default.Save();
            mainWindow.FormatDataGridColumns();
        }

        private void CheckBoxNotes_Click(object sender, RoutedEventArgs e)
        {
            CAN_X_CAN_Analyzer.Properties.Settings.Default.notes = (bool)CheckBoxNotes.IsChecked;
            CAN_X_CAN_Analyzer.Properties.Settings.Default.Save();
            mainWindow.FormatDataGridColumns();
        }

        private void ToggleButtonAutoTx_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.ToggleButtonAutoTx();
        }
    }
}
