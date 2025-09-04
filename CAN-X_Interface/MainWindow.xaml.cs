using CAN_X_CAN_Analyzer.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using USB_CAN_Interface;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Path = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;
using Timer = System.Threading.Timer;


/*
    

*/

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
        const byte COMMAND_CAN_MODE = 0x30;
        const byte COMMAND_INFO = 0x90; // get information from hardware, fw version, BTC value, type hardware
        const byte COMMAND_CAN_BTR = 0x91; // the CAN_BTC value from interface
        const byte COMMAND_VERSION = 0x92;
        const byte COMMAND_HARDWARE = 0x93; // 
        const byte COMMAND_FREQUENCY = 0x94; // the APB1 Frequency
        const byte COMMAND_BAUD = 0x95; // set the baud and mode
        /// </summary>

        // const defines
        const byte CAN_STD_ID = 0x00;
        const byte CAN_EXT_ID = 0x04;

        const int DATA_SIZE = 17;
        const bool NEW_DATA_FLAG = true;

        const int COM_PORT_QUEUE_SIZE = 256;

        const int MAX_ROW_COUNT = 10000; // how many lines to receive. This is used for the progress as well
        #endregion

        #region variables
        // arrays, variables, objects

        UInt32 lineCount = 1;

        public delegate void MessageParse(ref byte[] data);
        public delegate void SendMessage();       

        bool pauseMessagesFlag = false;
        bool scrollMessagesFlag = false;
        bool isTransmitMessage = false;

        // all tx/rx messages are stored in this list. Can be used to save to file.
        List<CanRxData> masterDataGridRx = new List<CanRxData>(); 

        string mainWindowTitle = "";

        System.Diagnostics.Stopwatch sw = null;
        Thread threadAutoTx = null;

        COM_PortDrv comPort;

        #endregion

        #region MainWindow
        public MainWindow()
        {
            InitializeComponent();

            messageMonitor.dataGridRxWindow.DataContext = this;

            // Values is item source for dataGridRxWindow
            Values = new ObservableCollection<CanRxData>();

            transmitMessages.TransmitMessagesEvent += UserControl_TransmitMessagesEvent;
            com_connection.COM_ConnectionEvent += UserControl_COM_ConnectionEvent;

            editTxMessages.EditTxMessagesUpdateStatusEvent += UserControl_EditTxMessagesUpdateStatusEvent;
            editRxMessages.EditRxMessagesUpdateStatusEvent += UserControl_EditRxMessagesUpdateStatusEvent;

            menuBar.MenuBarEvent += UserControl_MenuBarEvent;

        }
        #endregion

        #region UserControl_MenuBarEvent
        private void UserControl_MenuBarEvent(object sender, MenuBar.MenuBarEventArgs e)
        {
            if (e.EventType == "MenuItemTxPanel")
            {
                transmitMessages.Visibility = Visibility.Visible;
            }
            else if (e.EventType == "MenuItemCOM_Panel")
            {
                com_connection.Visibility = Visibility.Visible;
            }
            else if (e.EventType == "MenuItemNew")
            {
                MenuItemNew_Clicked();
            }
            else if (e.EventType == "MenuItemOpen")
            {
                MenuItemOpenProject_Clicked();
            }
            else if (e.EventType == "MenuItemSave")
            {
                MenuItemSaveProject_Clicked();
            }
            else if (e.EventType == "MenuItemExit")
            {
                MenuItemExit_Clicked();
            }
            else if(e.EventType == "MenuItemAbout")
            {
                MenuItemAbout_Clicked();
            }
        }
        #endregion

        #region UserControl_EditRxMessagesUpdateStatusEvent
        private void UserControl_EditRxMessagesUpdateStatusEvent(object sender, EditRxMessages.EditRxMessagesEventArgs e)
        {
            if ((e.EventType == "status_bar"))
            {
                statusBar.StatusBarStatus.Text = e.statusBar;
            }
        }
        #endregion

        #region UserControl_EditTxMessagesUpdateStatusEvent
        private void UserControl_EditTxMessagesUpdateStatusEvent(object sender, EditTxMessages.EditTxMessagesEventArgs e)
        {
            if(e.EventType == "dataGridTxWindow_Items_Add")
            {
                transmitMessages.dataGridTxWindow.Items.Add(e.canTxData);
            }
            else if(e.EventType == "dataGridTxWindow_Items_RemoveAt")
            {
                transmitMessages.dataGridTxWindow.Items.RemoveAt(e.rowIndex);
            }
            else if(e.EventType =="refresh")
            {
                transmitMessages.dataGridTxWindow.Items.Refresh();
            }
            else if( e.EventType == "unselectAll")
            {
                transmitMessages.dataGridTxWindow.UnselectAll();
            }
            else if( (e.EventType == "status_bar"))
            {
                statusBar.StatusBarStatus.Text = e.statusBar;
            }
        }
        #endregion

        #region UserControl_COM_ConnectionEvent
        private void UserControl_COM_ConnectionEvent(object sender, COM_Connection.COM_ConnectionEventArgs e)
        {
            if (e.EventType == "ButtonConnect")
            {
                ButtonConnect();
            }
            else if (e.EventType == "ButtonDisconnect")
            {
                ButtonDisconnect();
            }
            else if(e.EventType == "CalculateBTR")
            {
                CalculateBTR();
            }
            else if(e.EventType == "ButtonBtrClicked")
            {
                ButtonBtrClicked();
            }
            else if(e.EventType == "ResizeDataGridRx")
            {
                ResizeDataGridRx();
            }
            else if(e.EventType == "FormatDataGridColumns")
            {
                FormatDataGridColumns();
            }
            else if(e.EventType == "FormatDataGridColumns")
            {
                FormatDataGridColumns();
            }
            else if(e.EventType == "ToggleButtonAutoTx")
            {
                ToggleButtonAutoTx();
            }
                
        }
        #endregion

        #region UserControl_TransmitMessagesEvent
        private void UserControl_TransmitMessagesEvent(object sender, TransmitMessages.TransmitMessagesEventArgs e)
        {
            // Handle the event from the UserControl            
            if(e.EventType == "ButtonTxMessage_Click")
            {
                TransmitMessages_Send();
            }
            else if( e.EventType == "CheckBoxAutoTx_Checked")
            {
                TransmitMessages_AutoTx_Checked();
            }
            else if (e.EventType == "CheckBoxAutoTx_Unchecked")
            {
                TransmitMessages_AutoTx_Unchecked();
            }
            else if (e.EventType == "ToggleButtonAutoTx_Clicked")
            {
                TransmitWindowToggleButtonAutoTx();
            }
        }
        #endregion

        #region Properties for CanRxData Data Binding
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
        #endregion

        #region ComPortManager_DataReceived
        private void ComPortManager_DataReceived(object sender, byte[] data)
        {
            byte[,] twoDByteArray = new byte[COM_PORT_QUEUE_SIZE, data.Length];
            int msgCount = 0;

            QueueCOM_PortData(ref twoDByteArray, data, ref msgCount);

            int rowLength = twoDByteArray.GetLength(1);
            for (int i = 0; i < msgCount; i++)
            {
                byte[] singleDimByteArray = new byte[rowLength];
                int rowIndexToCopy = i;
                int sourceOffset = rowIndexToCopy * rowLength * sizeof(byte);

                Buffer.BlockCopy(twoDByteArray, sourceOffset, singleDimByteArray, 0, rowLength * sizeof(byte));

                ParseUsbData(ref singleDimByteArray);
            }
        }
        #endregion

        #region QueueCOM_PortData
        /*
         * Description: Parse message(s) from COM buffer into it's own queue. 
         */
        private void QueueCOM_PortData(ref byte[,] buffer, byte[] data, ref int msgCount)
        {
            int idxPtr = 0;
            int i = 0;
            int messageLength = 0;
            int msgDataPtr = 0;

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
        #endregion

        #region ParseUsbData
        public void ParseUsbData(ref byte[] data)
        {
            int command = data[0];

            byte[] newArray = new byte[data.Length - 4];

            Array.Copy(data, 4, newArray, 0, data.Length - 4);

            switch (command)
            {
                case COMMAND_MESSAGE:
                    ParseDeviceCAN_Message(ref newArray);
                    break;
                case COMMAND_ACK:
                    // StatusBarStatus.Text = "ACK Received";
                    break;
                case COMMAND_NAK:
                    // StatusBarStatus.Text = "NAK Received";
                    break;
                case COMMAND_CAN_BTR: // TODO - Work on STM32 to send this data
                    ShowBTC_VALUE(newArray);
                    break;
                case COMMAND_VERSION:
                    ShowString(COMMAND_VERSION, newArray);
                    break;
                case COMMAND_HARDWARE:
                    ShowString(COMMAND_HARDWARE, newArray);
                    break;
                case COMMAND_FREQUENCY:
                    ParseABP1_Frequency(newArray);
                    break;
            }
        }
        #endregion

        #region Button Connect/Disconnect
        // button event to connect to device
        public void ButtonConnect()
        {
            if (com_connection.ComboBoxCOM.Text == string.Empty)
            {
                com_connection.LabelConnectionStatus.Content = "Select a COM Port";
                return;
            }
            string com = com_connection.ComboBoxCOM.SelectedValue.ToString();
            comPort = new COM_PortDrv(com); // Replace with your port name and baud rate
            comPort.DataReceived += ComPortManager_DataReceived;
            try
            {
                comPort.Open();

                com_connection.LabelConnectionStatus.Content = comPort.portName + " is Opened";

                Console.WriteLine(comPort.portName + " is Opened");

                GetInfo();

                com_connection.ButtonDisconnect.IsEnabled = true;
                com_connection.ButtonConnect.IsEnabled = false;

                ClearStatusBarStatus();
            }
            catch (Exception ex) // TODO - make this and the button close call a function
            {
                com_connection.LabelConnectionStatus.Content = comPort.portName + " is not valid";

                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void ButtonDisconnect()
        {
            try
            {
                comPort.Close();

                com_connection.LabelConnectionStatus.Content = comPort.portName + " is Closed";

                Console.WriteLine(comPort.portName + " is Closed");

                ClearStatusSoftwareHarHardware();

                com_connection.ButtonConnect.IsEnabled = true;
                com_connection.ButtonDisconnect.IsEnabled = false;

                if (threadAutoTx != null)
                {
                    threadAutoTx.Abort();
                    threadAutoTx = null;
                }
                sw.Stop(); // for auto tx
                transmitMessages.ToggleButtonAutoTx.IsChecked = false;

                ClearStatusBarStatus();
            }
            catch (Exception ex)
            {
                com_connection.LabelConnectionStatus.Content = "No COM Opened";

                Console.WriteLine("Error: " + ex.Message);
            }
        }

        #endregion

        #region get and show string from data
        private void ShowString(byte command, byte[] data)
        {
            // todo - show the text sent by the interface. Need to figure out where to show. Maybe new TextBox or Lable.
            switch (command)
            {
                case COMMAND_VERSION:
                    statusBar.StatusBarStatusVersion.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        //StatusBarStatusVersion.Text = "FW: " + GetStringFromData(data);
                        statusBar.StatusBarStatusVersion.Text = "FW: " + Encoding.ASCII.GetString(data);
                    }));
                    break;
                case COMMAND_HARDWARE:
                    statusBar.StatusBarStatusVersion.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        //StatusBarStatusHardware.Text = "HW: " + GetStringFromData(data);
                        statusBar.StatusBarStatusHardware.Text = "HW: " + Encoding.ASCII.GetString(data);
                    }));
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
            btrValue = (UInt32)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);

            // TODO - show selected baud rate in combobox
            com_connection.TextBoxBtrValue.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                com_connection.TextBoxBtrValue.Text = "0x" + btrValue.ToString("X8");
            }));

            com_connection.ComboBoxMode.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if ((btrValue & 0x80000000) == 0x80000000) // silent
                {
                    com_connection.ComboBoxMode.SelectedIndex = 2;
                }
                else if ((btrValue & 0x40000000) == 0x40000000) // loopback
                {
                    com_connection.ComboBoxMode.SelectedIndex = 1;
                }
                else // normal
                {
                    com_connection.ComboBoxMode.SelectedIndex = 0;
                }
            }));

            com_connection.ComboBoxAPB1.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                int i = 0;
                string comboBoxAPB1Name = com_connection.ComboBoxAPB1.SelectedValue.ToString();
                CAN_BaudRate can_baudRate = new CAN_BaudRate(comboBoxAPB1Name);

                foreach (var item in can_baudRate.baudList)
                {
                    UInt32 _item = Convert.ToUInt32(item.value, 16) & 0x3FFFFFFF;
                    UInt32 _textBox = btrValue & 0x3FFFFFFF;
                    if (_item == _textBox)
                    {
                        com_connection.ComboBoxBaudRate.SelectedIndex = i;

                        return;
                    }
                    i++;
                }
            }));
        }
        #endregion

        #region Parse device CAN message
        private void ParseDeviceCAN_Message(ref byte[] data)
        {
            // get the date now!
            DateTime now = DateTime.Now;
            string dateNow = now.ToString("HH:mm:ss.ffff");

            CanRxData canRxData = new CanRxData(data);
            canRxData.Line = lineCount++;
            canRxData.TimeAbs = dateNow;
            // ParseAscii
            ParseAscii(ref canRxData);

            // parse ID and compare to receive messages editor. Return string description if available
            ParseForDescription(ref canRxData);

            editRxMessages.dataGridEditRxMessages.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                // add formatted data to data grid
                isTransmitMessage = false;
                AddToDataGrid(canRxData, isTransmitMessage, scrollMessagesFlag);

                // update the progress bar and remove first row if we are at MAX_ROW_COUNT
                if (UpdateProgressBar()) messageMonitor.dataGridRxWindow.Items.RemoveAt(0);
                //if (UpdateProgressBar()) Values.RemoveAt(0);
            }));
        }
        #endregion

        #region Parse ASCII from data
        private void ParseAscii(ref CanRxData canRxData)
        {
            com_connection.CheckBoxAscii.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if (com_connection.CheckBoxAscii.IsChecked != true) return;
            }));
            
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

            string apb1Freq = Encoding.ASCII.GetString(data);
            string frequency = "_" + apb1Freq.Replace(Environment.NewLine, "").Replace("\0", "");

            foreach (var en in Enum.GetNames(typeof(EnumDefines.Frequency)))
            {
                if (en == frequency)
                {
                    com_connection.ComboBoxAPB1.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        com_connection.ComboBoxAPB1.SelectedIndex = i;
                    }));
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
            foreach (CanRxData row in editRxMessages.dataGridEditRxMessages.Items)
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
            statusBar.ProgressBar.Value = lineCount;

            statusBar.TextBoxBufferPercentage.Text = lineCount.ToString() + "/" + MAX_ROW_COUNT.ToString();

            // remove data from datagrid if we reach max amount of rows
            if (lineCount >= MAX_ROW_COUNT)
            {
                statusBar.ProgressBar.Foreground = new SolidColorBrush(Colors.Red);
                return true;
            }
            statusBar.ProgressBar.Foreground = new SolidColorBrush(Colors.PaleGreen);
            return false;
        }
        #endregion

        #region AddToDataGrid
        private void AddToDataGrid(CanRxData canRxData, bool transmitFlag, bool scrollFlag)
        {
            CanRxData canRxDataNew = new CanRxData();
            bool is_CAN_ID_Match = false;

            // updating row members will update Values at that index on the fly
            if (!scrollFlag)
            {
                foreach (CanRxData row in Values)
                {
                    if (row.ArbID == canRxData.ArbID)
                    {
                        if (!transmitFlag)
                        {
                            row.RxCount = (Convert.ToUInt32(row.RxCount) + 1).ToString();         
                        }
                        else
                        {
                            row.TxCount = (Convert.ToUInt32(row.TxCount) + 1).ToString(); 
                        }

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

                        is_CAN_ID_Match = true;

                        break;
                    }
                }
                if (!is_CAN_ID_Match || messageMonitor.dataGridRxWindow.Items.Count == 0)// no match, so add new data
                {
                    if (!transmitFlag)
                    {
                        canRxData.RxCount = (1).ToString();
                    }
                    else
                    {
                        canRxData.TxCount = (1).ToString();
                    }
                    Values.Add(canRxData); // adds data to next row on data grid/gui

                    messageMonitor.dataGridRxWindow.ClearValue(ItemsControl.ItemsSourceProperty); // clear data grid/gui, is needed before using dataGridRxWindow.Items.Add
                    messageMonitor.dataGridRxWindow.Items.Add(canRxData);

                    masterDataGridRx.Add(canRxData); // do we need master list?
                }
            }
            else // scroll
            {
                messageMonitor.dataGridRxWindow.ClearValue(ItemsControl.ItemsSourceProperty); // clear data grid/gui, is needed before using dataGridRxWindow.Items.Add
                messageMonitor.dataGridRxWindow.Items.Add(canRxData);

                masterDataGridRx.Add(canRxData); // do we need master list?
            }

            // scrolls to end of data grid, if not paused
            if (pauseMessagesFlag == false && scrollFlag == true)
            {
                if (messageMonitor.dataGridRxWindow.Items.Count > 0)
                {
                    var border = VisualTreeHelper.GetChild(messageMonitor.dataGridRxWindow, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null) scroll.ScrollToEnd();
                    }
                }
            }
        }
        #endregion

        #region Button Send CAN message

        private void TransmitMessages_Send()
        {
            SendTxMsgToDataGridAndCanBus();
        }

        // send Tx message to device and update data grid
        private void SendTxMsgToDataGridAndCanBus()
        {
            DateTime now = DateTime.Now;
            string dateNow = now.ToString("HH:mm:ss.ffff");

            // get the current selected row data
            CanTxData canTxData = transmitMessages.dataGridTxWindow.SelectedItem as CanTxData;

            // formatting canRxData with canTxData and adding line count, time, tx
            CanRxData canRxData = new CanRxData(canTxData); // make copy before calling SendCanData below. Really only a problem with AutoTx, but we'll follow the same procedure here as well.

            // send to device
            SendCanData(ref canTxData);
            //CanRxData canRxData = new CanRxData(canTxData); // this is created before calling SendCanData above
            canRxData.Line = lineCount++;
            canRxData.TimeAbs = dateNow;
            canRxData.Tx = true;
            // parse ASCII characters from data bytes
            ParseAscii(ref canRxData);

            // add formatted data to data grid
            isTransmitMessage = true;
            AddToDataGrid(canRxData, isTransmitMessage, scrollMessagesFlag);
        }
        #endregion

        #region Send command to get the connected device version and hardware type
        private void GetInfo()
        {
            byte[] tmp_buf = new byte[DATA_SIZE]; // command + 63 byte = 64 bytes

            tmp_buf[0] = COMMAND_INFO;

            comPort.WriteBytes(tmp_buf, 1);
        }

        private void GetVersion()
        {
            byte[] tmp_buf = new byte[DATA_SIZE]; // command + 63 byte = 64 bytes

            tmp_buf[0] = COMMAND_VERSION;

            comPort.WriteBytes(tmp_buf, 1);
        }

        private void GetHardware()
        {
            byte[] tmp_buf = new byte[DATA_SIZE]; // command + 63 byte = 64 bytes

            tmp_buf[0] = COMMAND_HARDWARE;

            comPort.WriteBytes(tmp_buf, 1);
        }

        private void GetFrequency()
        {
            byte[] tmp_buf = new byte[DATA_SIZE]; // command + 63 byte = 64 bytes

            tmp_buf[0] = COMMAND_FREQUENCY;

            comPort.WriteBytes(tmp_buf, 1);
        }

        private void GetBaud()
        {
            byte[] tmp_buf = new byte[DATA_SIZE]; // command + 63 byte = 64 bytes

            tmp_buf[0] = COMMAND_CAN_BTR;

            comPort.WriteBytes(tmp_buf, 1);
        }
        #endregion

        #region Send CAN TX message to device over USB
        private void SendCanData(ref CanTxData canData)
        {
            byte[] usbPacket = new byte[DATA_SIZE + 4]; // original was 17, but we have 4 more bytes that are reserved

            usbPacket[0] = COMMAND_MESSAGE;

            // index 1-3 are reserved.

            // CAN Type ExID = 4, StdID = 0
            if (canData.IDE == "S")
            {
                usbPacket[4] = CAN_STD_ID;
            }
            else
            {
                usbPacket[4] = CAN_EXT_ID;
            }

            // RTR
            usbPacket[5] = canData.RTR == true ? (byte)1 : (byte)0; // RTR, Node

            // Node
            byte i = 0;
            foreach (var en in Enum.GetNames(typeof(EnumDefines.Nodes)))
            {
                if (en == canData.Node)
                {
                    usbPacket[6] = i;
                    break;
                }
                i++;
            }

            // index 7 is reserved

            // Arb ID 29/11 bit
            if (canData.IDE == "CAN_STD_ID")
            {
                UInt32 extID = Convert.ToUInt32(canData.ArbID, 16);
                extID = extID & 0x7FF;
                usbPacket[8] = (byte)(extID & 0xFF); // LSB GMLAN power mode ID
                usbPacket[9] = (byte)(extID >> 8 & 0xFF);
            }
            else
            {
                UInt32 extID = Convert.ToUInt32(canData.ArbID, 16);
                usbPacket[8] = (byte)(extID & 0xFF); // LSB GMLAN power mode ID
                usbPacket[9] = (byte)(extID >> 8 & 0xFF);
                usbPacket[10] = (byte)(extID >> 16 & 0xFF);
                usbPacket[11] = (byte)(extID >> 24 & 0xFF); // MSB         
            }

            //DLC
            if (canData.DLC != "")
            {
                usbPacket[12] = Convert.ToByte(canData.DLC);
            }

            // data bytes
            if (canData.Byte1 != "")
            {
                usbPacket[13] = Convert.ToByte(canData.Byte1, 16);
            }

            if (canData.Byte2 != "")
            {
                usbPacket[14] = Convert.ToByte(canData.Byte2, 16);
            }

            if (canData.Byte3 != "")
            {
                usbPacket[15] = Convert.ToByte(canData.Byte3, 16);
            }

            if (canData.Byte4 != "")
            {
                usbPacket[16] = Convert.ToByte(canData.Byte4, 16);
            }

            if (canData.Byte5 != "")
            {
                usbPacket[17] = Convert.ToByte(canData.Byte5, 16);
            }

            if (canData.Byte6 != "")
            {
                usbPacket[18] = Convert.ToByte(canData.Byte6, 16);
            }

            if (canData.Byte7 != "")
            {
                usbPacket[19] = Convert.ToByte(canData.Byte7, 16);
            }

            if (canData.Byte8 != "")
            {
                usbPacket[20] = Convert.ToByte(canData.Byte8, 16);
            }

            comPort.WriteBytes(usbPacket, DATA_SIZE + 4);
        }
        #endregion

        #region clear receive window, ClearStatusBar
        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            messageMonitor.dataGridRxWindow.ClearValue(ItemsControl.ItemsSourceProperty);
            while (messageMonitor.dataGridRxWindow.Items.Count != 0)
            {
                messageMonitor.dataGridRxWindow.Items.RemoveAt(0);
            }
            Values.Clear();
            while (masterDataGridRx.Count != 0)
            {
                masterDataGridRx.RemoveAt(0);
            }
            statusBar.ProgressBar.Value = 0;
            lineCount = 0;
            ClearStatusBarStatus();
        }

        private void ClearStatusBarStatus()
        {
            statusBar.StatusBarStatus.Text = "";
        }

        private void ClearStatusSoftwareHarHardware()
        {
            statusBar.StatusBarStatusHardware.Text = "";
            statusBar.StatusBarStatusVersion.Text = "";
        }
        #endregion

        #region ButtonBtr click. Sends new baud rate to device and/or Listen mode

        public void ButtonBtrClicked()
        {
            if ((comPort == null) || (comPort.IsOpen == false))
            {
                statusBar.StatusBarStatus.Text = "Device Not Connected";
                return;
            }

            byte[] tmp_buf = new byte[DATA_SIZE];
            string myString = com_connection.TextBoxBtrValue.Text;
            UInt32 btrValue;
            try
            {
                btrValue = Convert.ToUInt32(myString, 16);
            }
            catch (FormatException)
            {
                // we should never get here
                statusBar.StatusBarStatus.Text = "Hex value is not in correct format";
                return;
            }

            tmp_buf[0] = COMMAND_BAUD;
            // 3 bytes reserved
            tmp_buf[4] = (byte)(btrValue >> 24);
            tmp_buf[5] = (byte)(btrValue >> 16);
            tmp_buf[6] = (byte)(btrValue >> 8);
            tmp_buf[7] = (byte)(btrValue);

            tmp_buf[8] = 0; // CAN1

            //StatusBarStatus.Text = "Sending BTR Value";
            comPort.WriteBytes(tmp_buf, DATA_SIZE);
        }
        #endregion

        #region ComboBox BTR, Node, init
        private void InitPopulateBaudRateListBox()
        {
            // todo - allow use to select frequency to adjust CAN_BTR value for that frequency
            CAN_BaudRate can_baudRate = new CAN_BaudRate("APB1_48mHz");

            foreach (var item in can_baudRate.baudList)
            {
                com_connection.ComboBoxBaudRate.Items.Add(item.baud);
            }
            com_connection.ComboBoxBaudRate.SelectedIndex = 1;

            com_connection.ComboBoxAPB1.ItemsSource = Enum.GetNames(typeof(EnumDefines.APB1_Freq));
            com_connection.ComboBoxAPB1.SelectedIndex = 0;
        }

        public void CalculateBTR()
        {
            // baud rate change
            if (com_connection.ComboBoxBaudRate.Items.Count == 0 || com_connection.ComboBoxAPB1.Items.Count == 0) return;
            string comboBoxItemName = com_connection.ComboBoxBaudRate.SelectedValue.ToString();
            string comboBoxAPB1Name = com_connection.ComboBoxAPB1.SelectedValue.ToString();

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

            // CAN mode change
            int mode = com_connection.ComboBoxMode.SelectedIndex;
            UInt32 currentBTRValue = Convert.ToUInt32(value, 16);

            if (mode == 1)
            {
                currentBTRValue |= 0x40000000;// Bit 30 is Loopback mode, disable = 0, loopback enabled = 1
            }
            else if (mode == 2)
            {
                currentBTRValue |= 0x80000000;// Bit 31 is Normal=0, Silent = 1.
            }

            string finalStrValue = string.Format("{0}{1}", "0x", currentBTRValue.ToString("X8"));

            com_connection.TextBoxBtrValue.Text = finalStrValue;

        }

        #endregion

        #region main Window loaded
        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow1.Title = CAN_X_CAN_Analyzer.Properties.Settings.Default.titleWindow;
            mainWindowTitle = MainWindow1.Title; // make a copy
            if (File.Exists(CAN_X_CAN_Analyzer.Properties.Settings.Default.lastFilePath))
            {
                ReadXml(CAN_X_CAN_Analyzer.Properties.Settings.Default.lastFilePath);
                MainWindow1.Title = mainWindowTitle + " - " + Path.GetFileName(CAN_X_CAN_Analyzer.Properties.Settings.Default.lastFilePath);
            }

            com_connection.CheckBoxBlind.IsChecked = CAN_X_CAN_Analyzer.Properties.Settings.Default.imBlind;
            ResizeDataGridRx();

            InitPopulateBaudRateListBox();

            editRxMessages.ComboBoxRxNode.ItemsSource = Enum.GetNames(typeof(EnumDefines.Nodes));
            
            // TODO - fix
            //ComboBoxTxNode_DropDownClosed.ItemsSource = Enum.GetNames(typeof(EnumDefines.Nodes));

            // need to remove "_" in the enum
            List<string> txRateList = new List<string>();
            foreach (var en in Enum.GetNames(typeof(EnumDefines.TxRate)))
            {
                txRateList.Add(en.Replace("_", ""));
            }
            editTxMessages.ComboBoxEditTxRate.ItemsSource = txRateList;

            sw = new System.Diagnostics.Stopwatch();

            // get checkbox states
            com_connection.CheckBoxAscii.IsChecked = CAN_X_CAN_Analyzer.Properties.Settings.Default.ascii;
            com_connection.CheckBoxNotes.IsChecked = CAN_X_CAN_Analyzer.Properties.Settings.Default.notes;
            // now format datagrid if needed
            FormatDataGridColumns();

            statusBar.ProgressBar.Maximum = MAX_ROW_COUNT;

        }
        #endregion

        #region MenuItemexit
        private void MenuItemExit_Clicked()
        {
            this.Close();
        }
        #endregion

        #region Window Closing
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
                if (comPort == null || (comPort.IsOpen == false))
                {
                    return;
                }
                else
                {
                    comPort.Close(); // disconnet USB device
                }
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
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
                strBuilder.Append("Node" + ", ");
                strBuilder.Append("ASCII" + ", ");
                strBuilder.Append("Notes" + ", ");
                strBuilder.Append("\n");

                foreach (var item in masterDataGridRx)
                //foreach (var item in dataGridRx.Items.OfType<CanRxData>())
                {
                    strBuilder.Append(item.Line + ", ");
                    strBuilder.Append(item.RxCount + ", ");
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

                    strBuilder.Append(item.Node + ", ");
                    strBuilder.Append(item.ASCII + ", ");
                    strBuilder.Append(item.Notes + ", ");

                    strBuilder.Append("\n");
                }

                try
                {
                    File.WriteAllText(saveFile.FileName, strBuilder.ToString());
                    string filename = saveFile.FileName;
                    statusBar.StatusBarStatus.Text = "Successfully saved " + filename;
                }
                catch (IOException)
                {
                    string messageBoxText = "File not accessable! File may be in use.";
                    string dialogTitle = "File Error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Warning;

                    System.Windows.MessageBox.Show(messageBoxText, dialogTitle, button, icon);

                    statusBar.StatusBarStatus.Text = messageBoxText;
                }
            }
        }
        #endregion

        #region MenuItemNew

        private void MenuItemNew_Clicked()
        {
            editTxMessages.dataGridEditTxMessages.Items.Clear();
            editRxMessages.dataGridEditRxMessages.Items.Clear();
            messageMonitor.dataGridRxWindow.ClearValue(ItemsControl.ItemsSourceProperty);
            messageMonitor.dataGridRxWindow.Items.Clear();
            transmitMessages.dataGridTxWindow.ClearValue(ItemsControl.ItemsSourceProperty);
            transmitMessages.dataGridTxWindow.Items.Clear();
            masterDataGridRx.Clear();
            lineCount = 1;
        }
        #endregion

        #region MenuItem Save project
        private void MenuItemSaveProject_Clicked()
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
            settings.OmitXmlDeclaration = false;

            XmlWriter xmlWriter = XmlWriter.Create(saveFile, settings);

            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("CAN-X");
            xmlWriter.WriteElementString("Created_by", "CAN-X software by Karl Yamashita. (github.com/karlyamashita/CAN-X)");
            xmlWriter.WriteElementString("Project_Filename", Path.GetFileName(saveFile));

            // edit tx messages
            xmlWriter.WriteStartElement("edit_tx_messages");
            foreach (var item in editTxMessages.dataGridEditTxMessages.Items.OfType<CanTxData>())
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
             xmlWriter.WriteEndElement();

            // edit rx messages
            xmlWriter.WriteStartElement("edit_rx_messages");
            foreach (var item in editRxMessages.dataGridEditRxMessages.Items.OfType<CanRxData>())
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
            statusBar.StatusBarStatus.Text = "File saved successfully";
        }
        #endregion

        #region MenuItem Open project
        private void MenuItemOpenProject_Clicked()
        {
            OpenFileDialog openFile = new OpenFileDialog()
            {
                DefaultExt = ".canx",
                Filter = "CAN-X Project (.canx)|*.canx"
            };
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CAN_X_CAN_Analyzer.Properties.Settings.Default.lastFilePath = openFile.FileName;
                CAN_X_CAN_Analyzer.Properties.Settings.Default.Save();
                if (!File.Exists(openFile.FileName))
                {
                    statusBar.StatusBarStatus.Text = "File does not exist!";
                }
                else
                {
                    MenuItemNew_Clicked(); // clear current data
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
                            while (editRxMessages.dataGridEditRxMessages.Items.Count != 0)
                            {
                                editRxMessages.dataGridEditRxMessages.Items.RemoveAt(0);
                            }
                            while (editTxMessages.dataGridEditTxMessages.Items.Count != 0)
                            {
                                editTxMessages.dataGridEditTxMessages.Items.RemoveAt(0);
                            }
                            while (transmitMessages.dataGridTxWindow.Items.Count != 0)
                            {
                                transmitMessages.dataGridTxWindow.Items.RemoveAt(0);
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

                                editTxMessages.dataGridEditTxMessages.Items.Add(canTxData);
                                transmitMessages.dataGridTxWindow.Items.Add(canTxData);
                                canTxData = new CanTxData();
                            }
                            else
                            {
                                canRxData.Notes = result;

                                editRxMessages.dataGridEditRxMessages.Items.Add(canRxData);
                                canRxData = new CanRxData();
                            }
                            break;
                    }
                }
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
            
            // no longer scrolling so clear screen so active messages show on gui.
            if (!scrollMessagesFlag)
            {
                messageMonitor.dataGridRxWindow.ClearValue(ItemsControl.ItemsSourceProperty);
                while (messageMonitor.dataGridRxWindow.Items.Count != 0)
                {
                    messageMonitor.dataGridRxWindow.Items.RemoveAt(0);
                }
                Values.Clear();
                ButtonPauseMessages.IsEnabled = false;
                ButtonPauseMessages.IsChecked = false;
            }
            else
            {
                ButtonPauseMessages.IsEnabled = true;
            }
        }
        #endregion

        #region Resize DataGridRx
        public void ResizeDataGridRx()
        {
            if (com_connection.CheckBoxBlind.IsChecked == true)
            {
                Style rowStyle = new Style();
                rowStyle.TargetType = typeof(DataGridRow);
                rowStyle.Setters.Add(new Setter() { Property = FontSizeProperty, Value = 20D });
                rowStyle.Setters.Add(new Setter() { Property = HeightProperty, Value = 30D });
                messageMonitor.dataGridRxWindow.RowStyle = rowStyle;

                CAN_X_CAN_Analyzer.Properties.Settings.Default.imBlind = true;
                CAN_X_CAN_Analyzer.Properties.Settings.Default.Save();
            }
            else
            {
                Style rowStyle = new Style();
                rowStyle.TargetType = typeof(DataGridRow);
                rowStyle.Setters.Add(new Setter() { Property = FontSizeProperty, Value = 12D });
                rowStyle.Setters.Add(new Setter() { Property = HeightProperty, Value = 18D });
                messageMonitor.dataGridRxWindow.RowStyle = rowStyle;
                // resize columns
                foreach (DataGridColumn c in messageMonitor.dataGridRxWindow.Columns)
                {
                    c.Width = 0;
                }
                foreach (DataGridColumn c in messageMonitor.dataGridRxWindow.Columns)
                {
                    c.Width = DataGridLength.Auto;
                }
                messageMonitor.dataGridRxWindow.UpdateLayout();

                CAN_X_CAN_Analyzer.Properties.Settings.Default.imBlind = false;
                CAN_X_CAN_Analyzer.Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region CheckBoxEditTxAutoTx checked

        private void TransmitMessages_AutoTx_Checked()
        {
            CanTxData data = transmitMessages.dataGridTxWindow.SelectedItem as CanTxData; // grabs the current selected row, which you can get the items
            CanTxData dataEdit = editTxMessages.dataGridEditTxMessages.SelectedItem as CanTxData;

            if (data == null)
            {
                //   StatusBarStatus.Text = "Please select an ArbID to modify";
                return;
            }
            // need to update the dataGridEditRxMessages and CheckBoxEditTxAutoTx
            foreach (CanTxData row in editTxMessages.dataGridEditTxMessages.Items)
            {
                if (row.Key == data.Key)
                {
                    if (dataEdit != null)
                    {
                        data.AutoTx = true;
                        if (dataEdit.Key == row.Key)
                        {
                            editTxMessages.CheckBoxEditTxAutoTx.IsChecked = true;
                        }
                    }
                    row.AutoTx = true;
                    editTxMessages.dataGridEditTxMessages.Items.Refresh();
                }
            }
        }
        #endregion

        #region CheckBoxAutoTx Checked

        private void TransmitMessages_AutoTx_Unchecked()
        {
            CanTxData data = transmitMessages.dataGridTxWindow.SelectedItem as CanTxData; // grabs the current selected row, which you can get the items
            CanTxData dataEdit = editTxMessages.dataGridEditTxMessages.SelectedItem as CanTxData;

            if (data == null)
            {
                //   StatusBarStatus.Text = "Please select an ArbID to modify";
                return;
            }
            // need to update the dataGridEditRxMessages and CheckBoxEditTxAutoTx
            foreach (CanTxData row in editTxMessages.dataGridEditTxMessages.Items)
            {
                if (row.Key == data.Key)
                {
                    data.AutoTx = false;
                    if (dataEdit != null)
                    {
                        editTxMessages.dataGridEditTxMessages.UnselectAll();
                        editTxMessages.CheckBoxEditTxAutoTx.IsChecked = false;
                    }
                    row.AutoTx = false;
                    editTxMessages.dataGridEditTxMessages.Items.Refresh();
                }
            }
        }

        #endregion

        #region ToggleButtonAutoTx click

        public void ToggleButtonAutoTx()
        {
            if (transmitMessages.ToggleButtonAutoTx.IsChecked == true)
            {
                if ((comPort == null) || (comPort.IsOpen == false))
                {
                    statusBar.StatusBarStatus.Text = "Device Not Connected";
                    transmitMessages.ToggleButtonAutoTx.IsChecked = false;
                    return;
                }

                if (threadAutoTx == null)
                {
                    threadAutoTx = new Thread(TxSendThread);
                    threadAutoTx.Start();
                }

                sw.Start();
            }
            else
            {
                if (threadAutoTx != null)
                {
                    threadAutoTx.Abort();
                    threadAutoTx = null;
                }
                sw.Stop();
            }
        }

        public void TransmitWindowToggleButtonAutoTx()
        {
            if (transmitMessages.ToggleButtonAutoTx.IsChecked == true)
            {
                if ((comPort == null) || (comPort.IsOpen == false))
                {
                    statusBar.StatusBarStatus.Text = "Device Not Connected";
                    transmitMessages.ToggleButtonAutoTx.IsChecked = false;
                    return;
                }

                if (threadAutoTx == null)
                {
                    threadAutoTx = new Thread(TxSendThread);
                    threadAutoTx.Start();
                }

                sw.Start();
            }
            else
            {
                if (threadAutoTx != null)
                {
                    threadAutoTx.Abort();
                    threadAutoTx = null;
                }
                sw.Stop();
            }
        }

        #endregion

        #region TxSendThread
        public void TxSendThread(object state)
        {
            int Tick = 10;
            int Sleep = 1;
            long OldElapsedMilliseconds = 0;
            CanTxData canTxData = null;

            if(comPort == null || (comPort.IsOpen == false)) return;

            while (sw.IsRunning)
            {
                long ElapsedMilliseconds = sw.ElapsedMilliseconds;
                long mod = (ElapsedMilliseconds % Tick);
                if (OldElapsedMilliseconds != ElapsedMilliseconds && (mod == 0 || ElapsedMilliseconds > Tick))
                {
                    foreach (CanTxData row in transmitMessages.dataGridTxWindow.Items)
                    {
                        if (row.AutoTx == true)
                        {
                            if (++row.RateTimer >= (Convert.ToUInt32(row.Rate) / 10))
                            {
                                row.RateTimer = 0;
                                canTxData = new CanTxData(row);
                                CanRxData canRxData = new CanRxData(canTxData); // save to rx first before sending
                                SendCanData(ref canTxData);
                                editRxMessages.dataGridEditRxMessages.Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    // added 8-15-2025
                                    DateTime now = DateTime.Now;
                                    string dateNow = now.ToString("HH:mm:ss.ffff");
                                    // formatting canRxData with canTxData and adding line count, time, tx
                                   // CanRxData canRxData = new CanRxData(canTxData); // done before calling SendCanData above. Now GUI update seems to be in sync with Transmitted messages.
                                    canRxData.Line = lineCount++;
                                    canRxData.TimeAbs = dateNow;
                                    canRxData.Tx = true;
                                    // parse ASCII characters from data bytes
                                    ParseAscii(ref canRxData);

                                    // add formatted data to data grid
                                    isTransmitMessage = true;
                                    AddToDataGrid(canRxData, isTransmitMessage, scrollMessagesFlag);
                                }));
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

        #region ContextMenuItemSaveToRx
        private void MenuItemSaveRx_Click_1(object sender, RoutedEventArgs e)
        {
            // StatusBarStatus.Text = "Save to Rx";
            CanRxData data = messageMonitor.dataGridRxWindow.SelectedItem as CanRxData; // grabs the current selected row, which you can get the items
            if (data == null)
            {
                statusBar.StatusBarStatus.Text = "Please select an ArbID to save";
                return;
            }

            // find next key number to use
            ulong highKey = 0;
            foreach (CanRxData item in editRxMessages.dataGridEditRxMessages.Items)
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
            editRxMessages.dataGridEditRxMessages.Items.Add(canRxData);
        }

        private void MenuItemSaveTx_Click(object sender, RoutedEventArgs e)
        {
            // StatusBarStatus.Text = "Save to Tx";
            CanRxData data = messageMonitor.dataGridRxWindow.SelectedItem as CanRxData; // grabs the current selected row, which you can get the items
            if (data == null)
            {
                statusBar.StatusBarStatus.Text = "Please select an ArbID to save";
                return;
            }

            // find next key number to use
            ulong highKey = 0;
            foreach (CanTxData item in editTxMessages.dataGridEditTxMessages.Items)
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
            editTxMessages.dataGridEditTxMessages.Items.Add(canTxData);
            // add to main Tx datagrid
            transmitMessages.dataGridTxWindow.Items.Add(canTxData);
        }
        #endregion

        #region FormatDataGridColumns

        public void FormatDataGridColumns()
        {
            /*
            if (com_connection.CheckBoxAscii.IsChecked == true)
            {
                messageMonitor.dataGridRxWindow.Columns[20].Visibility = Visibility.Visible;
            }
            else
            {
                messageMonitor.dataGridRxWindow.Columns[20].Visibility = Visibility.Hidden;
            }

            if (com_connection.CheckBoxNotes.IsChecked == true)
            {
                messageMonitor.dataGridRxWindow.Columns[21].Visibility = Visibility.Visible;
            }
            else
            {
                messageMonitor.dataGridRxWindow.Columns[21].Visibility = Visibility.Hidden;
            }
            */
        }
        #endregion

        #region About Dialog

        private void MenuItemAbout_Clicked()
        {
            About about = new About();
            about.Show();
        }
        #endregion

    }
}

