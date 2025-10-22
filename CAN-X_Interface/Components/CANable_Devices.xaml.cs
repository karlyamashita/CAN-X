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
    /// Interaction logic for CANable_Devices.xaml
    /// </summary>
    public partial class CANable_Devices : UserControl
    {
        int rowIndexDevices = 0;

        public CANable_Devices()
        {
            InitializeComponent();
        }

        private void DataGridDevices_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void DataGridDevices_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAddDevice_Click(object sender, RoutedEventArgs e)
        {
            var matchFound = true;
            UInt32 newIndex = 0;

            // check for available key number
            while (matchFound)
            {
                matchFound = false;
                foreach (var item in dataGridCANableDevices.Items)
                {
                    /*
                    var it = item as CanRxData;
                    if (it.Key == newIndex)
                    {
                        matchFound = true;
                    }
                    */
                }
                if (matchFound)
                {
                    newIndex += 1;
                }
                else
                {
                    matchFound = false;
                }
            }
            // canRxData.Key = newIndex;
            // dataGridCANableDevices.Items.Add(canRxData);
        }

        private void ButtonDeleteDevices_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridCANableDevices.SelectedItem != null)
            {
                // TODO - need to find solution to delete selected row, for now using index
                dataGridCANableDevices.Items.RemoveAt(rowIndexDevices);
            }
        }

        private void ButtonCopyDevices_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}
