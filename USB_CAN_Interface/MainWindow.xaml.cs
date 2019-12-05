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

namespace CAN_X_CAN_Analyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

        const int DATA_SIZE = 20;
        const int BUFF_SIZE = 64;

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

        List<CAN_BaudRate> baudRateList = new List<CAN_BaudRate>();

        public delegate void MessageParse(byte[] data);

        public MainWindow()
        {
            InitializeComponent();

            // my init routines
            InitUsbDevice();
            InitAddTxMessagesToDataGrid();

            InitPopulateBaudRateListBox();
        }

        private void InitUsbDevice()
        {
            Device = new UsbHidDevice(0x0483, 0x5750);
            Device.OnConnected += DeviceOnConnected;
            Device.OnDisConnected += DeviceOnDisConnected;
            Device.DataReceived += DeviceDataReceived;
        }
      

        // delegate to receive USB data
        private void DeviceDataReceived(byte[] data)
        {        
            MessageParse msg = new MessageParse(ParseUsbData);
            this.Dispatcher.BeginInvoke(msg, new object[] { data });
        }

        // parse the data
        public void ParseUsbData(byte[] data)
        {
            switch (data[1])
            {
                case COMMAND_MESSAGE:
                    AddToDataGrid(data);
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

        private void ShowBTC_VALUE(byte[] data)
        {
            // todo - parse the BTC_VALUE and show in TextBoxBtcValue. Then set index in the ComboBoxBaudRate
            UInt32 btrValue = 0;
            btrValue = (UInt32) (data[2] << 24 | data[3] << 16 | data[4] << 8 | data[5]);

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

        // add CAN messages to DataGrid
        private void AddToDataGrid(byte[] data)
        {
            CanRxData canDataRx = new CanRxData
            {
                Line = lineCount++
            };

            DateTime now = DateTime.Now;

            canDataRx.TimeAbs = now.ToString("o");

            //data[1] is command
            if (data[2] == 0)
            {
                canDataRx.IDE = "S";
            }
            else
            {
                canDataRx.IDE = "X";
            }

            UInt32 id = (UInt32)(data[3] << 24 | data[4] << 16 | data[5] << 8 | data[6]);
            canDataRx.ArbID = Convert.ToString(id, 16).ToUpper();

            // todo parse ID and compare to database. Return string description if available
            if (id == 0x10242040)
            {
                canDataRx.Description = "PowerMode";
            }
            if (id == 0x100)
            {
                canDataRx.Description = "Tech 2";
            }
            if (id == 0x10288040)
            {
                canDataRx.Description = "Remote Transmit";
            }

            //data[7] is RTR
            if(data[7] == 0)
            {
                canDataRx.RTR = "";
            }
            else
            {
                canDataRx.RTR = "R";
            }

            canDataRx.DLC = data[8]; // data[8] is DLC

            if (data[8] >= 1)
            {
                canDataRx.Byte1 = data[9].ToString("X2");
            }
            if (data[8] >= 2)
            {
                canDataRx.Byte2 = data[10].ToString("X2");
            }
            if (data[8] >= 3)
            {
                canDataRx.Byte3 = data[11].ToString("X2");
            }
            if (data[8] >= 4)
            {
                canDataRx.Byte4 = data[12].ToString("X2");
            }
            if (data[8] >= 5)
            {
                canDataRx.Byte5 = data[13].ToString("X2");
            }
            if (data[8] >= 6)
            {
                canDataRx.Byte6 = data[14].ToString("X2");
            }
            if (data[8] >= 7)
            {
                canDataRx.Byte7 = data[15].ToString("X2");
            }
            if (data[8] == 8)
            {
                canDataRx.Byte8 = data[16].ToString("X2");
            }

            dataGridRx.Items.Add(canDataRx); // add new row and populate with new data

            // scroll to bottom
            if (dataGridRx.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(dataGridRx, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }

            var count = dataGridRx.Items.Count;

            ProgressBar.Value = count;
            if(count>=10000)
            {
                dataGridRx.Items.RemoveAt(0);                 
            }

        }

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

        //temporary, just to see what Tx Grid looks like
        private void InitAddTxMessagesToDataGrid()
        {
            // todo - add feature to add CAN messages to Tx window and Tx Button. For now just populate DataGrid with below data

            canData1.Description = "System_Power_Mode";
            //canData1.Rate = 0.100;
            canData1.IDE = "X"; // CAN_STD_ID = 0, CAN_EXT_ID = 4
            canData1.ArbID = "10002040";
            canData1.RTR = "";
            canData1.DLC = 3;
            canData1.Byte1 = "12";
            canData1.Byte2 = "00";
            canData1.Byte3 = "00";

            dataGridTx.Items.Add(canData1);

            canData4.Description = "Audio_Master_Arbitration_Command";
            //canData4.Rate = 0.100;
            canData4.IDE = "X";
            canData4.ArbID = "1028A080";
            canData4.RTR = "";
            canData4.DLC = 2;
            canData4.Byte1 = "00";
            canData4.Byte2 = "00";

            dataGridTx.Items.Add(canData4);

            canData5.Description = "Audio_Source_Status";
            //canData5.Rate = 0.100;
            canData5.IDE = "X";
            canData5.ArbID = "1031C097";
            canData5.RTR = "";
            canData5.DLC = 2;
            canData5.Byte1 = "00";
            canData5.Byte2 = "00";

            dataGridTx.Items.Add(canData5);

            canData6.Description = "VNMF_IRC_CHM";
            //canData6.Rate = 0.100;
            canData6.IDE = "S";
            canData6.ArbID = "624";
            canData6.RTR = "";
            canData6.DLC = 0;

            dataGridTx.Items.Add(canData6);
            
            dataGridTx.CanUserSortColumns = false;          
        }

        private void ButtonTxMessage_Click(object sender, RoutedEventArgs e)
        {
            if (!Device.IsDeviceConnected)
            {
                StatusBarStatus.Text = "Device Not Connected";
                return;
            }

            CanTxData data = dataGridTx.SelectedItem as CanTxData; // grabs the current selected row

            // todo - start async task to send data

            CanTxData canData = new CanTxData();
            if(data.IDE == "S")
            {
                canData.IDE = "CAN_STD_ID";
            }
            else
            {
                canData.IDE = "CAN_XTD_ID";
            }
            canData.ArbID = data.ArbID;
            canData.DLC = data.DLC;
            canData.Byte1 = data.Byte1;
            canData.Byte2 = data.Byte2;
            canData.Byte3 = data.Byte3;
            canData.Byte4 = data.Byte4;
            canData.Byte5 = data.Byte5;
            canData.Byte6 = data.Byte6;
            canData.Byte7 = data.Byte7;
            canData.Byte8 = data.Byte8;

            SendCanData(ref canData);

            // todo - update receive window with tx message
            CanRxData canRxData = new CanRxData
            {
                Line = lineCount++
            };

            canRxData.Tx = true;

            canRxData.IDE = data.IDE;
            canRxData.ArbID = data.ArbID;
            canRxData.DLC = data.DLC;
            canRxData.Byte1 = data.Byte1;
            canRxData.Byte2 = data.Byte2;
            canRxData.Byte3 = data.Byte3;
            canRxData.Byte4 = data.Byte4;
            canRxData.Byte5 = data.Byte5;
            canRxData.Byte6 = data.Byte6;
            canRxData.Byte7 = data.Byte7;
            canRxData.Byte8 = data.Byte8;

            dataGridRx.Items.Add(canRxData);

        }

        // button event to connect to device
        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            Device.Connect();       
            if(!Device.IsDeviceConnected)
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
                }
            }));
        }

        // get the connected device version and hardware type
        private void GetInfo()
        {
            byte[] tmp_buf = new byte[DATA_SIZE]; // command + 63 byte = 64 bytes

            var command = new CommandMessage(COMMAND_INFO, tmp_buf); // no data to send but need array
            Device.SendMessage(command);
        }

        private void ClearStatusBarStatus()
        {
            StatusBarStatusHardware.Text = "";
            StatusBarStatusVersion.Text = "";
            StatusBarStatus.Text = "";
        }

        private void SendCanData(ref CanTxData canData)
        {
            byte[] tmp_buf = new byte[DATA_SIZE]; // command + 63 byte = 64 bytes

            // CAN Type ExID = 4, StdID = 0
            if (canData.IDE == "CAN_STD_ID")
            {
                tmp_buf[0] = CAN_STD_ID;
            }
            else
            {
                tmp_buf[0] = CAN_EXT_ID;
            }

            // Arb ID 29/11 bit
            
            if (canData.IDE == "CAN_STD_ID")
            {
                UInt32 extID = Convert.ToUInt32(canData.ArbID, 16);
                extID = extID & 0x7FF;
                tmp_buf[3] = (byte)(extID >> 8 & 0xFF);
                tmp_buf[4] = (byte)(extID & 0xFF); // LSB GMLAN power mode ID
            }
            else
            {
                UInt32 extID = Convert.ToUInt32(canData.ArbID, 16);
                tmp_buf[1] = (byte)(extID >> 24 & 0xFF); // MSB
                tmp_buf[2] = (byte)(extID >> 16 & 0xFF);
                tmp_buf[3] = (byte)(extID >> 8 & 0xFF);
                tmp_buf[4] = (byte)(extID & 0xFF); // LSB GMLAN power mode ID
            }
            tmp_buf[5] = Convert.ToByte(canData.RTR); // RTR

            //DLC
            tmp_buf[6] = Convert.ToByte(canData.DLC);

            // data bytes
            tmp_buf[7] = Convert.ToByte(canData.Byte1, 16);
            tmp_buf[8] = Convert.ToByte(canData.Byte2, 16);
            tmp_buf[9] = Convert.ToByte(canData.Byte3, 16);
            tmp_buf[10] = Convert.ToByte(canData.Byte4, 16);
            tmp_buf[11] = Convert.ToByte(canData.Byte5, 16);
            tmp_buf[12] = Convert.ToByte(canData.Byte6, 16);
            tmp_buf[13] = Convert.ToByte(canData.Byte7, 16);
            tmp_buf[14] = Convert.ToByte(canData.Byte8, 16);

            var command = new CommandMessage(COMMAND_MESSAGE, tmp_buf);
            Device.SendMessage(command);
        }

        // clears the receive window
        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            dataGridRx.ItemsSource = null;
            dataGridRx.Items.Refresh();
            dataGridRx.Columns.Clear();
            ProgressBar.Value = 0;

            lineCount = 0;
        }

        // saves data to file
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
         
                strBuilder.Append("CAN-X by Karl Yamashita. " + localDate.ToString() + "\n\n");

                // build header
                strBuilder.Append("Line" + ", ");
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
                strBuilder.Append("TimeStamp");
                strBuilder.Append("\n");

                foreach (var item in dataGridRx.Items.OfType<CanRxData>())
                {
                    strBuilder.Append(item.Line + ", ");
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
                    strBuilder.Append(item.TimeStamp);
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

        // sends new baud rate to device
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

            StatusBarStatus.Text = "Sending BTR Value";
            var command = new CommandMessage(COMMAND_BAUD, tmp_buf);
            Device.SendMessage(command);
        }

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

        private void ButtonAddEditTxRow_Click(object sender, RoutedEventArgs e)
        {
            CanTxData canData = new CanTxData();
            canData.Key = 1;
            canData.ArbID = "0x10022040";
            dataGridEditTxMessages.Items.Add(canData);
        }

        private void ButtonAddEditRxRow_Click(object sender, RoutedEventArgs e)
        {
            dataGridEditRxMessages.Items.Add(new CanRxData());
        }
    }
}
