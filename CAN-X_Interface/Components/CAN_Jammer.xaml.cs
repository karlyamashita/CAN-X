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
            int result = 0;
            string value = null;
            TextBoxKey.Text = data.Key.ToString();
            TextBoxDescription.Text = data.Description;
            TextBoxArbID.Text = data.ArbID ?? "00000000";
            CheckBoxEnableJamming.IsChecked = data.Jam;
            ComboBoxNode.SelectedIndex = int.TryParse(data.Node, out int node) ? node : 0;

            // bits to toggle
            value = data.BitsToToggle?.Split(' ')[0] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToToggle_Byte_1_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToToggle_Byte_1_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToToggle_Byte_1_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToToggle_Byte_1_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToToggle_Byte_1_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToToggle_Byte_1_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToToggle_Byte_1_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToToggle_Byte_1_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToToggle?.Split(' ')[1] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToToggle_Byte_2_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToToggle_Byte_2_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToToggle_Byte_2_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToToggle_Byte_2_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToToggle_Byte_2_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToToggle_Byte_2_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToToggle_Byte_2_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToToggle_Byte_2_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToToggle?.Split(' ')[2] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToToggle_Byte_3_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToToggle_Byte_3_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToToggle_Byte_3_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToToggle_Byte_3_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToToggle_Byte_3_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToToggle_Byte_3_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToToggle_Byte_3_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToToggle_Byte_3_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToToggle?.Split(' ')[3] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToToggle_Byte_4_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToToggle_Byte_4_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToToggle_Byte_4_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToToggle_Byte_4_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToToggle_Byte_4_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToToggle_Byte_4_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToToggle_Byte_4_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToToggle_Byte_4_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToToggle?.Split(' ')[4] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToToggle_Byte_5_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToToggle_Byte_5_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToToggle_Byte_5_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToToggle_Byte_5_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToToggle_Byte_5_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToToggle_Byte_5_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToToggle_Byte_5_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToToggle_Byte_5_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToToggle?.Split(' ')[5] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToToggle_Byte_6_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToToggle_Byte_6_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToToggle_Byte_6_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToToggle_Byte_6_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToToggle_Byte_6_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToToggle_Byte_6_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToToggle_Byte_6_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToToggle_Byte_6_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToToggle?.Split(' ')[6] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToToggle_Byte_7_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToToggle_Byte_7_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToToggle_Byte_7_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToToggle_Byte_7_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToToggle_Byte_7_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToToggle_Byte_7_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToToggle_Byte_7_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToToggle_Byte_7_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToToggle?.Split(' ')[7] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToToggle_Byte_8_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToToggle_Byte_8_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToToggle_Byte_8_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToToggle_Byte_8_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToToggle_Byte_8_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToToggle_Byte_8_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToToggle_Byte_8_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToToggle_Byte_8_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            // bits to high
            value = data.BitsToHigh?.Split(' ')[0] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToHigh_Byte_1_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToHigh_Byte_1_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToHigh_Byte_1_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToHigh_Byte_1_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToHigh_Byte_1_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToHigh_Byte_1_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToHigh_Byte_1_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToHigh_Byte_1_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToHigh?.Split(' ')[1] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToHigh_Byte_2_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToHigh_Byte_2_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToHigh_Byte_2_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToHigh_Byte_2_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToHigh_Byte_2_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToHigh_Byte_2_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToHigh_Byte_2_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToHigh_Byte_2_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToHigh?.Split(' ')[2] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToHigh_Byte_3_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToHigh_Byte_3_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToHigh_Byte_3_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToHigh_Byte_3_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToHigh_Byte_3_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToHigh_Byte_3_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToHigh_Byte_3_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToHigh_Byte_3_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToHigh?.Split(' ')[3] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToHigh_Byte_4_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToHigh_Byte_4_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToHigh_Byte_4_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToHigh_Byte_4_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToHigh_Byte_4_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToHigh_Byte_4_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToHigh_Byte_4_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToHigh_Byte_4_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToHigh?.Split(' ')[4] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToHigh_Byte_5_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToHigh_Byte_5_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToHigh_Byte_5_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToHigh_Byte_5_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToHigh_Byte_5_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToHigh_Byte_5_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToHigh_Byte_5_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToHigh_Byte_5_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToHigh?.Split(' ')[5] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToHigh_Byte_6_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToHigh_Byte_6_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToHigh_Byte_6_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToHigh_Byte_6_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToHigh_Byte_6_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToHigh_Byte_6_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToHigh_Byte_6_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToHigh_Byte_6_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToHigh?.Split(' ')[6] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToHigh_Byte_7_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToHigh_Byte_7_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToHigh_Byte_7_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToHigh_Byte_7_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToHigh_Byte_7_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToHigh_Byte_7_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToHigh_Byte_7_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToHigh_Byte_7_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToHigh?.Split(' ')[7] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToHigh_Byte_8_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToHigh_Byte_8_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToHigh_Byte_8_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToHigh_Byte_8_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToHigh_Byte_8_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToHigh_Byte_8_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToHigh_Byte_8_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToHigh_Byte_8_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            // bits to low
            value = data.BitsToLow?.Split(' ')[0] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToLow_Byte_1_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToLow_Byte_1_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToLow_Byte_1_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToLow_Byte_1_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToLow_Byte_1_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToLow_Byte_1_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToLow_Byte_1_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToLow_Byte_1_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToLow?.Split(' ')[1] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToLow_Byte_2_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToLow_Byte_2_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToLow_Byte_2_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToLow_Byte_2_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToLow_Byte_2_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToLow_Byte_2_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToLow_Byte_2_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToLow_Byte_2_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToLow?.Split(' ')[2] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToLow_Byte_3_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToLow_Byte_3_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToLow_Byte_3_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToLow_Byte_3_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToLow_Byte_3_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToLow_Byte_3_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToLow_Byte_3_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToLow_Byte_3_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToLow?.Split(' ')[3] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToLow_Byte_4_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToLow_Byte_4_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToLow_Byte_4_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToLow_Byte_4_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToLow_Byte_4_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToLow_Byte_4_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToLow_Byte_4_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToLow_Byte_4_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToLow?.Split(' ')[4] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToLow_Byte_5_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToLow_Byte_5_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToLow_Byte_5_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToLow_Byte_5_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToLow_Byte_5_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToLow_Byte_5_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToLow_Byte_5_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToLow_Byte_5_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToLow?.Split(' ')[5] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToLow_Byte_6_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToLow_Byte_6_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToLow_Byte_6_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToLow_Byte_6_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToLow_Byte_6_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToLow_Byte_6_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToLow_Byte_6_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToLow_Byte_6_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToLow?.Split(' ')[6] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToLow_Byte_7_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToLow_Byte_7_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToLow_Byte_7_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToLow_Byte_7_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToLow_Byte_7_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToLow_Byte_7_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToLow_Byte_7_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToLow_Byte_7_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;

            value = data.BitsToLow?.Split(' ')[7] ?? "00";
            result = Convert.ToInt32(value, 16);
            CheckBoxBitToLow_Byte_8_Bit_1.IsChecked = (result & 0x80) == 0x80 ? true : false;
            CheckBoxBitToLow_Byte_8_Bit_2.IsChecked = (result & 0x40) == 0x40 ? true : false;
            CheckBoxBitToLow_Byte_8_Bit_3.IsChecked = (result & 0x20) == 0x20 ? true : false;
            CheckBoxBitToLow_Byte_8_Bit_4.IsChecked = (result & 0x10) == 0x10 ? true : false;
            CheckBoxBitToLow_Byte_8_Bit_5.IsChecked = (result & 0x08) == 0x08 ? true : false;
            CheckBoxBitToLow_Byte_8_Bit_6.IsChecked = (result & 0x04) == 0x04 ? true : false;
            CheckBoxBitToLow_Byte_8_Bit_7.IsChecked = (result & 0x02) == 0x02 ? true : false;
            CheckBoxBitToLow_Byte_8_Bit_8.IsChecked = (result & 0x01) == 0x01 ? true : false;


            TextBoxModifyByte1.Text = data.ByteValues?.Split(' ')[0] ?? "00";
            TextBoxModifyByte2.Text = data.ByteValues?.Split(' ')[1] ?? "00";
            TextBoxModifyByte3.Text = data.ByteValues?.Split(' ')[2] ?? "00";
            TextBoxModifyByte4.Text = data.ByteValues?.Split(' ')[3] ?? "00";
            TextBoxModifyByte5.Text = data.ByteValues?.Split(' ')[4] ?? "00";
            TextBoxModifyByte6.Text = data.ByteValues?.Split(' ')[5] ?? "00";
            TextBoxModifyByte7.Text = data.ByteValues?.Split(' ')[6] ?? "00";
            TextBoxModifyByte8.Text = data.ByteValues?.Split(' ')[7] ?? "00";

            result = Convert.ToInt32(data.ByteToModify, 16);
            CheckBoxBytesToModify_1.IsChecked = (result & 0b10000000) == 0b10000000 ? true : false;
            CheckBoxBytesToModify_2.IsChecked = (result & 0b01000000) == 0b01000000 ? true : false;
            CheckBoxBytesToModify_3.IsChecked = (result & 0b00100000) == 0b00100000 ? true : false;
            CheckBoxBytesToModify_4.IsChecked = (result & 0b00010000) == 0b00010000 ? true : false;
            CheckBoxBytesToModify_5.IsChecked = (result & 0b00001000) == 0b00001000 ? true : false;
            CheckBoxBytesToModify_6.IsChecked = (result & 0b00000100) == 0b00000100 ? true : false;
            CheckBoxBytesToModify_7.IsChecked = (result & 0b00000010) == 0b00000010 ? true : false;
            CheckBoxBytesToModify_8.IsChecked = (result & 0b00000001) == 0b00000001 ? true : false;
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

        private void CheckBoxBytesToModify_Checked(object sender, RoutedEventArgs e)
        {
            CAN_Jam can_jam_data = (CAN_Jam)dataGridCAN_Jam.SelectedItem;
            string hexString = string.Empty;

            if (can_jam_data == null)
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "You need to select a row" });
                return;
            }
            else
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "" });
            }

            Int32 decimalValue = Convert.ToInt32((CheckBoxBytesToModify_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBytesToModify_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBytesToModify_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBytesToModify_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBytesToModify_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBytesToModify_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBytesToModify_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBytesToModify_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString = decimalValue.ToString("X2");

            can_jam_data.ByteToModify = hexString;

            dataGridCAN_Jam.Items.Refresh();
        }

        private void CheckBoxBitsToToggle_Checked(object sender, RoutedEventArgs e)
        {
            CAN_Jam can_jam_data = (CAN_Jam)dataGridCAN_Jam.SelectedItem;
            string[] hexString = new string[8];

            if (can_jam_data == null)
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "You need to select a row" });
                return;
            }
            else
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "" });
            }


            Int32 decimalValue = Convert.ToInt32((CheckBoxBitToToggle_Byte_1_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_1_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_1_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_1_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_1_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_1_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_1_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_1_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[0] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToToggle_Byte_2_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_2_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_2_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_2_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_2_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_2_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_2_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_2_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[1] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToToggle_Byte_3_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_3_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_3_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_3_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_3_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_3_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_3_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_3_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[2] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToToggle_Byte_4_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_4_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_4_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_4_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_4_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_4_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_4_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_4_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[3] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToToggle_Byte_5_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_5_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_5_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_5_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_5_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_5_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_5_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_5_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[4] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToToggle_Byte_6_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_6_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_6_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_6_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_6_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_6_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_6_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_6_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[5] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToToggle_Byte_7_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_7_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_7_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_7_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_7_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_7_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_7_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_7_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[6] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToToggle_Byte_8_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_8_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_8_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_8_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_8_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_8_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToToggle_Byte_8_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToToggle_Byte_8_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[7] = decimalValue.ToString("X2");

            can_jam_data.BitsToToggle = hexString[0] + " " + hexString[1] + " " + hexString[2] + " " + hexString[3] + " " + hexString[4] + " " + hexString[5] + " " + hexString[6] + " " + hexString[7];

               

            dataGridCAN_Jam.Items.Refresh();
        }

        private void CheckBoxBitsToHigh_Checked(object sender, RoutedEventArgs e)
        {
            CAN_Jam can_jam_data = (CAN_Jam)dataGridCAN_Jam.SelectedItem;
            string[] hexString = new string[8];

            if (can_jam_data == null)
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "You need to select a row" });
                return;
            }
            else
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "" });
            }

            Int32 decimalValue = Convert.ToInt32((CheckBoxBitToHigh_Byte_1_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_1_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_1_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_1_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_1_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_1_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_1_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_1_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[0] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToHigh_Byte_2_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_2_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_2_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_2_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_2_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_2_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_2_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_2_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[1] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToHigh_Byte_3_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_3_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_3_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_3_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_3_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_3_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_3_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_3_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[2] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToHigh_Byte_4_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_4_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_4_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_4_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_4_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_4_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_4_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_4_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[3] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToHigh_Byte_5_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_5_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_5_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_5_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_5_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_5_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_5_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_5_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[4] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToHigh_Byte_6_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_6_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_6_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_6_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_6_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_6_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_6_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_6_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[5] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToHigh_Byte_7_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_7_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_7_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_7_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_7_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_7_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_7_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_7_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[6] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToHigh_Byte_8_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_8_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_8_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_8_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_8_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_8_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToHigh_Byte_8_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToHigh_Byte_8_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[7] = decimalValue.ToString("X2");

            can_jam_data.BitsToHigh = hexString[0] + " " + hexString[1] + " " + hexString[2] + " " + hexString[3] + " " + hexString[4] + " " + hexString[5] + " " + hexString[6] + " " + hexString[7];

            dataGridCAN_Jam.Items.Refresh();
        }

        private void CheckBoxBitsToLow_Checked(object sender, RoutedEventArgs e)
        {
            CAN_Jam can_jam_data = (CAN_Jam)dataGridCAN_Jam.SelectedItem;
            string[] hexString = new string[8];

            if (can_jam_data == null)
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "You need to select a row" });
                return;
            }
            else
            {
                //OnMyCustomEvent(new EditTxMessagesEventArgs { EventType = "" });
            }

            Int32 decimalValue = Convert.ToInt32((CheckBoxBitToLow_Byte_1_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_1_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_1_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_1_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_1_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_1_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_1_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_1_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[0] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToLow_Byte_2_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_2_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_2_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_2_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_2_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_2_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_2_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_2_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[1] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToLow_Byte_3_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_3_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_3_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_3_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_3_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_3_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_3_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_3_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[2] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToLow_Byte_4_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_4_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_4_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_4_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_4_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_4_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_4_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_4_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[3] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToLow_Byte_5_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_5_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_5_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_5_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_5_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_5_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_5_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_5_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[4] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToLow_Byte_6_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_6_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_6_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_6_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_6_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_6_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_6_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_6_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[5] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToLow_Byte_7_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_7_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_7_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_7_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_7_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_7_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_7_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_7_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[6] = decimalValue.ToString("X2");

            decimalValue = Convert.ToInt32((CheckBoxBitToLow_Byte_8_Bit_1.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_8_Bit_2.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_8_Bit_3.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_8_Bit_4.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_8_Bit_5.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_8_Bit_6.IsChecked == true ? 1 : 0).ToString()
                + (CheckBoxBitToLow_Byte_8_Bit_7.IsChecked == true ? 1 : 0).ToString() + (CheckBoxBitToLow_Byte_8_Bit_8.IsChecked == true ? 1 : 0).ToString(), 2);
            hexString[7] = decimalValue.ToString("X2");

            can_jam_data.BitsToLow = hexString[0] + " " + hexString[1] + " " + hexString[2] + " " + hexString[3] + " " + hexString[4] + " " + hexString[5] + " " + hexString[6] + " " + hexString[7];

            dataGridCAN_Jam.Items.Refresh();
        }

        private void CheckBoxEnableJamming_Checked(object sender, RoutedEventArgs e)
        {
            CAN_Jam data = dataGridCAN_Jam.SelectedItem as CAN_Jam; // grabs the current selected row
            if (data == null) return;

            if (CheckBoxEnableJamming.IsChecked == true)
            {
                data.Jam = true;
                BitsToggle.IsEnabled = false;
                BitsHigh.IsEnabled = false;
                BitsLow.IsEnabled = false;
                BytesToModify.IsEnabled = false;
            }
            else
            {
                data.Jam = false;
                BitsToggle.IsEnabled = true;
                BitsHigh.IsEnabled = true;
                BitsLow.IsEnabled = true;
                BytesToModify.IsEnabled = true;
            }

            dataGridCAN_Jam.Items.Refresh();
        }

        private void ButtonDetachDataGrid_Click(object sender, RoutedEventArgs e)
        {
            var parentContainer = this.Parent as ContentControl; // Or Grid, StackPanel, etc.
            byte[] data = new byte[1];

            if (parentContainer != null)
            {
                // Detach UserControl from parent
                parentContainer.Content = null;

                // Create new window
                Window newWindow = new Window
                {
                    Title = "CAN Jammer",
                    Content = this, // Assign UserControl to new window
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ResizeMode = ResizeMode.CanResizeWithGrip,
                };

                // Handle closing event to return UserControl to parent
                newWindow.Closing += (s, args) =>
                {
                    if (parentContainer != null)
                    {
                        parentContainer.Content = this; // Re-attach UserControl to parent
                        ButtonDetachDataGrid.Visibility = Visibility.Visible;

                        data[0] = 1; // show tab
                        OnMyCustomEvent(new CAN_JammerEventArgs { EventType = "CAN_Jam_Detach", Data = data });
                    }
                };
                ButtonDetachDataGrid.Visibility = Visibility.Collapsed;
                data[0] = 0; // don't show tab
                OnMyCustomEvent(new CAN_JammerEventArgs { EventType = "CAN_Jam_Detach", Data = data });

                newWindow.Show(); // Or newWindow.ShowDialog();

                
            }
        }
    }
}
