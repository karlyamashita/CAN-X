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

namespace CAN_X_CAN_Analyzer.Components
{
    /// <summary>
    /// Interaction logic for TransmitMessages.xaml
    /// </summary>
    public partial class TransmitMessages : UserControl
    {
        public event EventHandler<TransmitMessagesEventArgs> TransmitMessagesEvent;

        public class TransmitMessagesEventArgs : EventArgs
        {
            public string EventType { get; set; }
            // Add other properties as needed
        }

        // Helper method to raise the event
        protected virtual void OnMyCustomEvent(TransmitMessagesEventArgs e)
        {
            TransmitMessagesEvent?.Invoke(this, e);
        }

        public TransmitMessages()
        {
            InitializeComponent();
        }

        private void ButtonTxMessage_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new TransmitMessagesEventArgs { EventType = "ButtonTxMessage_Click" });
        }

        private void CheckBoxAutoTx_Checked(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new TransmitMessagesEventArgs { EventType = "CheckBoxAutoTx_Checked" });
            }

        private void CheckBoxAutoTx_Unchecked(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new TransmitMessagesEventArgs { EventType = "CheckBoxAutoTx_Unchecked" });
            }
    }
}
