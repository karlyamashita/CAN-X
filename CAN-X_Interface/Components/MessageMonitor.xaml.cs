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
    /// Interaction logic for MessageMonitor.xaml
    /// </summary>
    public partial class MessageMonitor : UserControl
    {
        MainWindow mainWindow;
        public MessageMonitor()
        {
            InitializeComponent();

            mainWindow = Application.Current.MainWindow as MainWindow;

        }

        private void MenuItemSaveRx_Click(object sender, RoutedEventArgs e)
        {
            CanRxData data = dataGridRxWindow.SelectedItem as CanRxData;
            if (data == null)
            {
                // StatusBarStatus.Text = "Please select an ArbID to save";
                Console.WriteLine("Please select an ArbID to save");
                return;
            }

            // find next key number to use
            ulong highKey = 0;
            foreach (CanRxData item in mainWindow.editRxMessages.dataGridEditRxMessages.Items)
            {
                if (item.Key > highKey)
                {
                    highKey = item.Key;
                }
            }

            // copy selected message to new object
            CanRxData canRxData = new CanRxData(data);
            // assign new Key number
            canRxData.Key = highKey + 1;
            // add to datagrid
            mainWindow.editRxMessages.dataGridEditRxMessages.Items.Add(canRxData);
        }

        private void MenuItemSaveTx_Click(object sender, RoutedEventArgs e)
        {
            CanRxData data = dataGridRxWindow.SelectedItem as CanRxData;
            if (data == null)
            {
                // StatusBarStatus.Text = "Please select an ArbID to save";
                Console.WriteLine("Please select an ArbID to save");
                return;
            }

            // find next key number to use
            ulong highKey = 0;
            foreach (CanTxData item in mainWindow.editTxMessages.dataGridEditTxMessages.Items)
            {
                if (item.Key > highKey)
                {
                    highKey = item.Key;
                }
            }

            // copy selected message to new object
            CanTxData canTxData = new CanTxData(data);
            // assign new key number
            canTxData.Key = highKey + 1;
            // add to message editor Tx datagrid
            mainWindow.editTxMessages.dataGridEditTxMessages.Items.Add(canTxData);
            // add to main Tx datagrid
            mainWindow.transmitMessages.dataGridTxWindow.Items.Add(canTxData);
        }
    }
}
