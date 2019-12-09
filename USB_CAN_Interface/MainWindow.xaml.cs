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

namespace CAN_X_CAN_Analyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
        const byte COMMAND_BAUD = 0x95; // data: 32bit CAN_BTC value
        const byte COMAAND_CAN_MODE = 0xA0; // data: normal=0, listen=1

        const byte COMMAND_ENABLE_MESSAGES = 0xB0; // enable hardware to send messages on USB data
        const byte COMMAND_DISABLE_MESSAGES = 0xB1; // disable hardware from sending messages on USB data

        const byte COMMAND_INFO = 0x90; // get information from hardware, fw version, BTC value, type hardware
        const byte COMMAND_CAN_BTR = 0x91; // the CAN_BTC value from interface
        const byte COMMAND_VERSION = 0x92;
        const byte COMMAND_HARDWARE = 0x93; // 

        // const defines
        const byte CAN_STD_ID = 0x00;
        const byte CAN_EXT_ID = 0x04;

        // nodes
        const byte CAN1_NODE = 0;
        const byte CAN2_NODE = 1;
        const byte SWCAN1_NODE = 2;
        const byte LSFTCAN1_NODE = 3;
        const byte LIN1_NODE = 4;
        const byte ETH1_NODE = 5;
        const byte SWCAN2_NODE = 6;
        const byte LSFTCAN2_NODE = 7;


        const int DATA_SIZE = 20;
        const int BUFF_SIZE = 64;

        const int MAX_ROW_COUNT = 100000; // how many lines to receive. This is used for the progress as well
        #endregion

        #region variables
        // arrays, variables, objects
        public static UsbHidDevice Device;

        UInt32 lineCount = 1;

        // temporary CAN data to populate Tx DataGrid.
        CanTxData canData1 = new CanTxData();
        CanTxData canData2 = new CanTxData();
        CanTxData canData3 = new CanTxData();
        CanTxData canData4 = new CanTxData();
        CanTxData canData5 = new CanTxData();
        CanTxData canData6 = new CanTxData();

        BindingList<CanTxData> listCanTxData = new BindingList<CanTxData>();

        List<CAN_BaudRate> baudRateList = new List<CAN_BaudRate>();

        public delegate void MessageParse(byte[] data);
        public delegate void SendMessage();

        int rowIndexEditTx = 0;
        int rowIndexEditRx = 0;

        bool pauseMessagesFlag = false;
        bool scrollMessagesFlag = true;
       

        #endregion

        #region MainWindow
        public MainWindow()
        {
            InitializeComponent();

            // my init routines
            InitUsbDevice();
            InitPopulateBaudRateListBox();
        }
        #endregion

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
            MessageParse msg = new MessageParse(ParseUsbData);
            this.Dispatcher.BeginInvoke(msg, new object[] { data });
        }

        private void SendButtonClicked()
        {

        }
        #endregion

        #region parse the USB data received. This is running on a thread
        public void ParseUsbData(byte[] data)
        {
            switch (data[1])
            {
                case COMMAND_MESSAGE:
                    ParseDeviceCAN_Message(data);
                    break;
                case COMMAND_BAUD:

                    break;
                case COMAAND_CAN_MODE:

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
            }
        }
        #endregion

        #region get and show string from data
        private void ShowString(byte command, byte[] data)
        {
            // todo - show the text sent by the interface. Need to figure out where to show. Maybe new TextBox or Lable.
            switch(command)
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
            byte[] temp = new byte[data.Length];

            while (data[i+1] != '\0') // index 1 is command
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
            btrValue = (UInt32) (data[2] << 24 | data[3] << 16 | data[4] << 8 | data[5]);

            if((btrValue >> 31 & 0x1) == 1)
            {
                CheckBoxListenOnly.IsChecked = true;
            }

            TextBoxBtrValue.Text = "0x" + btrValue.ToString("X8");

            int i = 0;
            foreach(var baud in baudRateList)
            {
                if(baud.value == TextBoxBtrValue.Text)
                {
                    ComboBoxBaudRate.SelectedIndex = i;
                    return;
                }
                i++;
            }
        }
        #endregion

        #region Parse device CAN message
        private void ParseDeviceCAN_Message(byte[] data)
        {
            // todo - copy data to struct to use member access instead of figureing out what index of data is.
            DateTime now = DateTime.Now;
            CanRxData canDataRx = new CanRxData
            {
                Line = lineCount++
            };
            //canDataRx.TimeAbs = now.ToString("MM/dd/yyyy - HH:mm:ss.ffff");
            canDataRx.TimeAbs = now.ToString("HH:mm:ss.ffff");

            Array.Copy(data, 1, data, 0, DATA_SIZE); // shift command to index 0

            //data[0] is command
            if (data[1] == 0)
            {
                canDataRx.IDE = "S";
            }
            else
            {
                canDataRx.IDE = "X";
            }

            //data[2] is RTR
            if ((data[2] & 0x01) == 0)
            {
                canDataRx.RTR = "false";
            }
            else
            {
                canDataRx.RTR = "true";
            }
            // data[3] is n/a

            UInt32 id = (UInt32)(data[4] | data[5] << 8 | data[6] << 16 | data[7] << 24);

            canDataRx.ArbID = Convert.ToString(id, 16).ToUpper();

            // parse ID and compare to receive messages editor. Return string description if available
            foreach(CanRxData row in dataGridEditRxMessages.Items)
            {
                if(row.ArbID == canDataRx.ArbID)
                {
                    canDataRx.Description = row.Description;
                }
            }

            canDataRx.DLC = data[8].ToString(); // data[8] is DLC

            if (data[8] >= 1)
            {
                canDataRx.Byte1 = data[10].ToString("X2");
            }
            if (data[8] >= 2)
            {
                canDataRx.Byte2 = data[11].ToString("X2");
            }
            if (data[8] >= 3)
            {
                canDataRx.Byte3 = data[12].ToString("X2");
            }
            if (data[8] >= 4)
            {
                canDataRx.Byte4 = data[13].ToString("X2");
            }
            if (data[8] >= 5)
            {
                canDataRx.Byte5 = data[14].ToString("X2");
            }
            if (data[8] >= 6)
            {
                canDataRx.Byte6 = data[15].ToString("X2");
            }
            if (data[8] >= 7)
            {
                canDataRx.Byte7 = data[16].ToString("X2");
            }
            if (data[8] == 8)
            {
                canDataRx.Byte8 = data[17].ToString("X2");
            }

            // todo - need to make struct for RTR to get node info
            if((data[2] >> 2 & 0x04) == CAN1_NODE)
            {
                canDataRx.Node = "CAN1";
            }

            // add formatted data to data grid
            AddToDataGrid(canDataRx); 

            // update progress bar
            var count = dataGridRx.Items.Count;
            ProgressBar.Value = count;
            // remove data from datagrid if we reach max amount of rows
            if(count>= MAX_ROW_COUNT)
            {
                dataGridRx.Items.RemoveAt(0);                 
            }
        }
        #endregion

        #region Add formatted data to datagrid and scrolls
        private void AddToDataGrid(CanRxData canRxData)
        {
            dataGridRx.Items.Add(canRxData);
            // scroll to bottom
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

        #region Button event to send CAN messages and to update DataGrid. Starts delegate
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

            // get the current selected row data
            CanTxData canTxdata = dataGridTx.SelectedItem as CanTxData;
            // now update datagrid
            SendCanData(ref canTxdata);

            // formatting canRxData with canTxData and adding line count, time
            CanRxData canRxData = new CanRxData();
            canRxData.Line = lineCount++;
            canRxData.TimeAbs = now.ToString("HH:mm:ss.ffff");
            canRxData.Tx = true;
            canRxData.IDE = canTxdata.IDE;
            canRxData.Description = canTxdata.Description;
            canRxData.ArbID = canTxdata.ArbID;
            canRxData.DLC = canTxdata.DLC;
            canRxData.Byte1 = canTxdata.Byte1;
            canRxData.Byte2 = canTxdata.Byte2;
            canRxData.Byte3 = canTxdata.Byte3;
            canRxData.Byte4 = canTxdata.Byte4;
            canRxData.Byte5 = canTxdata.Byte5;
            canRxData.Byte6 = canTxdata.Byte6;
            canRxData.Byte7 = canTxdata.Byte7;
            canRxData.Byte8 = canTxdata.Byte8;
            canRxData.Node = canTxdata.Node;

            // add formatted data to datagrid
            AddToDataGrid(canRxData);         
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

        #region temporary fill EditTx Datgrid, just to see what Tx Grid looks like
        private void InitAddTxMessagesToDataGrid()
        {
            // todo - add feature to add CAN messages to Tx window and Tx Button. For now just populate DataGrid with below data

            canData1.Key = 0;
            canData1.Description = "System_Power_Mode";
            canData1.IDE = "S"; // CAN_STD_ID = 0, CAN_EXT_ID = 4
            canData1.ArbID = "040";
            canData1.RTR = "";
            canData1.DLC = "8";
            canData1.Byte1 = "01";
            canData1.Byte2 = "02";
            canData1.Byte3 = "03";
            canData1.Byte3 = "04";
            canData1.Byte3 = "05";
            canData1.Byte3 = "06";
            canData1.Byte3 = "07";
            canData1.Byte3 = "08";

            dataGridTx.Items.Add(canData1);

            canData2.Key = 1;
            canData2.Description = "Amplifier";
            canData2.IDE = "S"; // CAN_STD_ID = 0, CAN_EXT_ID = 4
            canData2.ArbID = "244";
            canData2.RTR = "";
            canData2.DLC = "3";
            canData2.Byte1 = "01";
            canData2.Byte2 = "44";
            canData2.Byte3 = "FF";

            dataGridTx.Items.Add(canData2);

            dataGridTx.RowHeight = 20;

            dataGridEditTxMessages.Items.Add(canData1);
            dataGridEditTxMessages.Items.Add(canData2);

            dataGridEditTxMessages.RowHeight = 20;
       
        }
        #endregion\

        #region get the connected device version and hardware type
        private void GetInfo()
        {
            byte[] tmp_buf = new byte[DATA_SIZE]; // command + 63 byte = 64 bytes

            var command = new CommandMessage(COMMAND_INFO, tmp_buf); // no data to send but need array
            Device.SendMessage(command);
        }
        #endregion

        #region Send CAN message to device over USB
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
            if (canData.RTR != null && canData.RTR != "")
            {
                usbPacket[1] = Convert.ToByte(canData.RTR); // RTR, Node
            }

            // Node
            if (canData.Node != null && canData.Node != "")
            {
                switch (canData.Node)
                {
                    case "CAN1":
                        usbPacket[1] = (byte)(usbPacket[1] | (CAN1_NODE << 2));
                        break;
                    case "CAN2":
                        usbPacket[1] = (byte)(usbPacket[1] | (CAN2_NODE << 2));
                        break;
                    case "SWCAN1":
                        usbPacket[1] = (byte)(usbPacket[1] | (SWCAN1_NODE << 2));
                        break;
                    case "LSFTCAN1":
                        usbPacket[1] = (byte)(usbPacket[1] | (LSFTCAN1_NODE << 2));
                        break;
                    case "LIN1":
                        usbPacket[1] = (byte)(usbPacket[1] | (LIN1_NODE << 2));
                        break;
                }
            }
            else // set to default CAN1
            {
                usbPacket[1] = (byte)(usbPacket[1] | (CAN1_NODE << 2));
            }

            // tmp_buf[2] n/a. This aligns the 32 bit ArbID in the device's USB message structure

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
            if(canData.DLC != null && canData.DLC != "")
            {
                usbPacket[7] = Convert.ToByte(canData.DLC);             
            }

            // data bytes
            if(canData.Byte1 != null && canData.Byte1 != "")
            {
                usbPacket[8] = Convert.ToByte(canData.Byte1, 16);
            }

            if(canData.Byte2 != null && canData.Byte2 != "")
            {
                usbPacket[9] = Convert.ToByte(canData.Byte2, 16);
            }

            if(canData.Byte3 != null && canData.Byte3 != "")
            {
                usbPacket[10] = Convert.ToByte(canData.Byte3, 16);
            }

            if(canData.Byte4 != null && canData.Byte4 != "")
            {
                usbPacket[11] = Convert.ToByte(canData.Byte4, 16);
            }

            if(canData.Byte5 != null && canData.Byte5 != "")
            {
                usbPacket[12] = Convert.ToByte(canData.Byte5, 16);
            }

            if(canData.Byte6 != null && canData.Byte6 != "")
            {
                usbPacket[13] = Convert.ToByte(canData.Byte6, 16);
            }

            if(canData.Byte7 != null && canData.Byte7 != "")
            {
                usbPacket[14] = Convert.ToByte(canData.Byte7, 16);
            }

            if(canData.Byte8 != null && canData.Byte8 != "")
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
            while(dataGridRx.Items.Count != 0)
            {
                dataGridRx.Items.RemoveAt(0);
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

        #region sends new baud rate to device. Todo - this modify CAN1, need to make another button to modify CAN2
        private void ButtonBtrValue_Click(object sender, RoutedEventArgs e)
        {
            if (!Device.IsDeviceConnected)
            {
                StatusBarStatus.Text = "Device Not Connected";
                return;
            }

            byte[] tmp_buf = new byte[DATA_SIZE];
            string myString = TextBoxBtrValue.Text;
            UInt32 btrValue = Convert.ToUInt32(myString, 16);

            tmp_buf[0] = (byte)(btrValue >> 24);
            tmp_buf[1] = (byte)(btrValue >> 16);
            tmp_buf[2] = (byte)(btrValue >> 8);
            tmp_buf[3] = (byte)(btrValue);
            if (CheckBoxListenOnly.IsChecked == true)
            {
                tmp_buf[0] = (byte) (tmp_buf[0] | 0x80);// bit 31 is Normal=0, Silent = 1. Bit 30 is Loopback mode, disable = 0, loopback enabled = 1
            }

            tmp_buf[4] = CAN1_NODE; // CAN1

            StatusBarStatus.Text = "Sending BTR Value";
            var command = new CommandMessage(COMMAND_BAUD, tmp_buf);
            Device.SendMessage(command);
        }
        #endregion

        #region baud rate init and notification change
        private void InitPopulateBaudRateListBox()
        {
            CAN_BaudRate baud_1000k = new CAN_BaudRate(); // 1 mbits 0x001a0002
            CAN_BaudRate baud_500k = new CAN_BaudRate(); // 500kbits 0x001a0005
            CAN_BaudRate baud_250k = new CAN_BaudRate(); // 250k 0x001a000b
            CAN_BaudRate baud_125k = new CAN_BaudRate(); // 125k 0x001a0017
            CAN_BaudRate baud_100k = new CAN_BaudRate(); // 100k 0x001a001d
            CAN_BaudRate baud_83_333k = new CAN_BaudRate(); // 83.333k 0x001a0023
            CAN_BaudRate baud_50k = new CAN_BaudRate(); // 50k 0x001a003b
            CAN_BaudRate baud_33_333k = new CAN_BaudRate(); // 33.333k 0x001a0059

            baud_1000k.baud = "1000k";
            baud_1000k.value = "0x001A0002";

            baud_500k.baud = "500k";
            baud_500k.value = "0x001A0005";

            baud_250k.baud = "250k";
            baud_250k.value = "0x001A000B";

            baud_125k.baud = "125k";
            baud_125k.value = "0x001A0017";

            baud_100k.baud = "100k";
            baud_100k.value = "0x001A001D";

            baud_83_333k.baud = "83.333k";
            baud_83_333k.value = "0x001A0023";

            baud_50k.baud = "50k";
            baud_50k.value = "0x001A003b";

            baud_33_333k.baud = "33.333k";
            baud_33_333k.value = "0x001A0059";

            baudRateList.Add(baud_1000k);
            baudRateList.Add(baud_500k);
            baudRateList.Add(baud_250k);
            baudRateList.Add(baud_125k);
            baudRateList.Add(baud_100k);
            baudRateList.Add(baud_83_333k);
            baudRateList.Add(baud_50k);
            baudRateList.Add(baud_33_333k);

            foreach (var item in baudRateList)
            {
                ComboBoxBaudRate.Items.Add(item.baud);
            }
            ComboBoxBaudRate.SelectedIndex = 1;
        }

        private void ComboBoxBaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int indexItem = 0;
            indexItem = ComboBoxBaudRate.SelectedIndex;
            TextBoxBtrValue.Text = baudRateList[indexItem].value;
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

            // check for available key number
            while (matchFound) { 
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
            dataGridTx.RowHeight = 15;
        }

        private void ButtonDeleteEditTxRow_Click(object sender, RoutedEventArgs e)
        {
            if(dataGridEditTxMessages.SelectedItem != null)
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
            ComboBoxRxNode.SelectedIndex = GetComboBoxIndex(data.Node);
        }

        private void DataGridEditTxMessages_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {           
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row
            if (data == null) return;
            TextBoxTxDescription.Text = data.Description;
            TextBoxTxArbID.Text = data.ArbID;
            TextBoxTxDLC.Text = data.DLC;
            TextBoxTxByte1.Text = data.Byte1;
            TextBoxTxByte2.Text = data.Byte2;
            TextBoxTxByte3.Text = data.Byte3;
            TextBoxTxByte4.Text = data.Byte4;
            TextBoxTxByte5.Text = data.Byte5;
            TextBoxTxByte6.Text = data.Byte6;
            TextBoxTxByte7.Text = data.Byte7;
            TextBoxTxByte8.Text = data.Byte8;
            ComboBoxTxNode.SelectedIndex = GetComboBoxIndex(data.Node);
        }

        private int GetComboBoxIndex(string name)
        {
            int value = 0;
            switch(name)
            {
                case "CAN1":
                    value = 0;
                    break;
                case "CAN2":
                    value = 1;
                    break;
                case "SWCAN1":
                    value = 2;
                    break;
                case "LSFTCAN1":
                    value = 3;
                    break;
                case "LIN1":
                    value = 4;
                    break;
                case "ETH1":
                    value = 5;
                    break;
                case "SWCAN2":
                    value = 6;
                    break;
                case "LSFTCAN2":
                    value = 7;
                    break;
            }
            return value;
        }

        // Transmit
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
            switch(senderName)
            {
                case "TextBoxTxDescription":
                    canTxData.Description = TextBoxTxDescription.Text;
                    break;
                case "TextBoxTxArbID":
                    string tempStr = "";                  
                    var id = GetIs29BitID(TextBoxTxArbID.Text.ToUpper(), ref tempStr);
                   
                    if (id == 1) {
                        canTxData.IDE = "X";
                        StatusBarStatus.Text = "";
                    }
                    else if(id == 0)
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

        // Receive
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
            }
            dataGridEditRxMessages.Items.Refresh();
        }

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
            StatusBarStatus.Text = rowIndexEditTx.ToString();
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
            StatusBarStatus.Text = rowIndexEditRx.ToString();
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

        /*
         * function: Checks for valid ArbID. Also trims spaces in the ArbID
         * input: the ArbID
         * output: 11bit = 0, 29bit = 1, id is greater than 0x1fffffff = -1
         */
        private int GetIs29BitID(string ArbID, ref string trimmedID)
        {
            trimmedID = Regex.Replace(ArbID, @"\s", "");
            if (trimmedID == "") return - 1; // just in case person backspaces
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
                strBuilder.Append("TimeAbs" + ", ");
                strBuilder.Append("Description" + ", ");
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

                foreach (var item in dataGridRx.Items.OfType<CanRxData>())
                {
                    strBuilder.Append(item.Line + ", ");
                    strBuilder.Append(item.TimeAbs + ", ");
                    strBuilder.Append(item.Description + ", ");
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
                if(!File.Exists(saveFile.FileName))
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

            // rx received messages
            /*
            xmlWriter.WriteStartElement("rx_messages");
            foreach (var item in dataGridRx.Items.OfType<CanRxData>())
            {
                xmlWriter.WriteStartElement("rxMsg");
                xmlWriter.WriteElementString("Line",item.Line.ToString());
                xmlWriter.WriteElementString("TimeAbs", item.TimeAbs);
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
                xmlWriter.WriteEndElement();
            }
            */
            // xmlWriter.WriteEndElement();

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
                xmlWriter.WriteElementString("IDE", item.IDE);
                xmlWriter.WriteElementString("ArbID", item.ArbID);
                xmlWriter.WriteElementString("RTR", item.RTR);
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
                                canTxData.RTR = result;
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
                            //var regex = new Regex(@"\r\n?|\n|\t", RegexOptions.Compiled);
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
                                canTxData.Node = result;
                                dataGridEditTxMessages.Items.Add(canTxData);
                                dataGridTx.Items.Add(canTxData);
                                dataGridTx.RowHeight = 20;
                                canTxData = new CanTxData();
                            }
                            else
                            {
                                canRxData.Node = result;
                                dataGridEditRxMessages.Items.Add(canRxData);
                                dataGridRx.RowHeight = 20;
                                canRxData = new CanRxData();
                            }                          
                            break;
                    }
                }
            }
        }
        #endregion

        private UInt32 ConvertHexStrToInt(string str)
        {
            UInt32 intValue = (UInt32)(Convert.ToInt32(str, 16));
            return intValue;
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonPauseMessages_Click(object sender, RoutedEventArgs e)
        {
            pauseMessagesFlag = !pauseMessagesFlag;
        }

        private void ButtonScrollMessages_Click(object sender, RoutedEventArgs e)
        {
            scrollMessagesFlag = !scrollMessagesFlag;
        }

        private void DataGridRx_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            return;
            // Todo - open window only if Key column is clicked
            CanRxData data = dataGridRx.SelectedItem as CanRxData; // grabs the current selected row, which you can get the items



            DataGridRow dgr = null;
            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null)
            {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }
            if (dgr == null)
            {
                return;
            }




            int columnIndex = dataGridRx.CurrentColumn.DisplayIndex;


            StatusBarStatus.Text = "row: " + dgr.GetIndex().ToString() + " col: " + columnIndex;

            if(columnIndex != 0)
            {
                return;
            }

            Window2CopyMsg window2 = new Window2CopyMsg();
            window2.Show();
        }

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

        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(USB_CAN_Interface.Properties.Settings.Default.lastFilePath))
            {
                return;
            }
            else
            {
                ReadXml(USB_CAN_Interface.Properties.Settings.Default.lastFilePath);
            }
        }
    }
}
