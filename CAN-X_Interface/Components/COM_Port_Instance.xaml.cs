using System;
using System.Collections.Generic;
using System.IO.Ports;
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
    /// Interaction logic for COM_Port_Instance.xaml
    /// </summary>
    public partial class COM_Port_Instance : UserControl
    {
        private SerialPort _serialPort;

        const int COM_PORT_QUEUE_SIZE = 256;

        const byte COMMAND_INFO = 0x90; // 
        const byte COMMAND_HARDWARE = 0x93; // 

        string nodeSelected = string.Empty;


        public event EventHandler<COM_PortsEventArgs> COM_PortsEvent;

        public class COM_PortsEventArgs : EventArgs
        {
            public string EventType { get; set; }
            // Add other properties as needed
            public byte[] Data { get; set; }

            // Add other properties as needed
        }

        // Helper method to raise the event
        protected virtual void OnMyCustomEvent(COM_PortsEventArgs e)
        {
            COM_PortsEvent.Invoke(this, e);
        }
        public COM_Port_Instance()
        {
            InitializeComponent();

            _serialPort = new SerialPort();
            // Populate ComPortComboBox and BaudRateComboBox on load
            PopulateComPorts();
        }

        private void PopulateComPorts()
        {
            ComPortComboBox.ItemsSource = SerialPort.GetPortNames();
            if (ComPortComboBox.Items.Count > 0)
            {
                ComPortComboBox.SelectedIndex = 0;
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.PortName = ComPortComboBox.SelectedItem.ToString();
                    // Configure other settings like Parity, DataBits, StopBits if needed
                    _serialPort.Open();
                    StatusTextBlock.Text = $"Connected to {_serialPort.PortName}";
                    ConnectButton.Content = "Disconnect";

                    _serialPort.DataReceived += ComPortManager_DataReceived;

                    byte[] data = new byte[16];
                    data[0] = COMMAND_INFO;
                    _serialPort.Write(data, 0, 1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error connecting: {ex.Message}");
                    StatusTextBlock.Text = "Disconnected";
                }
            }
            else
            {
                _serialPort.Close();
                StatusTextBlock.Text = "Disconnected";
                ConnectButton.Content = "Connect";
            }
        }

        private void ComPortManager_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;

            byte[,] twoDByteArray = new byte[COM_PORT_QUEUE_SIZE, bytesToRead];
            int msgCount = 0;

            QueueCOM_PortData(ref twoDByteArray, sp, ref msgCount);

            int rowLength = twoDByteArray.GetLength(1);
            for (int i = 0; i < msgCount; i++)
            {
                byte[] singleDimByteArray = new byte[rowLength];
                int rowIndexToCopy = i;
                int sourceOffset = rowIndexToCopy * rowLength * sizeof(byte);

                Buffer.BlockCopy(twoDByteArray, sourceOffset, singleDimByteArray, 0, rowLength * sizeof(byte));

                // ParseUsbData(ref singleDimByteArray);
                OnMyCustomEvent(new COM_PortsEventArgs { EventType = nodeSelected, Data = singleDimByteArray });
            }
        }

        private void QueueCOM_PortData(ref byte[,] buffer, SerialPort sp, ref int msgCount)
        {
            int idxPtr = 0;
            int i = 0;
            int messageLength = 0;
            int msgDataPtr = 0;
            int bytesToRead = sp.BytesToRead;

            byte[] data = new byte[bytesToRead];

            sp.Read(data, 0, bytesToRead);

            foreach (byte b in data)
            {
                if (msgDataPtr == 3) messageLength = b + msgDataPtr + 1; // index 3 is the data size expected

                buffer[idxPtr, msgDataPtr] = data[i];

                if (++msgDataPtr == messageLength) // end of current message
                {
                    msgDataPtr = 0;
                    ++idxPtr; // increment to next queue
                }
                ++i;
            }

            msgCount = idxPtr; // return queue size
        }

        private void ComboBox_CAN_Node_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ComboBoxItem selectedItem = (ComboBoxItem)ComboBox_CAN_Node.SelectedItem;
            
           // string contentString = selectedItem.ToString();

            //nodeSelected = contentString;
            nodeSelected = "CAN 1";
            Console.WriteLine($"Node Selected: {nodeSelected}");
        }
    }
}
