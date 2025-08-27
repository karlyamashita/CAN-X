using System;
using System.Collections.Generic;
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
using static CAN_X_CAN_Analyzer.Components.TransmitMessages;

namespace CAN_X_CAN_Analyzer.Components
{
    /// <summary>
    /// Interaction logic for COM_Connection.xaml
    /// </summary>
    public partial class COM_Connection : UserControl
    {
        public event EventHandler<COM_ConnectionEventArgs> COM_ConnectionEvent;

        public class COM_ConnectionEventArgs : EventArgs
        {
            public string EventType { get; set; }
            // Add other properties as needed
        }

        // Helper method to raise the event
        protected virtual void OnMyCustomEvent(COM_ConnectionEventArgs e)
        {
            COM_ConnectionEvent?.Invoke(this, e);
        }

        public COM_Connection()
        {
            InitializeComponent();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new COM_ConnectionEventArgs { EventType = "ButtonConnect" });
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new COM_ConnectionEventArgs { EventType = "ButtonDisconnect" });
        }

        private void ComboBoxAPB1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnMyCustomEvent(new COM_ConnectionEventArgs { EventType = "CalculateBTR" });
        }

        private void ComboBoxBaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnMyCustomEvent(new COM_ConnectionEventArgs { EventType = "CalculateBTR" });
        }

        private void ComboBoxMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnMyCustomEvent(new COM_ConnectionEventArgs { EventType = "CalculateBTR" });
        }

        private void ButtonBtrValue_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new COM_ConnectionEventArgs { EventType = "ButtonBtrClicked" });
        }

        private void CheckBoxBlind_Click(object sender, RoutedEventArgs e)
        {
            CAN_X_CAN_Analyzer.Properties.Settings.Default.imBlind = (bool)CheckBoxBlind.IsChecked;
            CAN_X_CAN_Analyzer.Properties.Settings.Default.Save();
            OnMyCustomEvent(new COM_ConnectionEventArgs { EventType = "ResizeDataGridRx" });
        }

        private void CheckBoxAscii_Click(object sender, RoutedEventArgs e)
        {
            CAN_X_CAN_Analyzer.Properties.Settings.Default.ascii = (bool)CheckBoxAscii.IsChecked;
            CAN_X_CAN_Analyzer.Properties.Settings.Default.Save();
            OnMyCustomEvent(new COM_ConnectionEventArgs { EventType = "FormatDataGridColumns" });
        }

        private void CheckBoxNotes_Click(object sender, RoutedEventArgs e)
        {
            CAN_X_CAN_Analyzer.Properties.Settings.Default.notes = (bool)CheckBoxNotes.IsChecked;
            CAN_X_CAN_Analyzer.Properties.Settings.Default.Save();
            OnMyCustomEvent(new COM_ConnectionEventArgs { EventType = "FormatDataGridColumns" });
        }

        private void ToggleButtonAutoTx_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new COM_ConnectionEventArgs { EventType = "ToggleButtonAutoTx" });
        }
    }
}
