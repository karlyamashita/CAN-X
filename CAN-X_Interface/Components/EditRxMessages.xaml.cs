using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static CAN_X_CAN_Analyzer.Components.EditTxMessages;

namespace CAN_X_CAN_Analyzer.Components
{
    /// <summary>
    /// Interaction logic for EditRxMessages.xaml
    /// </summary>
    public partial class EditRxMessages : UserControl
    {
        int rowIndexEditRx = 0;

        MainWindow mainWindow;

        public event EventHandler<EditRxMessagesEventArgs> EditRxMessagesUpdateStatusEvent;

        public class EditRxMessagesEventArgs : EventArgs
        {
            public string EventType { get; set; }

            public string statusBar { get; set; }

            // Add other properties as needed
        }

        // Helper method to raise the event
        protected virtual void OnMyCustomEvent(EditRxMessagesEventArgs e)
        {
            EditRxMessagesUpdateStatusEvent?.Invoke(this, e);
        }

        public EditRxMessages()
        {
            InitializeComponent();

            mainWindow = Application.Current.MainWindow as MainWindow;
        }

        private void TextBoxRx_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int hexNumber;
            e.Handled = !int.TryParse(e.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out hexNumber);
        }

        private void TextBoxRxDLC_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text); ;
        }

        private void ComboBoxRxNode_DropDownClosed(object sender, EventArgs e)
        {
            CanRxData data = dataGridEditRxMessages.SelectedItem as CanRxData; // grabs the current selected row
            if (data == null)
            {
                try // this event happens before StatusBarStatus is generated in the window, so it is null. So using try/catch for now.
                {
                    //StatusBarStatus.Text = "Select an ArbID first and try selecting the node again";
                    OnMyCustomEvent(new EditRxMessagesEventArgs { EventType = "status_bar", statusBar = "Select an ArbID first and try selecting the node again" });
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
                //StackPanelHighVoltage.IsEnabled = true;
            }
            else
            {
                //StackPanelHighVoltage.IsEnabled = false;
                //CheckBoxHighVoltage.IsChecked = false;
            }
        }

        private void TextBoxEditMessageRx_TextChanged(object sender, TextChangedEventArgs e)
        {
            CanRxData canRxData = (CanRxData)dataGridEditRxMessages.SelectedItem;

            if (canRxData == null)
            {
                //    StatusBarStatus.Text = "You need to select a row";
                OnMyCustomEvent(new EditRxMessagesEventArgs { EventType = "status_bar", statusBar = "You need to select a row" });
                return;
            }
            else
            {
                //    StatusBarStatus.Text = "";
                OnMyCustomEvent(new EditRxMessagesEventArgs { EventType = "status_bar", statusBar = "" });
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
                        //StatusBarStatus.Text = "";
                        OnMyCustomEvent(new EditRxMessagesEventArgs { EventType = "status_bar", statusBar = "" });
                    }
                    else if (id == 0)
                    {
                        canRxData.IDE = "S";
                        //StatusBarStatus.Text = "";
                        OnMyCustomEvent(new EditRxMessagesEventArgs { EventType = "status_bar", statusBar = "" });
                    }
                    else
                    {
                        //StatusBarStatus.Text = "ArbID should be between 0x000 - 0x1FFFFFFF";
                        OnMyCustomEvent(new EditRxMessagesEventArgs { EventType = "status_bar", statusBar = "ArbID should be between 0x000 - 0x1FFFFFFF" });
                        break;
                    }
                    canRxData.ArbID = tempStr;
                    break;
                case "TextBoxRxDLC":
                    if (TextBoxRxDLC.Text != string.Empty)
                    {
                        canRxData.DLC = uint.Parse(TextBoxRxDLC.Text.ToUpper()).ToString("X2");
                    }
                    else
                    {
                        canRxData.DLC = string.Empty;
                    }
                    break;
                case "TextBoxRxByte1":
                    if (TextBoxRxByte1.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxRxByte1.Text, 16);
                        canRxData.Byte1 = result.ToString("X2");
                    }
                    else
                    {
                        canRxData.Byte1 = string.Empty;
                    }
                    break;
                case "TextBoxRxByte2":
                    if (TextBoxRxByte2.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxRxByte2.Text, 16);
                        canRxData.Byte2 = result.ToString("X2");
                    }
                    else
                    {
                        canRxData.Byte2 = string.Empty;
                    }
                    break;
                case "TextBoxRxByte3":
                    if (TextBoxRxByte3.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxRxByte3.Text, 16);
                        canRxData.Byte3 = result.ToString("X2");
                    }
                    else
                    {
                        canRxData.Byte3 = string.Empty;
                    }
                    break;
                case "TextBoxRxByte4":
                    if (TextBoxRxByte4.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxRxByte4.Text, 16);
                        canRxData.Byte4 = result.ToString("X2");
                    }
                    else
                    {
                        canRxData.Byte4 = string.Empty;
                    }
                    break;
                case "TextBoxRxByte5":
                    if (TextBoxRxByte5.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxRxByte5.Text, 16);
                        canRxData.Byte5 = result.ToString("X2");
                    }
                    else
                    {
                        canRxData.Byte5 = string.Empty;
                    }
                    break;
                case "TextBoxRxByte6":
                    if (TextBoxRxByte6.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxRxByte6.Text, 16);
                        canRxData.Byte6 = result.ToString("X2");
                    }
                    else
                    {
                        canRxData.Byte6 = string.Empty;
                    }
                    break;
                case "TextBoxRxByte7":
                    if (TextBoxRxByte7.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxRxByte7.Text, 16);
                        canRxData.Byte7 = result.ToString("X2");
                    }
                    else
                    {
                        canRxData.Byte7 = string.Empty;
                    }
                    break;
                case "TextBoxRxByte8":
                    if (TextBoxRxByte8.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxRxByte8.Text, 16);
                        canRxData.Byte8 = result.ToString("X2");
                    }
                    else
                    {
                        canRxData.Byte8 = string.Empty;
                    }
                    break;
                case "TextBoxRxNotes":
                    canRxData.Notes = TextBoxRxNotes.Text;
                    break;
            }
            dataGridEditRxMessages.Items.Refresh();
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

        private void ButtonCopyEditRxRow_Click(object sender, RoutedEventArgs e)
        {
            UInt32 newIndex = 0;

            if (dataGridEditRxMessages.SelectedItem != null)
            {
                CanRxData selectedItem = (CanRxData)dataGridEditRxMessages.SelectedItem;

                CanRxData newCanRxData = new CanRxData
                {
                    Description = selectedItem.Description,
                    IDE = selectedItem.IDE,
                    ArbID = selectedItem.ArbID,
                    RTR = selectedItem.RTR,
                    DLC = selectedItem.DLC,
                    Byte1 = selectedItem.Byte1,
                    Byte2 = selectedItem.Byte2,
                    Byte3 = selectedItem.Byte3,
                    Byte4 = selectedItem.Byte4,
                    Byte5 = selectedItem.Byte5,
                    Byte6 = selectedItem.Byte6,
                    Byte7 = selectedItem.Byte7,
                    Byte8 = selectedItem.Byte8,
                    Notes = selectedItem.Notes,
                    Node = selectedItem.Node
                };

                foreach (CanRxData rx in dataGridEditRxMessages.Items)
                {
                    if (rx.Key > newIndex)
                    {
                        newIndex = (UInt32)rx.Key;
                    }
                }
                newCanRxData.Key = newIndex + 1; // update key before adding item

                dataGridEditRxMessages.Items.Add(newCanRxData);
            }
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
            ComboBoxRxNode.SelectedIndex = GetComboBoxNodeIndex(data.Node);
            TextBoxRxNotes.Text = data.Notes;
        }

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
        }

        // TODO make a class or global since edit tx/rx both use this
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
                if (en.Replace("_", "") == name)
                {
                    return i;
                }
                i++;
            }
            return i;
        }

    }
}
