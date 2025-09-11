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
    /// Interaction logic for CAN_Jammer.xaml
    /// </summary>
    public partial class CAN_Jammer : UserControl
    {
        int rowIndex = 0;
        List<CAN_Jam> can_jam_list = new List<CAN_Jam>();
        private static readonly Regex _binaryRegex = new Regex("[01]+");
        private static readonly Regex HexRegex = new Regex("^[0-9A-F]*$");

        public event EventHandler<CAN_JammerEventArgs> CAN_JammerEvent;

        public class CAN_JammerEventArgs : EventArgs
        {
            public string EventType { get; set; }
            // Add other properties as needed

            public CAN_JammerEventArgs(string eventType)
            {
                EventType = eventType;
            }
        }

        // Helper method to raise the event
        protected virtual void OnMyCustomEvent(CAN_JammerEventArgs e)
        {
            CAN_JammerEvent?.Invoke(this, e);
        }

        public CAN_Jammer()
        {
            InitializeComponent();

            //can_jam_list.Add(new CAN_Jam { ARB_ID = 0x000, ModType = 0 });
        }

        public void Send_CAN_Jam_Parameters()
        {
            // todo: send parameters to device
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
            UInt32 result = 0;

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

            //todo - figure out which text box is changing then edit the correct one below
            switch (senderName)
            {
                case "TextBoxTxDescription":
                    can_jam_data.Description = TextBoxTxDescription.Text;
                    break;
                case "TextBoxArbID":
                    UInt32.TryParse(TextBoxArbID.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
                    can_jam_data.Arb_ID = result;
                    break;
                case "TextBoxModifyByte1":
                    UInt32.TryParse(TextBoxArbID.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
                    can_jam_data.ByteValues[0] = (byte)result;
                    break;


            }
            dataGridCAN_Jam.Items.Refresh();
        }

        private void ButtonAddCAN_JamRow_Click(object sender, RoutedEventArgs e)
        {
            var matchFound = true;
            UInt32 newIndex = 0;
            CAN_Jam can_jam = new CAN_Jam();

            // TODO - need to revist this. Forgot about Key order could be sorted out of order.
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
                    Arb_ID = selectedItem.Arb_ID,
                    CAN_Jam_Node = selectedItem.CAN_Jam_Node,
                    ModType = selectedItem.ModType,
                    ByteToModify = selectedItem.ByteToModify,
                    ByteValues = (byte[])selectedItem.ByteValues?.Clone(),
                    BitsToToggle = (byte[])selectedItem.BitsToToggle?.Clone(),
                    BitsToHigh = (byte[])selectedItem.BitsToHigh?.Clone(),
                    BitsToLow = (byte[])selectedItem.BitsToLow?.Clone()
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
    }

}
