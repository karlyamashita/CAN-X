using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
    /// Interaction logic for SignalsMessages.xaml
    /// </summary>
    public partial class SignalsMessages : UserControl
    {
        public ObservableCollection<ComboBoxItem> AvailableItems { get; set; }

        public SignalsMessages()
        {
            InitializeComponent();

            InitCanSignalGrid(); // testing data

            AvailableItems = new ObservableCollection<ComboBoxItem>
            {
                new ComboBoxItem { Id = 1, Name = "Analog" },
                new ComboBoxItem { Id = 2, Name = "Digital" },
                new ComboBoxItem { Id = 3, Name = "State Encoded" },
                new ComboBoxItem { Id = 4, Name = "Text" }
            };

            
        }

        private void InitCanSignalGrid()
        {
            var rows = new List<CanSignalRow>
            {
                new CanSignalRow
                {
                    Description = "Key State",
                    Byte1Bit2 = "1",
                    Byte1Bit1 = "1",
                },
                new CanSignalRow
                {
                    Description = "Key Sense",
                    Byte1Bit0 = "1"
                },
                new CanSignalRow
                {
                    Description = "Window Operation",
                    Byte2Bit0 = "1"
                }
            };

            CanGrid.ItemsSource = rows;
        }
    }

    
    public class CanSignalRow
    {
        public string Description { get; set; }

        public string Type { get; set; }
        public string Byte1Bit7 { get; set; }
        public string Byte1Bit6 { get; set; }
        public string Byte1Bit5 { get; set; }
        public string Byte1Bit4 { get; set; }
        public string Byte1Bit3 { get; set; }
        public string Byte1Bit2 { get; set; }
        public string Byte1Bit1 { get; set; }
        public string Byte1Bit0 { get; set; }

        // Repeat for Byte2 … Byte8
        public string Byte2Bit7 { get; set; }
        public string Byte2Bit6 { get; set; }
        public string Byte2Bit5 { get; set; }
        public string Byte2Bit4 { get; set; }
        public string Byte2Bit3 { get; set; }
        public string Byte2Bit2 { get; set; }
        public string Byte2Bit1 { get; set; }
        public string Byte2Bit0 { get; set; }
    }

    public class ComboBoxItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
