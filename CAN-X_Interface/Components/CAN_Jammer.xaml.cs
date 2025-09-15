using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CAN_X_CAN_Analyzer.Components
{
    /// <summary>
    /// Interaction logic for CAN_Jammer.xaml
    /// </summary>
    public partial class CAN_Jammer : UserControl
    {
        int rowIndex = 0;

        private static readonly Regex _binaryRegex = new Regex("[01]+");
        private static readonly Regex HexRegex = new Regex("^[0-9A-F]*$");

        public event EventHandler<CAN_JammerEventArgs> CAN_JammerEvent;

        public class CAN_JammerEventArgs : EventArgs
        {
            public string EventType { get; set; }
            // Add other properties as needed
            public byte[] Data { get; set; }
        }

        // Helper method to raise the event
        protected virtual void OnMyCustomEvent(CAN_JammerEventArgs e)
        {
            CAN_JammerEvent?.Invoke(this, e);
        }

        public CAN_Jammer()
        {
            InitializeComponent();
        }

        public void Send_CAN_Jam_Parameters()
        {
            // todo: send parameters to device
            //int count = 0;

            foreach (var items in dataGridCAN_Jam.Items)
            {
                if (items is CAN_Jam item)
                {
                    int size = dataGridCAN_Jam.Items.Count;
                    byte[] data = new byte[40];
                    item.GetBytes().CopyTo(data, 0);
                    /*
                    foreach(byte b in data)
                    {
                        Console.Write(b.ToString("X2"));
                        Console.Write(" ");
                        ++count;
                        if((count %= 40) == 0)
                        {
                            Console.WriteLine();
                            count = 0;
                        }
                    }
                    Console.WriteLine();
                    */

                    OnMyCustomEvent(new CAN_JammerEventArgs { EventType = "CAN_Jam_Parameters", Data = data });
                }
            }
        }

        private void RadioButton_RelayChecked(object sender, RoutedEventArgs e)
        {
            RadioButton selectedRadioButton = sender as RadioButton;
            if (selectedRadioButton != null)
            {
                byte[] data = new byte[2];
                string name = selectedRadioButton.Name;

                switch (name)
                {
                    case "CAN1_RelayDisableFalse":
                        data[0] = 0x00;
                        data[1] = 0x00;
                        break;
                    case "CAN1_RelayDisableTrue":
                        data[0] = 0x00;
                        data[1] = 0x01;
                        break;
                    case "CAN2_RelayDisableFalse":
                        data[0] = 0x01;
                        data[1] = 0x00;
                        break;
                    case "CAN2_RelayDisableTrue":
                        data[0] = 0x01;
                        data[1] = 0x01;
                        break;
                }

                OnMyCustomEvent(new CAN_JammerEventArgs { EventType = "CAN_Jam_Relay", Data = data});
            }
        }

        private void TextBoxBytesToModify_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_binaryRegex.IsMatch(e.Text);
        }

        private void TextBoxPreview_IsHex(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !HexRegex.IsMatch(e.Text);
        }

        private void TextBox_TextChanged_IsBinary(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string originalText = textBox.Text;
            int caretPosition = textBox.CaretIndex;

            textBox.CaretIndex = Math.Min(caretPosition, originalText.Length);

            CAN_Jam can_jam_data = (CAN_Jam)dataGridCAN_Jam.SelectedItem;

            if (can_jam_data == null)
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "You need to select a row" });
                return;
            }
            else
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "" });
            }

            string senderName = textBox.Name;
            string[] bitsToToggleValues = null;
            string[] bitsToHighValues = null;
            string[] bitsToLowValues = null;
            bool bitToggleUpdated = false;
            bool bitHighUpdated = false;
            bool bitLowUpdated = false;

            if (can_jam_data.BitsToToggle == null)
            {
                can_jam_data.BitsToToggle = "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000";
            }
            if (can_jam_data.BitsToHigh == null)
            {
                can_jam_data.BitsToHigh = "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000";
            }
            if (can_jam_data.BitsToLow == null)
            {
                can_jam_data.BitsToLow = "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000";
            }
            bitsToToggleValues = can_jam_data.BitsToToggle.Split(' ');
            bitsToHighValues = can_jam_data.BitsToHigh.Split(' ');
            bitsToLowValues = can_jam_data.BitsToLow.Split(' ');

            //todo - figure out which text box is changing then edit the correct one below
            switch (senderName)
            {
                case "TextBoxBytesToModify":
                    can_jam_data.ByteToModify = TextBoxBytesToModify.Text;
                    break;
                // toggle bits
                case "TextBoxBitsToggleByte1":
                    bitsToToggleValues[0] = TextBoxBitsToggleByte1.Text;
                    bitToggleUpdated = true;
                    break;
                case "TextBoxBitsToggleByte2":
                    bitsToToggleValues[1] = TextBoxBitsToggleByte2.Text;
                    bitToggleUpdated = true;
                    break;
                case "TextBoxBitsToggleByte3":
                    bitsToToggleValues[2] = TextBoxBitsToggleByte3.Text;
                    bitToggleUpdated = true;
                    break;
                case "TextBoxBitsToggleByte4":
                    bitsToToggleValues[3] = TextBoxBitsToggleByte4.Text;
                    bitToggleUpdated = true;
                    break;
                case "TextBoxBitsToggleByte5":
                    bitsToToggleValues[4] = TextBoxBitsToggleByte5.Text;
                    bitToggleUpdated = true;
                    break;
                case "TextBoxBitsToggleByte6":
                    bitsToToggleValues[5] = TextBoxBitsToggleByte6.Text;
                    bitToggleUpdated = true;
                    break;
                case "TextBoxBitsToggleByte7":
                    bitsToToggleValues[6] = TextBoxBitsToggleByte7.Text;
                    bitToggleUpdated = true;
                    break;
                case "TextBoxBitsToggleByte8":
                    bitsToToggleValues[7] = TextBoxBitsToggleByte8.Text;
                    bitToggleUpdated = true;
                    break;
                // high bits
                case "TextBoxBitsToHighByte1":
                    bitsToHighValues[0] = TextBoxBitsToHighByte1.Text;
                    bitHighUpdated = true;
                    break;
                case "TextBoxBitsToHighByte2":
                    bitsToHighValues[1] = TextBoxBitsToHighByte2.Text;
                    bitHighUpdated = true;
                    break;
                case "TextBoxBitsToHighByte3":
                    bitsToHighValues[2] = TextBoxBitsToHighByte3.Text;
                    bitHighUpdated = true;
                    break;
                case "TextBoxBitsToHighByte4":
                    bitsToHighValues[3] = TextBoxBitsToHighByte4.Text;
                    bitHighUpdated = true;
                    break;
                case "TextBoxBitsToHighByte5":
                    bitsToHighValues[4] = TextBoxBitsToHighByte5.Text;
                    bitHighUpdated = true;
                    break;
                case "TextBoxBitsToHighByte6":
                    bitsToHighValues[5] = TextBoxBitsToHighByte6.Text;
                    bitHighUpdated = true;
                    break;
                case "TextBoxBitsToHighByte7":
                    bitsToHighValues[6] = TextBoxBitsToHighByte7.Text;
                    bitHighUpdated = true;
                    break;
                case "TextBoxBitsToHighByte8":
                    bitsToHighValues[7] = TextBoxBitsToHighByte8.Text;
                    bitHighUpdated = true;
                    break;
                case "TextBoxBitsToLowByte1":
                    bitsToLowValues[0] = TextBoxBitsToLowByte1.Text;
                    bitLowUpdated = true;
                    break;
                // low bits
                case "TextBoxBitsToLowByte2":
                    bitsToLowValues[1] = TextBoxBitsToLowByte2.Text;
                    bitLowUpdated = true;
                    break;
                case "TextBoxBitsToLowByte3":
                    bitsToLowValues[2] = TextBoxBitsToLowByte3.Text;
                    bitLowUpdated = true;
                    break;
                case "TextBoxBitsToLowByte4":
                    bitsToLowValues[3] = TextBoxBitsToLowByte4.Text;
                    bitLowUpdated = true;
                    break;
                case "TextBoxBitsToLowByte5":
                    bitsToLowValues[4] = TextBoxBitsToLowByte5.Text;
                    bitLowUpdated = true;
                    break;
                case "TextBoxBitsToLowByte6":
                    bitsToLowValues[5] = TextBoxBitsToLowByte6.Text;
                    bitLowUpdated = true;
                    break;
                case "TextBoxBitsToLowByte7":
                    bitsToLowValues[6] = TextBoxBitsToLowByte7.Text;
                    bitLowUpdated = true;
                    break;
                case "TextBoxBitsToLowByte8":
                    bitsToLowValues[7] = TextBoxBitsToLowByte8.Text;
                    bitLowUpdated = true;
                    break;
            }

            if (bitToggleUpdated)
            {
                can_jam_data.BitsToToggle = bitsToToggleValues[0] + ' ' + bitsToToggleValues[1] + ' ' + bitsToToggleValues[2] + ' ' + bitsToToggleValues[3]
                    + ' ' + bitsToToggleValues[4] + ' ' + bitsToToggleValues[5] + ' ' + bitsToToggleValues[6] + ' ' + bitsToToggleValues[7];
            }
            else if (bitHighUpdated)
            {
                can_jam_data.BitsToHigh = bitsToHighValues[0] + ' ' + bitsToHighValues[1] + ' ' + bitsToHighValues[2] + ' ' + bitsToHighValues[3]
                    + ' ' + bitsToHighValues[4] + ' ' + bitsToHighValues[5] + ' ' + bitsToHighValues[6] + ' ' + bitsToHighValues[7];
            }
            else if (bitLowUpdated)
            {
                can_jam_data.BitsToLow = bitsToLowValues[0] + ' ' + bitsToLowValues[1] + ' ' + bitsToLowValues[2] + ' ' + bitsToLowValues[3]
                    + ' ' + bitsToLowValues[4] + ' ' + bitsToLowValues[5] + ' ' + bitsToLowValues[6] + ' ' + bitsToLowValues[7];
            }

            dataGridCAN_Jam.Items.Refresh();
        }

        private void TextBox_TextChanged_IsHex(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string originalText = textBox.Text;
            int caretPosition = textBox.CaretIndex;

            // Remove non-hex characters and convert to uppercase
            string newText = Regex.Replace(originalText, "[^0-9A-Fa-f]", "").ToUpper();

            if (originalText != newText)
            {
                textBox.Text = newText;
                // Adjust caret position if characters were removed before it
                textBox.CaretIndex = Math.Min(caretPosition, newText.Length);
            }

            CAN_Jam can_jam_data = (CAN_Jam)dataGridCAN_Jam.SelectedItem;

            if (can_jam_data == null)
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "You need to select a row" });
                return;
            }
            else
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "" });
            }

            string senderName = textBox.Name;
            string[] hexValues = null;
            bool updated = false;

            if(can_jam_data.ByteValues == null)
            {
                can_jam_data.ByteValues = "00 00 00 00 00 00 00 00";
            }
            hexValues = can_jam_data.ByteValues.Split(' ');

            switch (senderName)
            {
                case "TextBoxArbID":
                    can_jam_data.ArbID = TextBoxArbID.Text;
                    break;
                // modify bytes
                case "TextBoxModifyByte1":            
                    hexValues[0] = TextBoxModifyByte1.Text;
                    updated = true;
                    break;
                case "TextBoxModifyByte2":
                    hexValues[1] = TextBoxModifyByte2.Text;
                    updated = true;
                    break;
                case "TextBoxModifyByte3":
                    hexValues[2] = TextBoxModifyByte3.Text;
                    updated = true;
                    break;
                case "TextBoxModifyByte4":
                    hexValues[3] = TextBoxModifyByte4.Text;
                    updated = true;
                    break;
                case "TextBoxModifyByte5":
                    hexValues[4] = TextBoxModifyByte5.Text;
                    updated = true;
                    break;
                case "TextBoxModifyByte6":
                    hexValues[5] = TextBoxModifyByte6.Text;
                    updated = true;
                    break;
                case "TextBoxModifyByte7":
                    hexValues[6] = TextBoxModifyByte7.Text;
                    updated = true;
                    break;
                case "TextBoxModifyByte8":
                    hexValues[7] = TextBoxModifyByte8.Text;
                    updated = true;
                    break;
            }

            if(updated)
            {
                can_jam_data.ByteValues = hexValues[0] + ' ' + hexValues[1] + ' ' + hexValues[2] + ' ' + hexValues[3]
                    + ' ' + hexValues[4] + ' ' + hexValues[5] + ' ' + hexValues[6] + ' ' + hexValues[7];

            }
            dataGridCAN_Jam.Items.Refresh();
        }

        private void ButtonAddCAN_JamRow_Click(object sender, RoutedEventArgs e)
        {
            var matchFound = true;
            UInt32 newIndex = 0;
            CAN_Jam can_jam = new CAN_Jam();

            // check for available key number
            while (matchFound)
            {
                matchFound = false;
                foreach (var item in dataGridCAN_Jam.Items)
                {
                    var it = item as CAN_Jam;
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
            can_jam.Key = newIndex;

            dataGridCAN_Jam.Items.Add(can_jam);
        }

        private void ButtonDeleteCAN_JamRow_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridCAN_Jam.SelectedItem != null)
            {
                dataGridCAN_Jam.Items.RemoveAt(rowIndex);
            }
        }

        private void ButtonCopyCAN_JamRow_Click(object sender, RoutedEventArgs e)
        {
            UInt32 newIndex = 0;

            if (dataGridCAN_Jam.SelectedItem != null)
            {
                CAN_Jam selectedItem = (CAN_Jam)dataGridCAN_Jam.SelectedItem;

                CAN_Jam newCAN_JamData = new CAN_Jam
                {

                    Description = selectedItem.Description,
                    ArbID = selectedItem.ArbID,
                    Node = selectedItem.Node,
                    RelayDisabled = selectedItem.RelayDisabled,
                    ByteToModify = selectedItem.ByteToModify,               
                    BitsToToggle = selectedItem.BitsToToggle,
                    BitsToHigh =selectedItem.BitsToHigh,
                    BitsToLow = selectedItem.BitsToLow,
                    ByteValues = selectedItem.ByteValues,
                };

                foreach (CAN_Jam cj in dataGridCAN_Jam.Items)
                {
                    if (cj.Key > newIndex)
                    {
                        newIndex = (UInt32)cj.Key;
                    }
                }
                newCAN_JamData.Key = newIndex + 1; // update key before adding item

                dataGridCAN_Jam.Items.Add(newCAN_JamData);
            }
        }

        private void dataGridCAN_Jam_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = null;

            var visParent = VisualTreeHelper.GetParent(e.OriginalSource as FrameworkElement);
            while (dgr == null && visParent != null)
            {
                dgr = visParent as DataGridRow;
                visParent = VisualTreeHelper.GetParent(visParent);
            }
            if (dgr == null) { return; }

            rowIndex = dgr.GetIndex();
        }

        private void dataGridCAN_Jam_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CAN_Jam data = dataGridCAN_Jam.SelectedItem as CAN_Jam; // grabs the current selected row
            if (data == null) return;
            TextBoxKey.Text = data.Key.ToString();
            TextBoxDescription.Text = data.Description;
            TextBoxArbID.Text = data.ArbID ?? "00000000";
            CheckBoxEnableJamming.IsChecked = data.Jam;
            ComboBoxNode.SelectedIndex = int.TryParse(data.Node, out int node) ? node : 0;

            TextBoxBitsToggleByte1.Text = data.BitsToToggle?.Split(' ')[0] ?? "00000000";
            TextBoxBitsToggleByte2.Text = data.BitsToToggle?.Split(' ')[1] ?? "00000000";
            TextBoxBitsToggleByte3.Text = data.BitsToToggle?.Split(' ')[2] ?? "00000000";
            TextBoxBitsToggleByte4.Text = data.BitsToToggle?.Split(' ')[3] ?? "00000000";
            TextBoxBitsToggleByte5.Text = data.BitsToToggle?.Split(' ')[4] ?? "00000000";
            TextBoxBitsToggleByte6.Text = data.BitsToToggle?.Split(' ')[5] ?? "00000000";
            TextBoxBitsToggleByte7.Text = data.BitsToToggle?.Split(' ')[6] ?? "00000000";
            TextBoxBitsToggleByte8.Text = data.BitsToToggle?.Split(' ')[7] ?? "00000000";

            TextBoxBitsToHighByte1.Text = data.BitsToHigh?.Split(' ')[0] ?? "00000000";
            TextBoxBitsToHighByte2.Text = data.BitsToHigh?.Split(' ')[1] ?? "00000000";
            TextBoxBitsToHighByte3.Text = data.BitsToHigh?.Split(' ')[2] ?? "00000000";
            TextBoxBitsToHighByte4.Text = data.BitsToHigh?.Split(' ')[3] ?? "00000000";
            TextBoxBitsToHighByte5.Text = data.BitsToHigh?.Split(' ')[4] ?? "00000000";
            TextBoxBitsToHighByte6.Text = data.BitsToHigh?.Split(' ')[5] ?? "00000000";
            TextBoxBitsToHighByte7.Text = data.BitsToHigh?.Split(' ')[6] ?? "00000000";
            TextBoxBitsToHighByte8.Text = data.BitsToHigh?.Split(' ')[7] ?? "00000000";

            TextBoxBitsToLowByte1.Text = data.BitsToLow?.Split(' ')[0] ?? "00000000";
            TextBoxBitsToLowByte2.Text = data.BitsToLow?.Split(' ')[1] ?? "00000000";
            TextBoxBitsToLowByte3.Text = data.BitsToLow?.Split(' ')[2] ?? "00000000";
            TextBoxBitsToLowByte4.Text = data.BitsToLow?.Split(' ')[3] ?? "00000000";
            TextBoxBitsToLowByte5.Text = data.BitsToLow?.Split(' ')[4] ?? "00000000";
            TextBoxBitsToLowByte6.Text = data.BitsToLow?.Split(' ')[5] ?? "00000000";
            TextBoxBitsToLowByte7.Text = data.BitsToLow?.Split(' ')[6] ?? "00000000";
            TextBoxBitsToLowByte8.Text = data.BitsToLow?.Split(' ')[7] ?? "00000000";

            TextBoxModifyByte1.Text = data.ByteValues?.Split(' ')[0] ?? "00";
            TextBoxModifyByte2.Text = data.ByteValues?.Split(' ')[1] ?? "00";
            TextBoxModifyByte3.Text = data.ByteValues?.Split(' ')[2] ?? "00";
            TextBoxModifyByte4.Text = data.ByteValues?.Split(' ')[3] ?? "00";
            TextBoxModifyByte5.Text = data.ByteValues?.Split(' ')[4] ?? "00";
            TextBoxModifyByte6.Text = data.ByteValues?.Split(' ')[5] ?? "00";
            TextBoxModifyByte7.Text = data.ByteValues?.Split(' ')[6] ?? "00";
            TextBoxModifyByte8.Text = data.ByteValues?.Split(' ')[7] ?? "00";

            TextBoxBytesToModify.Text = data.ByteToModify ?? "00000000";
        }

        private void Button_UpdateCANJam_Click(object sender, RoutedEventArgs e)
        {
            Send_CAN_Jam_Parameters();
        }

        private void ComboBoxNode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if(comboBox.SelectedItem == null)
            {
                return;
            }

            CAN_Jam can_jam_data = (CAN_Jam)dataGridCAN_Jam.SelectedItem;
            if (can_jam_data == null)
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "You need to select a row" });
                return;
            }
            else
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "" });
            }
            string senderName = comboBox.Name;
            switch (senderName)
            {
                case "ComboBoxNode":
                    can_jam_data.Node = comboBox.SelectedIndex.ToString();
                    Console.WriteLine("ComboBox Selected Item: " + can_jam_data.Node);
                    break;

                    /*case "ComboBoxRelayDisabled":
                        can_jam_data.RelayDisabled = (string)((ComboBoxItem)comboBox.SelectedItem).Content;
                        break;*/  
            }
            dataGridCAN_Jam.Items.Refresh();
        }

        private void CheckBoxEnableJamming_Click(object sender, RoutedEventArgs e)
        {
            CAN_Jam data = dataGridCAN_Jam.SelectedItem as CAN_Jam; // grabs the current selected row
            if (data == null) return;

            if (CheckBoxEnableJamming.IsChecked == true)
            {
                data.Jam = true;
            }
            else
            {
                data.Jam = false;
            }

            dataGridCAN_Jam.Items.Refresh();
        }
    }
}
