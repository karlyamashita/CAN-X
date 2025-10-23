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
            Devices_Data data = dataGridCANableDevices.SelectedItem as Devices_Data; // grabs the current selected row
            if (data == null) return;
            TextBoxDeviceDescription.Text = data.Description;
            TextBoxAPB1Clock.Text = data.APB1_Clock;
        }

        private void DataGridDevices_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;

            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null)
            {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }
            if (dgr == null) { return; }

            rowIndexDevices = dgr.GetIndex();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAddDevice_Click(object sender, RoutedEventArgs e)
        {
            var matchFound = true;
            UInt32 newIndex = 0;
            Devices_Data devices = new Devices_Data();

            // check for available key number
            while (matchFound)
            {
                matchFound = false;
                foreach (var item in dataGridCANableDevices.Items)
                {
                    var it = item as Devices_Data;
                    if (it.Key == newIndex)
                    {
                        matchFound = true;
                    }
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
             devices.Key = newIndex;
             dataGridCANableDevices.Items.Add(devices);
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
            UInt32 newIndex = 0;

            if (dataGridCANableDevices.SelectedItem != null)
            {
                Devices_Data selectedItem = (Devices_Data)dataGridCANableDevices.SelectedItem;

                Devices_Data newDeviceData = new Devices_Data
                {
                    Description = selectedItem.Description,
                    COM_Port = selectedItem.COM_Port,
                    Node = selectedItem.Node,
                    APB1_Clock = selectedItem.APB1_Clock,
                    Baud_Rate = selectedItem.Baud_Rate,
                    CAN_Mode = selectedItem.CAN_Mode,
                    Connected_Status = selectedItem.Connected_Status
                };

                foreach (Devices_Data rx in dataGridCANableDevices.Items)
                {
                    if (rx.Key > newIndex)
                    {
                        newIndex = (UInt32)rx.Key;
                    }
                }
                newDeviceData.Key = newIndex + 1; // update key before adding item

                dataGridCANableDevices.Items.Add(newDeviceData);
            }
        }

        private void TextBoxDevice_TextChanged(object sender, TextChangedEventArgs e)
        {
            Devices_Data devicesData = (Devices_Data)dataGridCANableDevices.SelectedItem;

            TextBox obj = sender as TextBox;
            string senderName = obj.Name;

            if (devicesData != null)
            {
                switch (senderName)
                {
                    case "TextBoxDeviceDescription":
                        devicesData.Description = TextBoxDeviceDescription.Text;
                        break;
                    case "TextBoxAPB1Clock":
                        devicesData.APB1_Clock = TextBoxAPB1Clock.Text;
                        break;
                    default:
                        break;
                }

                dataGridCANableDevices.Items.Refresh();
            }
        }
    }
}
