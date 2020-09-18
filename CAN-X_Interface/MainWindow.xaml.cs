using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using UsbHid;
using UsbHid.USB.Classes.Messaging;
using Button = System.Windows.Controls.Button;
using System.ComponentModel;
using DataGridCell = System.Windows.Controls.DataGridCell;
using System.Windows.Controls.Primitives;
using DataGrid = System.Windows.Controls.DataGrid;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;
using TextBox = System.Windows.Controls.TextBox;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Xml;
using System.Data;
using Path = System.IO.Path;
using ComboBox = System.Windows.Controls.ComboBox;
using USB_CAN_Interface;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Timer = System.Threading.Timer;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CAN_X_CAN_Analyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region const defines
        // standard ASCII characters
        const byte COMMAND_SOH = 0x01;
        const byte COMMAND_STX = 0x02;
        const byte COMMAND_ETX = 0x03;
        const byte COMMAND_EOT = 0x04;
        const byte COMMAND_ACK = 0x06; // acknowlege
        const byte COMMAND_LF = 0x0A;
        const byte COMMAND_CR = 0x0D;
        const byte COMMAND_NAK = 0x15; // not acknowlege

        // custom commands
        const byte COMMAND_MESSAGE = 0x80; // CAN message structure over USB

        const byte COMMAND_ENABLE_MESSAGES = 0xB0; // enable hardware to send messages on USB data
        const byte COMMAND_DISABLE_MESSAGES = 0xB1; // disable hardware from sending messages on USB data

        const byte COMMAND_INFO = 0x90; // get information from hardware, fw version, BTC value, type hardware
        const byte COMMAND_CAN_BTR = 0x91; // the CAN_BTC value from interface
        const byte COMMAND_VERSION = 0x92;
        const byte COMMAND_HARDWARE = 0x93; // 
        const byte COMMAND_FREQUENCY = 0x94; // the APB1 Frequency
        const byte COMMAND_BAUD = 0x95;

        // const defines
        const byte CAN_STD_ID = 0x00;
        const byte CAN_EXT_ID = 0x04;

        const int DATA_SIZE = 17;
        const bool NEW_DATA_FLAG = true;

        const int MAX_ROW_COUNT = 10000; // how many lines to receive. This is used for the progress as well
        #endregion

        #region variables
        // arrays, variables, objects
        public static UsbHidDevice Device;

        UInt32 lineCount = 1;

        public delegate void MessageParse(ref byte[] data);
        public delegate void SendMessage();

        int rowIndexEditTx = 0;
        int rowIndexEditRx = 0;

        bool pauseMessagesFlag = false;
        bool scrollMessagesFlag = false;
        bool isTransmitMessage = false;

        List<CanRxData> masterDataGridRx = new List<CanRxData>();

        string mainWindowTitle = "";

        System.Diagnostics.Stopwatch sw = null;
        Thread threadAutoTx = null;

        #endregion

        #region MainWindow
        public MainWindow()
        {
            InitializeComponent();

            // my init routines
            InitUsbDevice();

            dataGridRx.DataContext = this;

            Values = new ObservableCollection<CanRxData>();
        }
        #endregion

        private ObservableCollection<CanRxData> _values;

        public ObservableCollection<CanRxData> Values
        {
            get { return _values; }
            set
            {
                if (_values == value)
                    return;

                _values = value;
                OnPropertyChanged("Values");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region init USB device
        private void InitUsbDevice()
        {
            Device = new UsbHidDevice(0x0483, 0x5750);
            Device.OnConnected += DeviceOnConnected;
            Device.OnDisConnected += DeviceOnDisConnected;
            Device.DataReceived += DeviceDataReceived;
        }
        #endregion

        #region DeviceDataReceived, delegate to receive USB data
        private void DeviceDataReceived(byte[] data)
        {
            byte[] newBytes = new byte[DATA_SIZE];

            Array.Copy(data, newBytes, DATA_SIZE - 1);

            MessageParse msg = new MessageParse(ParseUsbData);
            this.Dispatcher.BeginInvoke(msg, new object[] { data });
        }
        #endregion

        #region parse the USB data received. This is running on a thread
        public void ParseUsbData(ref byte[] data)
        {
            switch (data[1])
            {
                case COMMAND_MESSAGE:
                    ParseDeviceCAN_Message(ref data);
                    break;
                case COMMAND_ACK:
                    StatusBarStatus.Text = "ACK Received";
                    break;
                case COMMAND_NAK:
                    StatusBarStatus.Text = "NAK Received";
                    break;
                case COMMAND_CAN_BTR:
                    ShowBTC_VALUE(data);
                    break;
                case COMMAND_VERSION:
                    ShowString(COMMAND_VERSION, data);
                    break;
                case COMMAND_HARDWARE:
                    ShowString(COMMAND_HARDWARE, data);
                    break;
                case COMMAND_FREQUENCY:
                    ParseABP1_Frequency(data);
                    break;
            }
        }
        #endregion

        #region Button Connect/Disconnect, DeviceOnConnect/Disconnect 
        // delegate when USB device is connected
        private void DeviceOnConnected()
        {
            DeviceConnectionChanged();
        }

        // delegate when USB device is disconnected
        private void DeviceOnDisConnected()
        {
            DeviceConnectionChanged();
        }

        // button event to connect to device
        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            Device.Connect();
            if (!Device.IsDeviceConnected)
            {
                RichTextBoxConnectStatus.Document.Blocks.Clear();
                Paragraph myParagraph = new Paragraph(new Run("Device Not Attached"))
                {
                    Foreground = Brushes.Black,
                    Background = Brushes.Gold,
                    //myParagraph.FontFamily = new FontFamily("Arial");
                    //myParagraph.FontSize = 12;
                    //myParagraph.FontWeight = FontWeights.UltraBold;
                    //myParagraph.FontStretch = FontStretches.UltraExpanded;
                    Padding = new Thickness(5, 1, 5, 1),
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center
                };
                RichTextBoxConnectStatus.Document.Blocks.Add(myParagraph);
            }
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Device.Disconnect();
        }

        private void DeviceConnectionChanged()
        {
            RichTextBoxConnectStatus.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if (Device.IsDeviceConnected)
                {
                    Console.WriteLine("Device Connected\n");

                    StatusBarStatus.Text = "";

                    RichTextBoxConnectStatus.Document.Blocks.Clear();
                    Paragraph myParagraph = new Paragraph(new Run("Device Connected"))
                    {
                        Foreground = Brushes.White,
                        Background = Brushes.Green,
                        FontWeight = FontWeights.Bold,
                        Padding = new Thickness(5, 1, 5, 1),
                        TextAlignment = TextAlignment.Center
                    };
                    RichTextBoxConnectStatus.Document.Blocks.Add(myParagraph);
                    GetInfo();// get the device info
                }
                else if (Device.IsDeviceConnected != true)
                {
                    Console.WriteLine("Device Not Connected\n");

                    RichTextBoxConnectStatus.Document.Blocks.Clear();
                    Paragraph myParagraph = new Paragraph(new Run("Device Not Connected"))
                    {
                        Foreground = Brushes.White,
                        Background = Brushes.Red,
                        FontWeight = FontWeights.Bold,
                        Padding = new Thickness(5, 1, 5, 1),
                        TextAlignment = TextAlignment.Center
                    };
                    RichTextBoxConnectStatus.Document.Blocks.Add(myParagraph);
                    ClearStatusBarStatus(); // clear the status text
                    ClearStatusSoftwareHarHardware();
                }
            }));
        }

        #endregion

        #region get and show string from data
        private void ShowString(byte command, byte[] data)
        {
            // todo - show the text sent by the interface. Need to figure out where to show. Maybe new TextBox or Lable.
            switch (command)
            {
                case COMMAND_VERSION:
                    StatusBarStatusVersion.Text = "FW: " + GetStringFromData(data);
                    break;
                case COMMAND_HARDWARE:
                    StatusBarStatusHardware.Text = "HW: " + GetStringFromData(data);
                    break;
            }
        }

        private string GetStringFromData(byte[] data)
        {
            int i = 0;
            byte[] temp = new byte[DATA_SIZE];

            while (data[i + 1] != '\0') // index 1 is command
            {
                temp[i] = data[i + 2]; // string starts at index 2 
                i++;
            }
            return Encoding.ASCII.GetString(temp);
        }
        #endregion

        #region show new CAN_BTC value
        private void ShowBTC_VALUE(byte[] data)
        {
            // todo - parse the BTC_VALUE and show in TextBoxBtcValue. Then set index in the ComboBoxBaudRate
            UInt32 btrValue = 0;
            btrValue = (UInt32)(data[2] << 24 | data[3] << 16 | data[4] << 8 | data[5]);

            if ((btrValue >> 31 & 0x1) == 1)
            {
                CheckBoxListenOnly.IsChecked = true;
            }

            TextBoxBtrValue.Text = "0x" + btrValue.ToString("X8");

            int i = 0;
            string comboBoxAPB1Name = ComboBoxAPB1.SelectedValue.ToString();
            CAN_BaudRate can_baudRate = new CAN_BaudRate(comboBoxAPB1Name);

            foreach (var item in can_baudRate.baudList)
            {
                if (item.value == TextBoxBtrValue.Text)
                {
                    ComboBoxBaudRate.SelectedIndex = i;
                    return;
                }
                i++;
            }
        }
        #endregion

        #region Parse device CAN message
        private void ParseDeviceCAN_Message(ref byte[] data)
        {
            // get the date now!
            DateTime now = DateTime.Now;
            string dateNow = now.ToString("HH:mm:ss.ffff");
            // shift command to index 0
            Array.Copy(data, 1, data, 0, DATA_SIZE);

            CanRxData canRxData = new CanRxData(data);
            canRxData.Line = lineCount++;
            canRxData.TimeAbs = dateNow;
            // ParseAscii
            ParseAscii(ref canRxData);

            // parse ID and compare to receive messages editor. Return string description if available
            ParseForDescription(ref canRxData);

            // add formatted data to data grid
            isTransmitMessage = false;
            AddToDataGrid(canRxData, isTransmitMessage, scrollMessagesFlag, NEW_DATA_FLAG);

            // add to master list, update count first
         //   canRxData.Count = count;
         //   masterDataGridRx.Add(canRxData);

            // update the progress bar and remove first row if we are at MAX_ROW_COUNT
            if (UpdateProgressBar()) dataGridRx.Items.RemoveAt(0);
        }
        #endregion

        #region Parse ASCII from data
        private void ParseAscii(ref CanRxData canRxData)
        {
            byte[] data = new byte[8];
            int dlcLength = (char)Convert.ToUInt16(canRxData.DLC);

            if (dlcLength >= 1)
            {
                data[0] = Convert.ToByte(canRxData.Byte1, 16);
            }
            if (dlcLength >= 2)
            {
                data[1] = Convert.ToByte(canRxData.Byte2, 16);
            }
            if (dlcLength >= 3)
            {
                data[2] = Convert.ToByte(canRxData.Byte3, 16);
            }
            if (dlcLength >= 4)
            {
                data[3] = Convert.ToByte(canRxData.Byte4, 16);
            }
            if (dlcLength >= 5)
            {
                data[4] = Convert.ToByte(canRxData.Byte5, 16);
            }
            if (dlcLength >= 6)
            {
                data[5] = Convert.ToByte(canRxData.Byte6, 16);
            }
            if (dlcLength >= 7)
            {
                data[6] = Convert.ToByte(canRxData.Byte7, 16);
            }
            if (dlcLength >= 8)
            {
                data[7] = Convert.ToByte(canRxData.Byte8, 16);
            }

            canRxData.ASCII = Encoding.UTF8.GetString(data);
        }
        #endregion

        #region Parse APB1 Frequency
        private void ParseABP1_Frequency(byte[] data)
        {
            int i = 0;
            byte[] temp = new byte[DATA_SIZE];

            while (data[i + 1] != '\0') // index 1 is command
            {
                temp[i] = data[i + 2]; // string starts at index 2 
                i++;
            }
            string apb1Freq = Encoding.ASCII.GetString(temp);

            apb1Freq = apb1Freq.Trim('\0');
            /*
            switch (apb1Freq)
            {
                case "APB1_36mHz":
                    StatusBarStatus.Text = "36mHz";
                    break;
                case "APB1_42mHz":
                    StatusBarStatus.Text = "42mHz";
                    break;
                case "APB1_48mHz":
                    StatusBarStatus.Text = "48mHz";
                    break;
                default:

                    break;
            }
            */
            i = 0;
            foreach(var en in Enum.GetNames(typeof(EnumDefines.APB1_Freq))){
                if(en == apb1Freq)
                {
                    ComboBoxAPB1.SelectedIndex = i;
                    break;
                }
                i++;
            }
        }
        #endregion

        #region Parse for Description
        // goes through the Receive messages to find a ArbID match and copy description if avaialbble
        private void ParseForDescription(ref CanRxData canRxData)
        {
            foreach (CanRxData row in dataGridEditRxMessages.Items)
            {
                if (row.ArbID == canRxData.ArbID)
                {
                    canRxData.Description = row.Description;
                    canRxData.Notes = row.Notes;
                }
            }
        }
        #endregion

        #region update progress bar
        private bool UpdateProgressBar()
        {
            // update progress bar
            int count = masterDataGridRx.Count;
            ProgressBar.Value = count;

            TextBoxBufferPercentage.Text = count.ToString() + "/" + MAX_ROW_COUNT.ToString();

            // remove data from datagrid if we reach max amount of rows
            if (count >= MAX_ROW_COUNT)
            {
                ProgressBar.Foreground = new SolidColorBrush(Colors.Red);
                return true;
            }
            ProgressBar.Foreground = new SolidColorBrush(Colors.PaleGreen);
            return false;
        }
        #endregion
    
        #region Add formatted data to datagrid
        private void AddToDataGrid(CanRxData canRxData, bool transmitFlag, bool scrollFlag, bool newData)
        {
            CanRxData canRxDataNew = new CanRxData();
            bool foundMatch = false;
            if (newData)
            {               
                if (scrollFlag)
                {
                    masterDataGridRx.Reverse();
                    foreach (CanRxData masterRow in masterDataGridRx)
                    {
                        if(!transmitFlag)
                        {
                            if (masterRow.ArbID == canRxData.ArbID && masterRow.Tx == false)
                            {
                                if (masterRow.Count == "")
                                {
                                    masterRow.Count = masterRow.CountSaved;
                                }
                                if (masterRow.Count == "")
                                {
                                    canRxDataNew.Count = (1).ToString();
                                }
                                else
                                {
                                    canRxDataNew.Count = (Convert.ToUInt32(masterRow.Count) + 1).ToString();
                                }

                                canRxDataNew.CountSaved = canRxDataNew.Count;
                                canRxDataNew.TxCount = "";

                                canRxDataNew.Line = canRxData.Line;
                                canRxDataNew.TimeAbs = canRxData.TimeAbs;

                                canRxDataNew.Description = canRxData.Description;

                                // ArbID matches so copy new data to current row
                                canRxDataNew.Tx = canRxData.Tx;
                                canRxDataNew.IDE = canRxData.IDE;
                                canRxDataNew.ArbID = canRxData.ArbID;
                                canRxDataNew.RTR = canRxData.RTR;

                                canRxDataNew.DLC = canRxData.DLC;
                                if(!String.Equals(canRxDataNew.Byte1, canRxData.Byte1))
                                {
                                    canRxDataNew.Byte1 = canRxData.Byte1;
                                }
                                if (!String.Equals(canRxDataNew.Byte2, canRxData.Byte2))
                                {
                                    canRxDataNew.Byte2 = canRxData.Byte2;
                                }
                                if (!String.Equals(canRxDataNew.Byte3, canRxData.Byte3))
                                {
                                    canRxDataNew.Byte3 = canRxData.Byte3;
                                }
                                if (!String.Equals(canRxDataNew.Byte4, canRxData.Byte4))
                                {
                                    canRxDataNew.Byte4 = canRxData.Byte4;
                                }
                                if (!String.Equals(canRxDataNew.Byte5, canRxData.Byte5))
                                {
                                    canRxDataNew.Byte5 = canRxData.Byte5;
                                }
                                if (!String.Equals(canRxDataNew.Byte6, canRxData.Byte6))
                                {
                                    canRxDataNew.Byte6 = canRxData.Byte6;
                                }
                                if (!String.Equals(canRxDataNew.Byte7, canRxData.Byte7))
                                {
                                    canRxDataNew.Byte7 = canRxData.Byte7;
                                }
                                if (!String.Equals(canRxDataNew.Byte8, canRxData.Byte8))
                                {
                                    canRxDataNew.Byte8 = canRxData.Byte8;
                                }

                                canRxDataNew.Node = canRxData.Node;

                                canRxDataNew.ASCII = canRxData.ASCII;

                                canRxDataNew.Notes = canRxData.Notes;

                                foundMatch = true;

                                masterDataGridRx.Reverse();// back to original order
                                break;
                            }
                            else
                            {
                                canRxData.TxCount = "";
                            }
                        }
                        else
                        {
                            if (masterRow.ArbID == canRxData.ArbID && masterRow.Tx == true)
                            {
                                if (masterRow.TxCount == "")
                                {
                                    masterRow.TxCount = masterRow.TxCountSaved;
                                }
                                if (masterRow.TxCount == "")
                                {
                                    canRxDataNew.TxCount = (1).ToString();
                                }
                                else
                                {
                                    canRxDataNew.TxCount = (Convert.ToUInt32(masterRow.TxCount) + 1).ToString();
                                }

                                canRxDataNew.TxCountSaved = canRxDataNew.TxCount;
                                canRxDataNew.Count = "";

                                canRxDataNew.Line = canRxData.Line;
                                canRxDataNew.TimeAbs = canRxData.TimeAbs;

                                canRxDataNew.Description = canRxData.Description;

                                // ArbID matches so copy new data to current row
                                canRxDataNew.Tx = canRxData.Tx;
                                canRxDataNew.IDE = canRxData.IDE;
                                canRxDataNew.ArbID = canRxData.ArbID;
                                canRxDataNew.RTR = canRxData.RTR;

                                canRxDataNew.DLC = canRxData.DLC;
                                if (!String.Equals(canRxDataNew.Byte1, canRxData.Byte1))
                                {
                                    canRxDataNew.Byte1 = canRxData.Byte1;
                                }
                                if (!String.Equals(canRxDataNew.Byte2, canRxData.Byte2))
                                {
                                    canRxDataNew.Byte2 = canRxData.Byte2;
                                }
                                if (!String.Equals(canRxDataNew.Byte3, canRxData.Byte3))
                                {
                                    canRxDataNew.Byte3 = canRxData.Byte3;
                                }
                                if (!String.Equals(canRxDataNew.Byte4, canRxData.Byte4))
                                {
                                    canRxDataNew.Byte4 = canRxData.Byte4;
                                }
                                if (!String.Equals(canRxDataNew.Byte5, canRxData.Byte5))
                                {
                                    canRxDataNew.Byte5 = canRxData.Byte5;
                                }
                                if (!String.Equals(canRxDataNew.Byte6, canRxData.Byte6))
                                {
                                    canRxDataNew.Byte6 = canRxData.Byte6;
                                }
                                if (!String.Equals(canRxDataNew.Byte7, canRxData.Byte7))
                                {
                                    canRxDataNew.Byte7 = canRxData.Byte7;
                                }
                                if (!String.Equals(canRxDataNew.Byte8, canRxData.Byte8))
                                {
                                    canRxDataNew.Byte8 = canRxData.Byte8;
                                }

                                canRxDataNew.Node = canRxData.Node;

                                canRxDataNew.ASCII = canRxData.ASCII;

                                canRxDataNew.Notes = canRxData.Notes;


                                foundMatch = true;

                                masterDataGridRx.Reverse();// back to original order
                                break;
                            }   
                            else
                            {
                                canRxData.Count = "";
                            }
                        }
                    }
                }
                else
                {
                    //foreach (CanRxData row in dataGridRx.Items)

                    foreach (CanRxData row in Values)
                    {
                        if (row.ArbID == canRxData.ArbID)
                        {
                            masterDataGridRx.Reverse();
                            if (row.Count == "")
                            {
                                row.Count = "1";
                            }

                            row.Count = (Convert.ToUInt32(row.Count) + 1).ToString();

                            row.Line = canRxData.Line;
                            row.TimeAbs = canRxData.TimeAbs;

                            row.Description = canRxData.Description;

                            // ArbID matches so copy new data to current row
                            row.Tx = canRxData.Tx;
                            row.IDE = canRxData.IDE;
                            row.ArbID = canRxData.ArbID;
                            row.RTR = canRxData.RTR;

                            row.DLC = canRxData.DLC;
                            if (!String.Equals(row.Byte1, canRxData.Byte1))
                            {
                                row.Byte1 = canRxData.Byte1;
                            }
                            if (!String.Equals(row.Byte2, canRxData.Byte2))
                            {
                                row.Byte2 = canRxData.Byte2;
                            }
                            if (!String.Equals(row.Byte3, canRxData.Byte3))
                            {
                                row.Byte3 = canRxData.Byte3;
                            }
                            if (!String.Equals(row.Byte4, canRxData.Byte4))
                            {
                                row.Byte4 = canRxData.Byte4;
                            }
                            if (!String.Equals(row.Byte5, canRxData.Byte5))
                            {
                                row.Byte5 = canRxData.Byte5;
                            }
                            if (!String.Equals(row.Byte6, canRxData.Byte6))
                            {
                                row.Byte6 = canRxData.Byte6;
                            }
                            if (!String.Equals(row.Byte7, canRxData.Byte7))
                            {
                                row.Byte7 = canRxData.Byte7;
                            }
                            if (!String.Equals(row.Byte8, canRxData.Byte8))
                            {
                                row.Byte8 = canRxData.Byte8;
                            }

                            row.Node = canRxData.Node;

                            row.ASCII = canRxData.ASCII;

                            row.Notes = canRxData.Notes;

                            foundMatch = true;

                            break;
                        }
                    }
                }

                if (foundMatch)
                {
                    if (scrollFlag)
                    {
                        masterDataGridRx.Add(canRxDataNew); // matchFound
                        dataGridRx.ClearValue(ItemsControl.ItemsSourceProperty);
                        dataGridRx.Items.Add(canRxDataNew);
                    }
                    else
                    {
                        masterDataGridRx.Add(canRxData); // matchFound
                     //   dataGridRx.Items.Refresh();
                    }
                }
                else
                {
                    masterDataGridRx.Reverse();// back to original order
                    masterDataGridRx.Add(canRxData);

                    Values.Add(canRxData);

                    Debug.WriteLine(Values.Count);

                 //   dataGridRx.ClearValue(ItemsControl.ItemsSourceProperty);
                 //   dataGridRx.Items.Add(canRxData);
                }
            }
 
            if (pauseMessagesFlag == false)
            {
                if (dataGridRx.Items.Count > 0)
                {
                    var border = VisualTreeHelper.GetChild(dataGridRx, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null) scroll.ScrollToEnd();
                    }
                }
            }
        }
        #endregion

        #region Button event to send CAN Tx message and to update DataGrid. Starts delegate
        private void ButtonTxMessage_Click(object sender, RoutedEventArgs e)
        {
            if (!Device.IsDeviceConnected)
            {
                StatusBarStatus.Text = "Device Not Connected";
                return;
            }
            SendMessage msg = new SendMessage(SendTxMsgToDataGridAndCanBus);
            this.Dispatcher.BeginInvoke(msg);
        }

        // send Tx message to device and update data grid
        private void SendTxMsgToDataGridAndCanBus()
        {
            DateTime now = DateTime.Now;
            string dateNow = now.ToString("HH:mm:ss.ffff");

            // get the current selected row data
            CanTxData canTxData = dataGridTx.SelectedItem as CanTxData;
            // send to device
            SendCanData(ref canTxData);

            // formatting canRxData with canTxData and adding line count, time, tx
            CanRxData canRxData = new CanRxData(canTxData);
            canRxData.Line = lineCount++;
            canRxData.TimeAbs = dateNow;
            canRxData.Tx = true;
            // parse ASCII characters from data bytes
            ParseAscii(ref canRxData);

            // add formatted data to data grid
            isTransmitMessage = true;
            AddToDataGrid(canRxData, isTransmitMessage, scrollMessagesFlag, NEW_DATA_FLAG);
        }
        #endregion

        #region Send command to get the connected device version and hardware type
        private void GetInfo()
        {
            byte[] tmp_buf = new byte[DATA_SIZE]; // command + 63 byte = 64 bytes

            var command = new CommandMessage(COMMAND_INFO, tmp_buf); // no data to send but need array
            Device.SendMessage(command);
        }
        #endregion

        #region Send CAN TX message to device over USB
        private void SendCanData(ref CanTxData canData)
        {
            byte[] usbPacket = new byte[DATA_SIZE - 1]; // command + (DATA_SIZE - 1) should be less than 64 bytes

            // CAN Type ExID = 4, StdID = 0
            if (canData.IDE == "S")
            {
                usbPacket[0] = CAN_STD_ID;
            }
            else
            {
                usbPacket[0] = CAN_EXT_ID;
            }

            // RTR
            usbPacket[1] = canData.RTR == true ? (byte) 1: (byte) 0; // RTR, Node

            // Node
            byte i = 0;
            foreach (var en in Enum.GetNames(typeof(EnumDefines.Nodes)))
            {
                if(en == canData.Node)
                {
                    usbPacket[2] = i;
                    break;
                }
                i++;
            }
 
            // Arb ID 29/11 bit
            if (canData.IDE == "CAN_STD_ID")
            {
                UInt32 extID = Convert.ToUInt32(canData.ArbID, 16);
                extID = extID & 0x7FF;
                usbPacket[3] = (byte)(extID & 0xFF); // LSB GMLAN power mode ID
                usbPacket[4] = (byte)(extID >> 8 & 0xFF);
            }
            else
            {
                UInt32 extID = Convert.ToUInt32(canData.ArbID, 16);
                usbPacket[3] = (byte)(extID & 0xFF); // LSB GMLAN power mode ID
                usbPacket[4] = (byte)(extID >> 8 & 0xFF);
                usbPacket[5] = (byte)(extID >> 16 & 0xFF);
                usbPacket[6] = (byte)(extID >> 24 & 0xFF); // MSB         
            }

            //DLC
            if (canData.DLC != "")
            {
                usbPacket[7] = Convert.ToByte(canData.DLC);
            }

            // data bytes
            if (canData.Byte1 != "")
            {
                usbPacket[8] = Convert.ToByte(canData.Byte1, 16);
            }

            if (canData.Byte2 != "")
            {
                usbPacket[9] = Convert.ToByte(canData.Byte2, 16);
            }

            if (canData.Byte3 != "")
            {
                usbPacket[10] = Convert.ToByte(canData.Byte3, 16);
            }

            if (canData.Byte4 != "")
            {
                usbPacket[11] = Convert.ToByte(canData.Byte4, 16);
            }

            if (canData.Byte5 != "")
            {
                usbPacket[12] = Convert.ToByte(canData.Byte5, 16);
            }

            if (canData.Byte6 != "")
            {
                usbPacket[13] = Convert.ToByte(canData.Byte6, 16);
            }

            if (canData.Byte7 != "")
            {
                usbPacket[14] = Convert.ToByte(canData.Byte7, 16);
            }

            if (canData.Byte8 != "")
            {
                usbPacket[15] = Convert.ToByte(canData.Byte8, 16);
            }

            var command = new CommandMessage(COMMAND_MESSAGE, usbPacket);
            Device.SendMessage(command);
        }
        #endregion

        #region clear receive window, ClearStatusBar
        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            dataGridRx.ClearValue(ItemsControl.ItemsSourceProperty);
            while (dataGridRx.Items.Count != 0)
            {
                dataGridRx.Items.RemoveAt(0);
            }
            Values.Clear();
            while (masterDataGridRx.Count != 0)
            {
                masterDataGridRx.RemoveAt(0);
            }
            ProgressBar.Value = 0;
            lineCount = 0;
        }

        private void ClearStatusBarStatus()
        {
            StatusBarStatus.Text = "";
        }

        private void ClearStatusSoftwareHarHardware()
        {
            StatusBarStatusHardware.Text = "";
            StatusBarStatusVersion.Text = "";
        }
        #endregion

        #region sends new baud rate to device and/or Listen mode
        // Todo - this modifies CAN1, need to make another button  or another approach to modify CAN2, SWCAN, etc
        private void ButtonBtrValue_Click(object sender, RoutedEventArgs e)
        {
            if (!Device.IsDeviceConnected)
            {
                StatusBarStatus.Text = "Device Not Connected";
                return;
            }

            byte[] tmp_buf = new byte[DATA_SIZE];
            string myString = TextBoxBtrValue.Text;
            UInt32 btrValue;
            try
            {
                btrValue = Convert.ToUInt32(myString, 16);
            }
            catch (FormatException)
            {
                StatusBarStatus.Text = "Hex value is not in correct format";
                return;
            }


            tmp_buf[0] = (byte)(btrValue >> 24);
            tmp_buf[1] = (byte)(btrValue >> 16);
            tmp_buf[2] = (byte)(btrValue >> 8);
            tmp_buf[3] = (byte)(btrValue);
            if (CheckBoxListenOnly.IsChecked == true)
            {
                tmp_buf[0] = (byte)(tmp_buf[0] | 0x80);// bit 31 is Normal=0, Silent = 1. Bit 30 is Loopback mode, disable = 0, loopback enabled = 1
            }

            tmp_buf[4] = 0; // CAN1

            StatusBarStatus.Text = "Sending BTR Value";
            var command = new CommandMessage(COMMAND_BAUD, tmp_buf);
            Device.SendMessage(command);
        }
        #endregion

        #region ComboBox BTR, Node, init
        private void InitPopulateBaudRateListBox()
        {
            // todo - allow use to select frequency to adjust CAN_BTR value for that frequency
            CAN_BaudRate can_baudRate = new CAN_BaudRate("APB1_48mHz");

            foreach (var item in can_baudRate.baudList)
            {
                ComboBoxBaudRate.Items.Add(item.baud);
            }
            ComboBoxBaudRate.SelectedIndex = 1;

            ComboBoxAPB1.ItemsSource = Enum.GetNames(typeof(EnumDefines.APB1_Freq));
            ComboBoxAPB1.SelectedIndex = 0;
        }

        private void ComboBoxBaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxBaudRate.Items.Count == 0 || ComboBoxAPB1.Items.Count == 0) return;
            string comboBoxItemName = ComboBoxBaudRate.SelectedValue.ToString();
            string comboBoxAPB1Name = ComboBoxAPB1.SelectedValue.ToString();

            CAN_BaudRate can_baudRate = new CAN_BaudRate(comboBoxAPB1Name);
            string value = "";
            foreach (var baud in can_baudRate.baudList)
            {
                if (comboBoxItemName == baud.baud)
                {
                    value = baud.value;
                    break;
                }
            }
            TextBoxBtrValue.Text = value;// baudRateList[indexItem].value;
        }

        private void ComboBoxAPB1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string comboBoxItemName = ComboBoxBaudRate.SelectedValue.ToString();
            string comboBoxAPB1Name = ComboBoxAPB1.SelectedValue.ToString();

            CAN_BaudRate can_baudRate = new CAN_BaudRate(comboBoxAPB1Name);
            string value = "";
            foreach (var baud in can_baudRate.baudList)
            {
                if (comboBoxItemName == baud.baud)
                {
                    value = baud.value;
                    break;
                }
            }
            TextBoxBtrValue.Text = value;// baudRateList[indexItem].value;
        }

        private void CheckBoxListenOnly_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxBtrValue.Text == "")
            {
                StatusBarStatus.Text = "Missing BTR value";
                CheckBoxListenOnly.IsChecked = !CheckBoxListenOnly.IsChecked;
                return;
            }
            else
            {
                StatusBarStatus.Text = "";
            }
            string btrValue = TextBoxBtrValue.Text;
            UInt32 btrNumber = Convert.ToUInt32(btrValue, 16);
            if (CheckBoxListenOnly.IsChecked == true)
            {
                btrNumber = (UInt32)(btrNumber | (1 << 31));
            }
            else
            {
                btrNumber = (UInt32)(btrNumber ^ (1 << 31));
            }
            TextBoxBtrValue.Text = "0x" + btrNumber.ToString("X8");
        }

        private void ComboBoxNodeSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        #endregion

        #region main Window loaded
        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow1.Title = USB_CAN_Interface.Properties.Settings.Default.titleWindow;
            mainWindowTitle = MainWindow1.Title; // make a copy
            if (File.Exists(USB_CAN_Interface.Properties.Settings.Default.lastFilePath))
            {
                ReadXml(USB_CAN_Interface.Properties.Settings.Default.lastFilePath);
                MainWindow1.Title = mainWindowTitle + " - " + Path.GetFileName(USB_CAN_Interface.Properties.Settings.Default.lastFilePath);
            }

            CheckBoxBlind.IsChecked = USB_CAN_Interface.Properties.Settings.Default.imBlind;
            ResizeDataGridRx();

            InitPopulateBaudRateListBox();

            ComboBoxRxNode.ItemsSource = Enum.GetNames(typeof(EnumDefines.Nodes));
            ComboBoxTxNode.ItemsSource = Enum.GetNames(typeof(EnumDefines.Nodes));

            // need to remove "_" in the enum
            List<string> txRateList = new List<string>();
            foreach(var en in Enum.GetNames(typeof(EnumDefines.TxRate)))
            {
                txRateList.Add(en.Replace("_", ""));
            }
            ComboBoxEditTxRate.ItemsSource = txRateList;

            sw = new System.Diagnostics.Stopwatch();

            // get checkbox states
            CheckBoxAscii.IsChecked = USB_CAN_Interface.Properties.Settings.Default.ascii;
            CheckBoxNotes.IsChecked = USB_CAN_Interface.Properties.Settings.Default.notes;
            // now format datagrid if needed
            FormatDataGridColumns();

            ProgressBar.Maximum = MAX_ROW_COUNT;

        }
        #endregion

        #region exit program
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var response = System.Windows.MessageBox.Show("Do you really want to exit?", "Exiting...",
                                   MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (response == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                Device.Disconnect(); // disconnet USB device
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
        #endregion

        #region add and edit messages
        /*
         * function: Insert a new row. Routine will go through 
         * all rows for next open key number to use.
         * 
         */
        private void ButtonAddEditTxRow_Click(object sender, RoutedEventArgs e)
        {
            var matchFound = true;
            UInt32 newIndex = 0;
            CanTxData canTxData = new CanTxData();

            // TODO - need to revist this. Forgot about Key order could be sorted out of order.
            // check for available key number
            while (matchFound)
            {
                matchFound = false;
                foreach (var item in dataGridEditTxMessages.Items)
                {
                    var it = item as CanTxData;
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
            canTxData.Key = newIndex;
            dataGridEditTxMessages.Items.Add(canTxData);

            dataGridTx.Items.Add(canTxData); // the Tx dataGrid
         //   dataGridTx.RowHeight = 15;
        }

        private void ButtonDeleteEditTxRow_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridEditTxMessages.SelectedItem != null)
            {
                // TODO - need to find solution to delete selected row, for now using index
                dataGridEditTxMessages.Items.RemoveAt(rowIndexEditTx);
                dataGridTx.Items.RemoveAt(rowIndexEditTx);
            }
        }

        private void ButtonAddEditRxRow_Click(object sender, RoutedEventArgs e)
        {
            var matchFound = true;
            UInt32 newIndex = 0;
            CanRxData canRxData = new CanRxData();

            // check for available key number
            while (matchFound)
            {
                matchFound = false;
                foreach (var item in dataGridEditRxMessages.Items)
                {
                    var it = item as CanRxData;
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
            canRxData.Key = newIndex;
            dataGridEditRxMessages.Items.Add(canRxData);
        }

        private void ButtonDeleteEditRxRow_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridEditRxMessages.SelectedItem != null)
            {
                // TODO - need to find solution to delete selected row, for now using index
                dataGridEditRxMessages.Items.RemoveAt(rowIndexEditRx);
            }
        }
        #endregion

        #region On mouse button up will update TextBoxes from current selected data grid row
        private void DataGridEditRxMessages_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanRxData data = dataGridEditRxMessages.SelectedItem as CanRxData; // grabs the current selected row
            if (data == null) return;
            TextBoxRxDescription.Text = data.Description;
            TextBoxRxArbID.Text = data.ArbID;
            TextBoxRxDLC.Text = data.DLC;
            TextBoxRxByte1.Text = data.Byte1;
            TextBoxRxByte2.Text = data.Byte2;
            TextBoxRxByte3.Text = data.Byte3;
            TextBoxRxByte4.Text = data.Byte4;
            TextBoxRxByte5.Text = data.Byte5;
            TextBoxRxByte6.Text = data.Byte6;
            TextBoxRxByte7.Text = data.Byte7;
            TextBoxRxByte8.Text = data.Byte8;
            ComboBoxRxNode.SelectedIndex = GetComboBoxNodeIndex(data.Node);
            TextBoxRxNotes.Text = data.Notes;
        }

        private void DataGridEditTxMessages_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row
            if (data == null) return;
            TextBoxTxDescription.Text = data.Description;
            TextBoxTxArbID.Text = data.ArbID;
            CheckBoxRemoteTransmit.IsChecked = Convert.ToBoolean(data.RTR);
            TextBoxTxDLC.Text = data.DLC;
            TextBoxTxByte1.Text = data.Byte1;
            TextBoxTxByte2.Text = data.Byte2;
            TextBoxTxByte3.Text = data.Byte3;
            TextBoxTxByte4.Text = data.Byte4;
            TextBoxTxByte5.Text = data.Byte5;
            TextBoxTxByte6.Text = data.Byte6;
            TextBoxTxByte7.Text = data.Byte7;
            TextBoxTxByte8.Text = data.Byte8;
            ComboBoxTxNode.SelectedIndex = GetComboBoxNodeIndex(data.Node);

            ComboBoxEditTxRate.SelectedIndex = GetComboBoxTxRateIndex(data.Rate);

            CheckBoxEditTxAutoTx.IsChecked = data.AutoTx;

            if (CheckBoxRemoteTransmit.IsChecked == false)
            {
                // enable just in case they were disabled by RTR checkbox
                TextBoxTxDLC.IsEnabled = true;
                TextBoxTxByte1.IsEnabled = true;
                TextBoxTxByte2.IsEnabled = true;
                TextBoxTxByte3.IsEnabled = true;
                TextBoxTxByte4.IsEnabled = true;
                TextBoxTxByte5.IsEnabled = true;
                TextBoxTxByte6.IsEnabled = true;
                TextBoxTxByte7.IsEnabled = true;
                TextBoxTxByte8.IsEnabled = true;
            }
            else
            {
                TextBoxTxDLC.IsEnabled = false;
                TextBoxTxByte1.IsEnabled = false;
                TextBoxTxByte2.IsEnabled = false;
                TextBoxTxByte3.IsEnabled = false;
                TextBoxTxByte4.IsEnabled = false;
                TextBoxTxByte5.IsEnabled = false;
                TextBoxTxByte6.IsEnabled = false;
                TextBoxTxByte7.IsEnabled = false;
                TextBoxTxByte8.IsEnabled = false;
            }
        }

        private int GetComboBoxNodeIndex(string name)
        {
            int i = 0;
            foreach (var en in Enum.GetNames(typeof(EnumDefines.Nodes)))
            {
                if (en == name)
                {
                    return i;
                }
                i++;
            }
            return i;
        }

        private int GetComboBoxTxRateIndex(string name)
        {
            int i = 0;
            foreach (var en in Enum.GetNames(typeof(EnumDefines.TxRate)))
            {
                if(en.Replace("_","") == name)
                {
                    return i;
                }
                i++;
            }
            return i;
        }

        #endregion

        #region On Transmit text change from TextBox will update dataGridTx
        private void TextBoxEditMessageTx_TextChanged(object sender, TextChangedEventArgs e)
        {
            CanTxData canTxData = (CanTxData)dataGridEditTxMessages.SelectedItem;

            if (canTxData == null)
            {
                StatusBarStatus.Text = "You need to select a row";
                return;
            }
            else
            {
                StatusBarStatus.Text = "";
            }

            TextBox obj = sender as TextBox;
            string senderName = obj.Name;

            //todo - figure out which text box is changing then edit the correct one below
            switch (senderName)
            {
                case "TextBoxTxDescription":
                    canTxData.Description = TextBoxTxDescription.Text;
                    break;
                case "TextBoxTxArbID":
                    string tempStr = "";
                    var id = GetIs29BitID(TextBoxTxArbID.Text.ToUpper(), ref tempStr);

                    if (id == 1)
                    {
                        canTxData.IDE = "X";
                        StatusBarStatus.Text = "";
                    }
                    else if (id == 0)
                    {
                        canTxData.IDE = "S";
                        StatusBarStatus.Text = "";
                    }
                    else
                    {
                        StatusBarStatus.Text = "ArbID should be between 0x000 - 0x1FFFFFFF";
                        break;
                    }
                    canTxData.ArbID = tempStr;
                    break;
                case "TextBoxTxDLC":
                    canTxData.DLC = TextBoxTxDLC.Text.ToUpper();
                    break;
                case "TextBoxTxByte1":
                    canTxData.Byte1 = TextBoxTxByte1.Text.ToUpper();
                    break;
                case "TextBoxTxByte2":
                    canTxData.Byte2 = TextBoxTxByte2.Text.ToUpper();
                    break;
                case "TextBoxTxByte3":
                    canTxData.Byte3 = TextBoxTxByte3.Text.ToUpper();
                    break;
                case "TextBoxTxByte4":
                    canTxData.Byte4 = TextBoxTxByte4.Text.ToUpper();
                    break;
                case "TextBoxTxByte5":
                    canTxData.Byte5 = TextBoxTxByte5.Text.ToUpper();
                    break;
                case "TextBoxTxByte6":
                    canTxData.Byte6 = TextBoxTxByte6.Text.ToUpper();
                    break;
                case "TextBoxTxByte7":
                    canTxData.Byte7 = TextBoxTxByte7.Text.ToUpper();
                    break;
                case "TextBoxTxByte8":
                    canTxData.Byte8 = TextBoxTxByte8.Text.ToUpper();
                    break;

            }
            dataGridEditTxMessages.Items.Refresh();
        }
        #endregion

        #region On Receive text change from TextBox will update dataGridRx
        private void TextBoxEditMessageRx_TextChanged(object sender, TextChangedEventArgs e)
        {
            CanRxData canRxData = (CanRxData)dataGridEditRxMessages.SelectedItem;

            if (canRxData == null)
            {
                StatusBarStatus.Text = "You need to select a row";
                return;
            }
            else
            {
                StatusBarStatus.Text = "";
            }

            TextBox obj = sender as TextBox;
            string senderName = obj.Name;

            //todo - figure out which text box is changing then edit the correct one below
            switch (senderName)
            {
                case "TextBoxRxDescription":
                    canRxData.Description = TextBoxRxDescription.Text;
                    break;
                case "TextBoxRxArbID":
                    string tempStr = "";
                    var id = GetIs29BitID(TextBoxRxArbID.Text.ToUpper(), ref tempStr);

                    if (id == 1)
                    {
                        canRxData.IDE = "X";
                        StatusBarStatus.Text = "";
                    }
                    else if (id == 0)
                    {
                        canRxData.IDE = "S";
                        StatusBarStatus.Text = "";
                    }
                    else
                    {
                        StatusBarStatus.Text = "ArbID should be between 0x000 - 0x1FFFFFFF";
                        break;
                    }
                    canRxData.ArbID = tempStr;
                    break;
                case "TextBoxRxDLC":
                    canRxData.DLC = TextBoxRxDLC.Text.ToUpper();
                    break;
                case "TextBoxRxByte1":
                    canRxData.Byte1 = TextBoxRxByte1.Text.ToUpper();
                    break;
                case "TextBoxRxByte2":
                    canRxData.Byte2 = TextBoxRxByte2.Text.ToUpper();
                    break;
                case "TextBoxRxByte3":
                    canRxData.Byte3 = TextBoxRxByte3.Text.ToUpper();
                    break;
                case "TextBoxRxByte4":
                    canRxData.Byte4 = TextBoxRxByte4.Text.ToUpper();
                    break;
                case "TextBoxRxByte5":
                    canRxData.Byte5 = TextBoxRxByte5.Text.ToUpper();
                    break;
                case "TextBoxRxByte6":
                    canRxData.Byte6 = TextBoxRxByte6.Text.ToUpper();
                    break;
                case "TextBoxRxByte7":
                    canRxData.Byte7 = TextBoxRxByte7.Text.ToUpper();
                    break;
                case "TextBoxRxByte8":
                    canRxData.Byte8 = TextBoxRxByte8.Text.ToUpper();
                    break;
                case "TextBoxRxNotes":
                    canRxData.Notes = TextBoxRxNotes.Text;
                    break;
            }
            dataGridEditRxMessages.Items.Refresh();
        }
        #endregion

        #region previews mouse left button down for which row index is selected and stores in variable
        // gets the current row index and saves in variable
        private void DataGridEditTxMessages_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;

            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null)
            {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }
            if (dgr == null) { return; }

            rowIndexEditTx = dgr.GetIndex();
         //   StatusBarStatus.Text = rowIndexEditTx.ToString();
        }

        // gets the row index
        private void DataGridEditRxMessages_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;

            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null)
            {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }
            if (dgr == null) { return; }

            rowIndexEditRx = dgr.GetIndex();
         //   StatusBarStatus.Text = rowIndexEditRx.ToString();
        }
        #endregion

        #region preview text input for hex numbers
        /*
 * function: Checks for valid ArbID. Also trims spaces in the ArbID
 * input: the ArbID
 * output: 11bit = 0, 29bit = 1, id is greater than 0x1fffffff = -1
 */
        private int GetIs29BitID(string ArbID, ref string trimmedID)
        {
            trimmedID = Regex.Replace(ArbID, @"\s", "");
            if (trimmedID == "") return -1; // just in case person backspaces
            UInt32 id = Convert.ToUInt32(trimmedID.ToString(), 16);
            if (id > 0x7ff && id < 0x1fffffff)
            {
                return 1;
            }
            else if (id <= 0x7FF)
            {
                return 0;
            }
            return -1;
        }

        private void TextBoxTxDLC_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text); ;
        }

        private void TextBoxRxDLC_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text); ;
        }

        private void TextBoxTx_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int hexNumber;
            e.Handled = !int.TryParse(e.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out hexNumber);
        }

        private void TextBoxRx_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int hexNumber;
            e.Handled = !int.TryParse(e.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out hexNumber);
        }
        #endregion

        #region saves receive data to file
        private void ButtonSaveRxMessages_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog
            {
                DefaultExt = ".csv",
                Filter = "Can Messages (.csv)|*.csv"
            };
            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringBuilder strBuilder = new StringBuilder();
                DateTime localDate = DateTime.Now;

                strBuilder.Append("CAN-X by Karl Yamashita. " + localDate.ToString() + "\n");
                strBuilder.Append("karlyamashita@gmail.com" + "\n\n");

                // build header
                strBuilder.Append("Line" + ", ");
                strBuilder.Append("RxCnt" + ", ");
                strBuilder.Append("TxCnt" + ", ");
                strBuilder.Append("TimeAbs" + ", ");
                strBuilder.Append("Description" + ", ");
                strBuilder.Append("Tx" + ", ");
                strBuilder.Append("IDE" + ", ");
                strBuilder.Append("ArbID" + ", ");
                strBuilder.Append("RTR" + ", ");
                strBuilder.Append("DLC" + ", ");
                strBuilder.Append("Byte1" + ", ");
                strBuilder.Append("Byte2" + ", ");
                strBuilder.Append("Byte3" + ", ");
                strBuilder.Append("Byte4" + ", ");
                strBuilder.Append("Byte5" + ", ");
                strBuilder.Append("Byte6" + ", ");
                strBuilder.Append("Byte7" + ", ");
                strBuilder.Append("Byte8" + ", ");
                strBuilder.Append("\n");

                foreach (var item in masterDataGridRx)
                    //foreach (var item in dataGridRx.Items.OfType<CanRxData>())
                {
                    strBuilder.Append(item.Line + ", ");
                    strBuilder.Append(item.Count + ", ");
                    strBuilder.Append(item.TxCount + ", ");
                    strBuilder.Append(item.TimeAbs + ", ");
                    strBuilder.Append(item.Description + ", ");
                    strBuilder.Append(item.Tx + ", ");
                    strBuilder.Append(item.IDE + ", ");
                    strBuilder.Append("0x" + item.ArbID + ", "); // prevents Excel from using value as exponent
                    strBuilder.Append(item.RTR + ", ");
                    strBuilder.Append(item.DLC + ", ");
                    strBuilder.Append(item.Byte1 + ", ");
                    strBuilder.Append(item.Byte2 + ", ");
                    strBuilder.Append(item.Byte3 + ", ");
                    strBuilder.Append(item.Byte4 + ", ");
                    strBuilder.Append(item.Byte5 + ", ");
                    strBuilder.Append(item.Byte6 + ", ");
                    strBuilder.Append(item.Byte7 + ", ");
                    strBuilder.Append(item.Byte8 + ", ");
                    strBuilder.Append("\n");
                }

                try
                {
                    File.WriteAllText(saveFile.FileName, strBuilder.ToString());
                    string filename = saveFile.FileName;
                    StatusBarStatus.Text = "Successfully saved " + filename;
                }
                catch (IOException)
                {
                    string messageBoxText = "File not accessable! File may be in use.";
                    string dialogTitle = "File Error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Warning;

                    System.Windows.MessageBox.Show(messageBoxText, dialogTitle, button, icon);

                    StatusBarStatus.Text = messageBoxText;
                }
            }
        }
        #endregion

        #region save project
        // TODO - save Edit messages Tx and Rx datagrid to xml file
        private void MenuItemSaveProject_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog
            {
                DefaultExt = ".canx",
                Filter = "CAN-X Project (.canx)|*.canx"
            };
            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!File.Exists(saveFile.FileName))
                {
                    File.Create(saveFile.FileName).Dispose();// create the file then dispose in order to open for writing
                    SaveProjectFiles(saveFile.FileName);
                }
                else
                {
                    SaveProjectFiles(saveFile.FileName);
                }
            }
        }

        private void SaveProjectFiles(string saveFile)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            XmlWriter xmlWriter = XmlWriter.Create(saveFile, settings);

            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("CANX");
            xmlWriter.WriteElementString("Created_by", "CAN-X software by Karl Yamashita on 12/07/2019. (github.com/karlyamashita/CAN-X)");
            xmlWriter.WriteElementString("Project_Filename", Path.GetFileName(saveFile));

            // edit tx messages
            xmlWriter.WriteStartElement("edit_tx_messages");
            foreach (var item in dataGridEditTxMessages.Items.OfType<CanTxData>())
            {
                xmlWriter.WriteStartElement("edit_txMsg");
                xmlWriter.WriteElementString("Key", item.Key.ToString());
                xmlWriter.WriteElementString("Description", item.Description);
                xmlWriter.WriteElementString("AutoTx", item.AutoTx.ToString());
                xmlWriter.WriteElementString("Rate", item.Rate);

                xmlWriter.WriteElementString("IDE", item.IDE);
                xmlWriter.WriteElementString("ArbID", item.ArbID);
                xmlWriter.WriteElementString("RTR", item.RTR.ToString());
                xmlWriter.WriteElementString("DLC", item.DLC);
                xmlWriter.WriteElementString("Byte1", item.Byte1);
                xmlWriter.WriteElementString("Byte2", item.Byte2);
                xmlWriter.WriteElementString("Byte3", item.Byte3);
                xmlWriter.WriteElementString("Byte4", item.Byte4);
                xmlWriter.WriteElementString("Byte5", item.Byte5);
                xmlWriter.WriteElementString("Byte6", item.Byte6);
                xmlWriter.WriteElementString("Byte7", item.Byte7);
                xmlWriter.WriteElementString("Byte8", item.Byte8);
                xmlWriter.WriteElementString("Node", item.Node);
                xmlWriter.WriteElementString("Notes", item.Notes);
                xmlWriter.WriteEndElement();
            }
            // xmlWriter.WriteEndElement();

            // edit rx messages
            xmlWriter.WriteStartElement("edit_rx_messages");
            foreach (var item in dataGridEditRxMessages.Items.OfType<CanRxData>())
            {
                xmlWriter.WriteStartElement("edit_rxMsg");
                xmlWriter.WriteElementString("Key", item.Key.ToString());
                xmlWriter.WriteElementString("Description", item.Description);
                xmlWriter.WriteElementString("IDE", item.IDE);
                xmlWriter.WriteElementString("ArbID", item.ArbID);
                xmlWriter.WriteElementString("DLC", item.DLC);
                xmlWriter.WriteElementString("Byte1", item.Byte1);
                xmlWriter.WriteElementString("Byte2", item.Byte2);
                xmlWriter.WriteElementString("Byte3", item.Byte3);
                xmlWriter.WriteElementString("Byte4", item.Byte4);
                xmlWriter.WriteElementString("Byte5", item.Byte5);
                xmlWriter.WriteElementString("Byte6", item.Byte6);
                xmlWriter.WriteElementString("Byte7", item.Byte7);
                xmlWriter.WriteElementString("Byte8", item.Byte8);
                xmlWriter.WriteElementString("Node", item.Node);
                xmlWriter.WriteElementString("Notes", item.Notes);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            StatusBarStatus.Text = "File saved successfully";
        }
        #endregion

        #region open project
        private void MenuItemOpenProject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog()
            {
                DefaultExt = ".canx",
                Filter = "CAN-X Project (.canx)|*.canx"
            };
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                USB_CAN_Interface.Properties.Settings.Default.lastFilePath = openFile.FileName;
                USB_CAN_Interface.Properties.Settings.Default.Save();
                if (!File.Exists(openFile.FileName))
                {
                    StatusBarStatus.Text = "File does not exist!";
                }
                else
                {
                    ReadXml(openFile.FileName);
                    MainWindow1.Title = mainWindowTitle + " - " + Path.GetFileName(openFile.FileName);
                }
            }
        }

        private void ReadXml(string openFile)
        {
            XmlReader xmlReader = XmlReader.Create(openFile);
            CanRxData canRxData = new CanRxData();
            CanTxData canTxData = new CanTxData();

            var regex = new Regex(@"\r\n?|\n|\t", RegexOptions.Compiled);
            string result = "";

            string dataGridName = "";
            while (xmlReader.Read())
            {
                // Only detect start elements.
                if (xmlReader.IsStartElement())
                {
                    // Get element name and switch on it.
                    switch (xmlReader.Name)
                    {
                        case "CANX":
                            // Detect this element.
                            Console.WriteLine("Start CANX element.");
                            // clear the datagrids
                            while (dataGridEditRxMessages.Items.Count != 0)
                            {
                                dataGridEditRxMessages.Items.RemoveAt(0);
                            }
                            while (dataGridEditTxMessages.Items.Count != 0)
                            {
                                dataGridEditTxMessages.Items.RemoveAt(0);
                            }
                            while (dataGridTx.Items.Count != 0)
                            {
                                dataGridTx.Items.RemoveAt(0);
                            }
                            break;
                        case "edit_tx_messages":
                            dataGridName = "edit_tx_messages";
                            break;
                        case "edit_rx_messages":
                            dataGridName = "edit_rx_messages";
                            break;
                        case "edit_txMsg":

                            break;
                        case "edit_rxMsg":

                            break;
                        case "Key":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Key = Convert.ToUInt32(result);
                            }
                            else
                            {
                                canRxData.Key = Convert.ToUInt32(result);
                            }
                            break;

                        case "AutoTx":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.AutoTx = result == "True";
                            }
                            break;
                        case "Rate":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Rate = result;
                            }
                            break;
                        case "Description":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Description = result;
                            }
                            else
                            {
                                canRxData.Description = result;
                            }
                            break;
                        case "IDE":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.IDE = result;
                            }
                            else
                            {
                                canRxData.IDE = result;
                            }
                            break;
                        case "ArbID":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.ArbID = result;
                            }
                            else
                            {
                                canRxData.ArbID = result;
                            }
                            break;
                        case "RTR":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.RTR = result == "True";
                            }
                            // rx doesn't have RTR
                            break;
                        case "DLC":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.DLC = result;
                            }
                            else
                            {
                                canRxData.DLC = result;
                            }
                            break;
                        case "Byte1":
                            xmlReader.Read();                            
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Byte1 = result;
                            }
                            else
                            {
                                canRxData.Byte1 = result;
                            }
                            break;
                        case "Byte2":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Byte2 = result;
                            }
                            else
                            {
                                canRxData.Byte2 = result;
                            }
                            break;
                        case "Byte3":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Byte3 = result;
                            }
                            else
                            {
                                canRxData.Byte3 = result;
                            }
                            break;
                        case "Byte4":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Byte4 = result;
                            }
                            else
                            {
                                canRxData.Byte4 = result;
                            }
                            break;
                        case "Byte5":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Byte5 = result;
                            }
                            else
                            {
                                canRxData.Byte5 = result;
                            }
                            break;
                        case "Byte6":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Byte6 = result;
                            }
                            else
                            {
                                canRxData.Byte6 = result;
                            }
                            break;
                        case "Byte7":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Byte7 = result;
                            }
                            else
                            {
                                canRxData.Byte7 = result;
                            }
                            break;
                        case "Byte8":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Byte8 = result;
                            }
                            else
                            {
                                canRxData.Byte8 = result;
                            }
                            break;
                        case "Node": // last element so add to datagrid and start new instance of CanRxData/CanTxData
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                if (result == "")
                                {
                                    canTxData.Node = "CAN1"; // default to CAN1
                                }
                                else
                                {
                                    canTxData.Node = result;
                                }
                            }
                            else
                            {
                                if (result == "")
                                {
                                    canRxData.Node = "CAN1"; // default to CAN1
                                }
                                else
                                {
                                    canRxData.Node = result;
                                }
                            }
                            break;
                        case "Notes":
                            xmlReader.Read();
                            result = regex.Replace(xmlReader.Value, String.Empty).Replace(" ", "");
                            if (dataGridName == "edit_tx_messages")
                            {
                                canTxData.Notes = result;

                                dataGridEditTxMessages.Items.Add(canTxData);
                                dataGridTx.Items.Add(canTxData);
                                canTxData = new CanTxData();
                            }
                            else
                            {
                                canRxData.Notes = result;

                                dataGridEditRxMessages.Items.Add(canRxData);
                                canRxData = new CanRxData();
                            }
                            break;
                    }
                }
            }
        }
        #endregion

        #region ComboBox Node selection
        private void ComboBoxTxNode_DropDownClosed(object sender, EventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row
            if (data == null)
            {
                try // this event happens before StatusBarStatus is generated in the window, so it is null. So using try/catch for now.
                {
                    StatusBarStatus.Text = "Select an ArbID first and try selecting the node again";
                }
                catch (NullReferenceException)
                {

                }
                return;
            }
            ComboBox comboBox = (ComboBox)sender;
            data.Node = comboBox.SelectionBoxItem.ToString();
            dataGridEditTxMessages.Items.Refresh();
            // update dataGridTx
            foreach (CanTxData canTxData in dataGridTx.Items)
            {
                if (data.Key == canTxData.Key)
                {
                    canTxData.Node = comboBox.SelectionBoxItem.ToString();
                    dataGridTx.Items.Refresh();
                    break;
                }
            }

            if (comboBox.SelectionBoxItem.ToString() == "SWCAN1" || comboBox.SelectionBoxItem.ToString() == "SWCAN2")
            {
                StackPanelHighVoltage.IsEnabled = true;
            }
            else
            {
                StackPanelHighVoltage.IsEnabled = false;
                CheckBoxHighVoltage.IsChecked = false;
            }
        }

        private void ComboBoxRxNode_DropDownClosed(object sender, EventArgs e)
        {
            CanRxData data = dataGridEditRxMessages.SelectedItem as CanRxData; // grabs the current selected row
            if (data == null)
            {
                try // this event happens before StatusBarStatus is generated in the window, so it is null. So using try/catch for now.
                {
                    StatusBarStatus.Text = "Select an ArbID first and try selecting the node again";
                }
                catch (NullReferenceException)
                {

                }
                return;
            }
            ComboBox comboBox = (ComboBox)sender;
            data.Node = comboBox.SelectionBoxItem.ToString();
            dataGridEditRxMessages.Items.Refresh();

            if (comboBox.SelectionBoxItem.ToString() == "SWCAN1" || comboBox.SelectionBoxItem.ToString() == "SWCAN2")
            {
                StackPanelHighVoltage.IsEnabled = true;
            }
            else
            {
                StackPanelHighVoltage.IsEnabled = false;
                CheckBoxHighVoltage.IsChecked = false;
            }
        }
        #endregion

        #region pause and scroll buttons

        private void ButtonPauseMessages_Click(object sender, RoutedEventArgs e)
        {
            pauseMessagesFlag = (bool)ButtonPauseMessages.IsChecked;
        }

        private void ButtonScrollMessages_Click(object sender, RoutedEventArgs e)
        {
            scrollMessagesFlag = (bool)ButtonScrollMessages.IsChecked;
        }
        #endregion

        #region Checkbox RTR click event
        private void CheckBoxRemoteTransmit_Click(object sender, RoutedEventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row, which you can get the items

            if (data == null)
            {
                StatusBarStatus.Text = "Please select an ArbID to modify";
                return;
            }

            data.RTR = (bool) CheckBoxRemoteTransmit.IsChecked;

            if (data.RTR == true)
            {
                data.DLC = "0";
                data.Byte1 = "";
                data.Byte2 = "";
                data.Byte3 = "";
                data.Byte4 = "";
                data.Byte5 = "";
                data.Byte6 = "";
                data.Byte7 = "";
                data.Byte8 = "";

                TextBoxTxDLC.Text = "0";
                TextBoxTxByte1.Text = "";
                TextBoxTxByte2.Text = "";
                TextBoxTxByte3.Text = "";
                TextBoxTxByte4.Text = "";
                TextBoxTxByte5.Text = "";
                TextBoxTxByte6.Text = "";
                TextBoxTxByte7.Text = "";
                TextBoxTxByte8.Text = "";

                TextBoxTxDLC.IsEnabled = false;
                TextBoxTxByte1.IsEnabled = false;
                TextBoxTxByte2.IsEnabled = false;
                TextBoxTxByte3.IsEnabled = false;
                TextBoxTxByte4.IsEnabled = false;
                TextBoxTxByte5.IsEnabled = false;
                TextBoxTxByte6.IsEnabled = false;
                TextBoxTxByte7.IsEnabled = false;
                TextBoxTxByte8.IsEnabled = false;
            }
            else
            {
                TextBoxTxDLC.IsEnabled = true;
                TextBoxTxByte1.IsEnabled = true;
                TextBoxTxByte2.IsEnabled = true;
                TextBoxTxByte3.IsEnabled = true;
                TextBoxTxByte4.IsEnabled = true;
                TextBoxTxByte5.IsEnabled = true;
                TextBoxTxByte6.IsEnabled = true;
                TextBoxTxByte7.IsEnabled = true;
                TextBoxTxByte8.IsEnabled = true;
            }
            dataGridEditTxMessages.Items.Refresh();
            // now update dataGridTx
            foreach(CanTxData row in dataGridTx.Items)
            {
                if(row.Key == data.Key)
                {
                    row.RTR = data.RTR;
                    dataGridTx.Items.Refresh();
                }
            }
        }
        #endregion

        #region Resize DataGridRx
        private void ResizeDataGridRx()
        {
            if (CheckBoxBlind.IsChecked == true)
            {
                Style rowStyle = new Style();
                rowStyle.TargetType = typeof(DataGridRow);
                rowStyle.Setters.Add(new Setter() { Property = FontSizeProperty, Value = 20D });
                rowStyle.Setters.Add(new Setter() { Property = HeightProperty, Value = 30D });
                dataGridRx.RowStyle = rowStyle;

                USB_CAN_Interface.Properties.Settings.Default.imBlind = true;
                USB_CAN_Interface.Properties.Settings.Default.Save();
            }
            else
            {
                Style rowStyle = new Style();
                rowStyle.TargetType = typeof(DataGridRow);
                rowStyle.Setters.Add(new Setter() { Property = FontSizeProperty, Value = 12D });
                rowStyle.Setters.Add(new Setter() { Property = HeightProperty, Value = 18D });
                dataGridRx.RowStyle = rowStyle;
                // resize columns
                foreach(DataGridColumn c in dataGridRx.Columns)
                {
                    c.Width = 0;
                }
                foreach (DataGridColumn c in dataGridRx.Columns)
                {
                    c.Width = DataGridLength.Auto;
                }
                dataGridRx.UpdateLayout();

                USB_CAN_Interface.Properties.Settings.Default.imBlind = false;
                USB_CAN_Interface.Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region CheckBoxEditTxAutoTx checked
        private void CheckBoxEditTxAutoTx_Checked(object sender, RoutedEventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row, which you can get the items

            if (data == null)
            {
                StatusBarStatus.Text = "Please select an ArbID to modify";
                return;
            }
            // need to update the dataGridTx
            foreach (CanTxData row in dataGridTx.Items)
            {
                if (row.Key == data.Key)
                {
                    data.AutoTx = true;
                    row.AutoTx = true;

                    dataGridEditTxMessages.Items.Refresh();
                    dataGridTx.Items.Refresh();
                }
            }
        }

        private void CheckBoxEditTxAutoTx_Unchecked(object sender, RoutedEventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row, which you can get the items

            if (data == null)
            {
                StatusBarStatus.Text = "Please select an ArbID to modify";
                return;
            }
            // need to update the dataGridTx
            foreach (CanTxData row in dataGridTx.Items)
            {
                if (row.Key == data.Key)
                {
                    dataGridTx.UnselectAll();
                    data.AutoTx = false;
                    row.AutoTx = false;

                    dataGridEditTxMessages.Items.Refresh();
                    dataGridTx.Items.Refresh();
                }
            }
        }
        #endregion

        #region CheckBoxAutoTx Checked
        private void CheckBoxAutoTx_Checked(object sender, RoutedEventArgs e)
        {
            CanTxData data = dataGridTx.SelectedItem as CanTxData; // grabs the current selected row, which you can get the items
            CanTxData dataEdit = dataGridEditTxMessages.SelectedItem as CanTxData;

            if (data == null)
            {
                StatusBarStatus.Text = "Please select an ArbID to modify";
                return;
            }
            // need to update the dataGridEditRxMessages and CheckBoxEditTxAutoTx
            foreach (CanTxData row in dataGridEditTxMessages.Items)
            {
                if (row.Key == data.Key)
                {
                    if (dataEdit != null)
                    {
                        data.AutoTx = true;
                        if (dataEdit.Key == row.Key)
                        {
                            CheckBoxEditTxAutoTx.IsChecked = true;
                        }
                    }
                    row.AutoTx = true;
                    dataGridEditTxMessages.Items.Refresh();
                }
            }
        }

        private void CheckBoxAutoTx_Unchecked(object sender, RoutedEventArgs e)
        {
            CanTxData data = dataGridTx.SelectedItem as CanTxData; // grabs the current selected row, which you can get the items
            CanTxData dataEdit = dataGridEditTxMessages.SelectedItem as CanTxData;

            if (data == null)
            {
                StatusBarStatus.Text = "Please select an ArbID to modify";
                return;
            }
            // need to update the dataGridEditRxMessages and CheckBoxEditTxAutoTx
            foreach (CanTxData row in dataGridEditTxMessages.Items)
            {
                if (row.Key == data.Key)
                {
                    data.AutoTx = false;
                    if (dataEdit != null)
                    {
                        dataGridEditTxMessages.UnselectAll();
                        CheckBoxEditTxAutoTx.IsChecked = false;
                    }
                    row.AutoTx = false;
                    dataGridEditTxMessages.Items.Refresh();
                }
            }
        }
        #endregion

        #region OnComboBoxTxRateTextChanged
        private void OnComboBoxTxRateTextChanged(object sender, EventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row
            if (data == null)
            {
                try // this event happens before StatusBarStatus is generated in the window, so it is null. So using try/catch for now.
                {
                    StatusBarStatus.Text = "Select an ArbID first and try selecting the node again";
                }
                catch (NullReferenceException)
                {

                }
                return;
            }
            data.Rate = ComboBoxEditTxRate.Text;
            dataGridEditTxMessages.Items.Refresh();
            // update dataGridTx
            foreach (CanTxData canTxData in dataGridTx.Items)
            {
                if (data.Key == canTxData.Key)
                {
                    canTxData.Rate = ComboBoxEditTxRate.Text;
                    dataGridTx.Items.Refresh();
                    break;
                }
            }
        }
        #endregion

        #region ToggleButtonAutoTx click
        private void ToggleButtonAutoTx_Click(object sender, RoutedEventArgs e)
        {
            if (toggleButtonAutoTx.IsChecked == true)
            {
                if (!Device.IsDeviceConnected)
                {
                    StatusBarStatus.Text = "Device Not Connected";
                    toggleButtonAutoTx.IsChecked = false;
                    return;
                }

                if (threadAutoTx == null)
                {
                    threadAutoTx = new Thread(TxSendThread);
                    threadAutoTx.Start();
                    sw.Start();
                }                
            //    StatusBarStatus.Text = "Started";
            }
            else
            {                
                threadAutoTx.Abort();
                threadAutoTx = null;
                sw.Stop();
            //    StatusBarStatus.Text = "Stopped";
            }
        }

        public void TxSendThread(object state)
        {
            int Tick = 10;
            int Sleep = 1;
            long OldElapsedMilliseconds = 0;
            CanTxData canTxData = null;

            while (sw.IsRunning && Device.IsDeviceConnected)
            {
                long ElapsedMilliseconds = sw.ElapsedMilliseconds;
                long mod = (ElapsedMilliseconds % Tick);
                if (OldElapsedMilliseconds != ElapsedMilliseconds && (mod == 0 || ElapsedMilliseconds > Tick))
                {
                    foreach (CanTxData row in dataGridTx.Items)
                    {
                        if (row.AutoTx == true)
                        {
                            if (++row.RateTimer >= (Convert.ToUInt32(row.Rate) /10))
                            {
                                row.RateTimer = 0;
                                canTxData = new CanTxData(row);
                                SendCanData(ref canTxData);
                            }
                        }
                        else
                        {
                            row.RateTimer = 0;
                        }
                    }
                    //-----------------Restart----------------Start
                    OldElapsedMilliseconds = ElapsedMilliseconds;
                    OldElapsedMilliseconds = 0;
                    sw.Reset();
                    sw.Start();
                    System.Threading.Thread.Sleep(Sleep);
                    //-----------------Restart----------------End
                }
            }
        }
        #endregion

        #region MenuItemSave
        private void MenuItemSaveRx_Click_1(object sender, RoutedEventArgs e)
        {
            // StatusBarStatus.Text = "Save to Rx";
            CanRxData data = dataGridRx.SelectedItem as CanRxData; // grabs the current selected row, which you can get the items
            if (data == null)
            {
                StatusBarStatus.Text = "Please select an ArbID to save";
                return;
            }

            // find next key number to use
            ulong highKey = 0;
            foreach (CanRxData item in dataGridEditRxMessages.Items)
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
            dataGridEditRxMessages.Items.Add(canRxData);
        }

        private void MenuItemSaveTx_Click(object sender, RoutedEventArgs e)
        {
            // StatusBarStatus.Text = "Save to Tx";
            CanRxData data = dataGridRx.SelectedItem as CanRxData; // grabs the current selected row, which you can get the items
            if (data == null)
            {
                StatusBarStatus.Text = "Please select an ArbID to save";
                return;
            }

            // find next key number to use
            ulong highKey = 0;
            foreach(CanTxData item in dataGridEditTxMessages.Items)
            {
                if(item.Key > highKey)
                {
                    highKey = item.Key;
                }
            }

            // copy selected message to new object
            CanTxData canTxData = new CanTxData(data);
            // assign new key number
            canTxData.Key = highKey + 1;
            // add to message editor Tx datagrid
            dataGridEditTxMessages.Items.Add(canTxData);
            // add to main Tx datagrid
            dataGridTx.Items.Add(canTxData);
        }
        #endregion

        #region CheckBox Blind, Ascii and Notes
        private void CheckBoxBlind_Click(object sender, RoutedEventArgs e)
        {
            USB_CAN_Interface.Properties.Settings.Default.imBlind = (bool)CheckBoxBlind.IsChecked;
            USB_CAN_Interface.Properties.Settings.Default.Save();
            ResizeDataGridRx();
        }

        private void CheckBoxAscii_Click(object sender, RoutedEventArgs e)
        {
            USB_CAN_Interface.Properties.Settings.Default.ascii = (bool) CheckBoxAscii.IsChecked;
            USB_CAN_Interface.Properties.Settings.Default.Save();
            FormatDataGridColumns();
        }

        private void CheckBoxNotes_Click(object sender, RoutedEventArgs e)
        {
            USB_CAN_Interface.Properties.Settings.Default.notes = (bool)CheckBoxNotes.IsChecked;
            USB_CAN_Interface.Properties.Settings.Default.Save();
            FormatDataGridColumns();
        }

        private void FormatDataGridColumns()
        {
            if (CheckBoxAscii.IsChecked == true)
            {
                dataGridRx.Columns[20].Visibility = Visibility.Visible;
            }
            else
            {
                dataGridRx.Columns[20].Visibility = Visibility.Hidden;
            }

            if (CheckBoxNotes.IsChecked == true)
            {
                dataGridRx.Columns[21].Visibility = Visibility.Visible;
            }
            else
            {
                dataGridRx.Columns[21].Visibility = Visibility.Hidden;
            }
        }
        #endregion

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            CanRxData canRxData = new CanRxData();

            canRxData.Byte1 = "AA";
            canRxData.Byte2 = "BB";
            canRxData.Byte3 = "CC";

            Values.Add(canRxData);

            Debug.WriteLine("count: " + Values.Count);
            foreach (CanRxData canrx in Values)
            {
                Debug.WriteLine("{0} {1} {2}", canrx.Byte1, canrx.Byte2, canrx.Byte3);
            }         
        }
    }
}

