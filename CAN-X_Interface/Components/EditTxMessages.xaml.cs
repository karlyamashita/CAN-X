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

namespace CAN_X_CAN_Analyzer.Components
{
    /// <summary>
    /// Interaction logic for EditTxMessages.xaml
    /// </summary>
    public partial class EditTxMessages : UserControl
    {
        int rowIndexEditTx = 0;

        public event EventHandler<EditTxMessagesEventArgs> EditTxMessagesUpdateStatusEvent;

        public class EditTxMessagesEventArgs : EventArgs
        {
            public string EventType { get; set; }

            public string statusBar { get; set; }

            public int rowIndex { get; set; }

            public bool dataGridTxWindow_Items_Refresh { get; set; }

            public CanTxData canTxData { get; set; }


            // Add other properties as needed
        }

        // Helper method to raise the event
        protected virtual void OnMyCustomEvent(EditTxMessagesEventArgs e)
        {
            EditTxMessagesUpdateStatusEvent?.Invoke(this, e);
        }

        public EditTxMessages()
        {
            InitializeComponent();
        }

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

            // the Tx dataGrid
            OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "dataGridTxWindow_Items_Add", canTxData = canTxData });
        }

        private void ButtonDeleteEditTxRow_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridEditTxMessages.SelectedItem != null)
            {
                // TODO - need to find solution to delete selected row, for now using index
                dataGridEditTxMessages.Items.RemoveAt(rowIndexEditTx);
                try
                {
                    // If adding new Tx row doesn't update Tx Window, then this index won't exist.
                    // So catch exception to avoid crash.
                    OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "dataGridTxWindow_Items_RemoveAt", rowIndex = rowIndexEditTx });
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
        }

        private void ButtonCopyEditTxRow_Click(object sender, RoutedEventArgs e)
        {
            UInt32 newIndex = 0;

            if (dataGridEditTxMessages.SelectedItem != null)
            {
                CanTxData selectedItem = (CanTxData)dataGridEditTxMessages.SelectedItem;

                CanTxData newCanTxData = new CanTxData
                {
                    Description = selectedItem.Description,
                    AutoTx = selectedItem.AutoTx,
                    Rate = selectedItem.Rate,
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

                foreach (CanTxData tx in dataGridEditTxMessages.Items)
                {
                    if (tx.Key > newIndex)
                    {
                        newIndex = (UInt32)tx.Key;
                    }
                }
                newCanTxData.Key = newIndex + 1; // update key before adding item

                dataGridEditTxMessages.Items.Add(newCanTxData);

                OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "dataGridTxWindow_Items_Add", canTxData = newCanTxData });
            }
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

            //ComboBoxEditTxRate.SelectedIndex = GetComboBoxTxRateIndex(data.Rate);
            ComboBoxEditTxRate.Text = data.Rate;

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
        }

        private void TextBoxEditMessageTx_TextChanged(object sender, TextChangedEventArgs e)
        {
            CanTxData canTxData = (CanTxData)dataGridEditTxMessages.SelectedItem;

            if (canTxData == null)
            {
                OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "You need to select a row" });
                return;
            }
            else
            {
                OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "" });
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
                        OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "status_bar", statusBar = "" });
                    }
                    else if (id == 0)
                    {
                        canTxData.IDE = "S";
                        OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "status_bar", statusBar = "" });
                    }
                    else
                    {
                        OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "status_bar", statusBar = "ArbID should be between 0x000 - 0x1FFFFFFF" });
                        break;
                    }
                    canTxData.ArbID = tempStr;
                    break;
                case "TextBoxTxDLC":
                    if (TextBoxTxDLC.Text != string.Empty)
                    {
                        canTxData.DLC = uint.Parse(TextBoxTxDLC.Text.ToUpper()).ToString();
                    }
                    else
                    {
                        canTxData.DLC = string.Empty;
                    }
                    break;
                case "TextBoxTxByte1":
                    if (TextBoxTxByte1.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxTxByte1.Text, 16);
                        canTxData.Byte1 = result.ToString("X2");
                    }
                    else
                    {
                        canTxData.Byte1 = string.Empty;
                    }
                    break;
                case "TextBoxTxByte2":
                    if (TextBoxTxByte2.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxTxByte2.Text, 16);
                        canTxData.Byte2 = result.ToString("X2");
                    }
                    else
                    {
                        canTxData.Byte2 = string.Empty;
                    }
                    break;
                case "TextBoxTxByte3":
                    if (TextBoxTxByte3.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxTxByte3.Text, 16);
                        canTxData.Byte3 = result.ToString("X2");
                    }
                    else
                    {
                        canTxData.Byte3 = string.Empty;
                    }
                    break;
                case "TextBoxTxByte4":
                    if (TextBoxTxByte4.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxTxByte4.Text, 16);
                        canTxData.Byte4 = result.ToString("X2");
                    }
                    else
                    {
                        canTxData.Byte4 = string.Empty;
                    }
                    break;
                case "TextBoxTxByte5":
                    if (TextBoxTxByte5.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxTxByte5.Text, 16);
                        canTxData.Byte5 = result.ToString("X2");
                    }
                    else
                    {
                        canTxData.Byte5 = string.Empty;
                    }
                    break;
                case "TextBoxTxByte6":
                    if (TextBoxTxByte6.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxTxByte6.Text, 16);
                        canTxData.Byte6 = result.ToString("X2");
                    }
                    else
                    {
                        canTxData.Byte6 = string.Empty;
                    }
                    break;
                case "TextBoxTxByte7":
                    if (TextBoxTxByte7.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxTxByte7.Text, 16);
                        canTxData.Byte7 = result.ToString("X2");
                    }
                    else
                    {
                        canTxData.Byte7 = string.Empty;
                    }
                    break;
                case "TextBoxTxByte8":
                    if (TextBoxTxByte8.Text != string.Empty)
                    {
                        int result = Convert.ToInt32(TextBoxTxByte8.Text, 16);
                        canTxData.Byte8 = result.ToString("X2");
                    }
                    else
                    {
                        canTxData.Byte8 = string.Empty;
                    }
                    break;

            }
            dataGridEditTxMessages.Items.Refresh();
            OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "refresh"});
        }

        private void TextBoxTx_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int hexNumber;
            e.Handled = !int.TryParse(e.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out hexNumber);
        }

        private void ComboBoxTxNode_DropDownClosed(object sender, EventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row
            if (data == null)
            {
                try // this event happens before StatusBarStatus is generated in the window, so it is null. So using try/catch for now.
                {
                   OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "status_bar", statusBar = "Select an ArbID first and try selecting the node again" });
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
            foreach (CanTxData canTxData in transmitMessages.dataGridTxWindow.Items)
            {
                if (data.Key == canTxData.Key)
                {
                    canTxData.Node = comboBox.SelectionBoxItem.ToString();
                    OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "refresh" });
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

        private void TextBoxTxDLC_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text); ;
        }

        private void CheckBoxEditTxAutoTx_Unchecked(object sender, RoutedEventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row, which you can get the items

            if (data == null)
            {
                OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "status_bar", statusBar = "Please select an ArbID to modify" });
                return;
            }
            // need to update the dataGridTx
            foreach (CanTxData row in transmitMessages.dataGridTxWindow.Items)
            {
                if (row.Key == data.Key)
                {
                    OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "unselectAll" });
                    data.AutoTx = false;
                    row.AutoTx = false;

                    dataGridEditTxMessages.Items.Refresh();
                    OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "status_bar", statusBar = "refresh" });
                }
            }
        }

        private void CheckBoxEditTxAutoTx_Checked(object sender, RoutedEventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row, which you can get the items

            if (data == null)
            {
                OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "status_bar", statusBar = "Please select an ArbID to modify" });

                return;
            }
            // need to update the dataGridTx
            // TODO figure out how to get datagrid item using Dependency Properties and Data Binding
            foreach (CanTxData row in transmitMessages.dataGridTxWindow.Items)
            {
                if (row.Key == data.Key)
                {
                    data.AutoTx = true;
                    row.AutoTx = true;

                    dataGridEditTxMessages.Items.Refresh();
                    OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "refresh" });
                }
            }
        }

        private void OnComboBoxTxRateTextChanged(object sender, EventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row
            if (data == null)
            {
                try // this event happens before StatusBarStatus is generated in the window, so it is null. So using try/catch for now.
                {
                    OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "status_bar", statusBar = "Select an ArbID first and try selecting the node again" });
                }
                catch (NullReferenceException)
                {

                }
                return;
            }
            data.Rate = ComboBoxEditTxRate.Text;
            dataGridEditTxMessages.Items.Refresh();
            // update dataGridTx
            foreach (CanTxData canTxData in transmitMessages.dataGridTxWindow.Items)
            {
                if (data.Key == canTxData.Key)
                {
                    canTxData.Rate = ComboBoxEditTxRate.Text;
                    OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "refresh" });
                    break;
                }
            }
        }

        private void CheckBoxRemoteTransmit_Click(object sender, RoutedEventArgs e)
        {
            CanTxData data = dataGridEditTxMessages.SelectedItem as CanTxData; // grabs the current selected row, which you can get the items

            if (data == null)
            {
                OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "status_bar", statusBar = "Please select an ArbID to modify" });
                return;
            }

            data.RTR = (bool)CheckBoxRemoteTransmit.IsChecked;

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
            foreach (CanTxData row in transmitMessages.dataGridTxWindow.Items)
            {
                if (row.Key == data.Key)
                {
                    row.RTR = data.RTR;
                    OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "refresh" });
                }
            }
        }

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
