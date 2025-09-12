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
                    byte[] data = new byte[size * 40];
                    item.GetBytes().CopyTo(data, item.Key * 40);
                    /*
                    foreach(byte b in data)
                    {
                        Console.Write(b.ToString("X"));
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

            //todo - figure out which text box is changing then edit the correct one below
            switch (senderName)
            {
                case "TextBoxBytesToModify":
                    can_jam_data.ByteToModify = TextBoxBytesToModify.Text;
                    break;
                case "TextBoxBitsToggleByte1":
                case "TextBoxBitsToggleByte2":
                case "TextBoxBitsToggleByte3":
                case "TextBoxBitsToggleByte4":
                case "TextBoxBitsToggleByte5":
                case "TextBoxBitsToggleByte6":
                case "TextBoxBitsToggleByte7":
                case "TextBoxBitsToggleByte8":
                    can_jam_data.BitsToToggle = TextBoxBitsToggleByte1.Text + " " + TextBoxBitsToggleByte2.Text
                        + " " + TextBoxBitsToggleByte3.Text + " " + TextBoxBitsToggleByte4.Text
                        + " " + TextBoxBitsToggleByte5.Text + " " + TextBoxBitsToggleByte6.Text
                        + " " + TextBoxBitsToggleByte7.Text + " " + TextBoxBitsToggleByte8.Text;
                    break;
                case "TextBoxBitsToHighByte1":
                case "TextBoxBitsToHighByte2":
                case "TextBoxBitsToHighByte3":
                case "TextBoxBitsToHighByte4":
                case "TextBoxBitsToHighByte5":
                case "TextBoxBitsToHighByte6":
                case "TextBoxBitsToHighByte7":
                case "TextBoxBitsToHighByte8":
                    can_jam_data.BitsToHigh = TextBoxBitsToHighByte1.Text + " " + TextBoxBitsToHighByte2.Text
                        + " " + TextBoxBitsToHighByte3.Text + " " + TextBoxBitsToHighByte4.Text
                        + " " + TextBoxBitsToHighByte5.Text + " " + TextBoxBitsToHighByte6.Text
                        + " " + TextBoxBitsToHighByte7.Text + " " + TextBoxBitsToHighByte8.Text;
                    break;
                case "TextBoxBitsToLowByte1":
                case "TextBoxBitsToLowByte2":
                case "TextBoxBitsToLowByte3":
                case "TextBoxBitsToLowByte4":
                case "TextBoxBitsToLowByte5":
                case "TextBoxBitsToLowByte6":
                case "TextBoxBitsToLowByte7":
                case "TextBoxBitsToLowByte8":
                    can_jam_data.BitsToLow = TextBoxBitsToLowByte1.Text + " " + TextBoxBitsToLowByte2.Text
                        + " " + TextBoxBitsToLowByte3.Text + " " + TextBoxBitsToLowByte4.Text
                        + " " + TextBoxBitsToLowByte5.Text + " " + TextBoxBitsToLowByte6.Text
                        + " " + TextBoxBitsToLowByte7.Text + " " + TextBoxBitsToLowByte8.Text;
                    break;



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


            switch (senderName)
            {
                case "TextBoxArbID":
                    can_jam_data.ArbID = TextBoxArbID.Text;
                    break;
                case "TextBoxModifyByte1":
                case "TextBoxModifyByte2":
                case "TextBoxModifyByte3":
                case "TextBoxModifyByte4":
                case "TextBoxModifyByte5":
                case "TextBoxModifyByte6":
                case "TextBoxModifyByte7":
                case "TextBoxModifyByte8":
                    can_jam_data.ByteValues = TextBoxModifyByte1.Text + " " + TextBoxModifyByte2.Text
                        + " " + TextBoxModifyByte3.Text + " " + TextBoxModifyByte4.Text
                        + " " + TextBoxModifyByte5.Text + " " + TextBoxModifyByte6.Text
                        + " " + TextBoxModifyByte7.Text + " " + TextBoxModifyByte8.Text;
                    break;
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
    }
}
