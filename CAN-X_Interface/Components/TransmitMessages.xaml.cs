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
            public string EventType { get; }
            // Add other properties as needed

            public TransmitMessagesEventArgs(string eventType)
            {
                EventType = eventType;
            }
        }

        // Helper method to raise the event
        protected virtual void OnTransmitMessagesEvent(TransmitMessagesEventArgs e)
        {
            TransmitMessagesEvent?.Invoke(this, e);
        }

        public TransmitMessages()
        {
            InitializeComponent();
        }

        private void ButtonTxMessage_Click(object sender, RoutedEventArgs e)
        {
            OnTransmitMessagesEvent(new TransmitMessagesEventArgs("ButtonTxMessage_Click"));
        }

        private void CheckBoxAutoTx_Checked(object sender, RoutedEventArgs e)
        {
            OnTransmitMessagesEvent(new TransmitMessagesEventArgs("CheckBoxAutoTx_Checked"));
        }

        private void CheckBoxAutoTx_Unchecked(object sender, RoutedEventArgs e)
        {
            OnTransmitMessagesEvent(new TransmitMessagesEventArgs("CheckBoxAutoTx_Unchecked"));
        }

        private void ToggleButtonAutoTx_Click(object sender, RoutedEventArgs e)
        {
            OnTransmitMessagesEvent(new TransmitMessagesEventArgs("ToggleButtonAutoTx_Clicked"));
        }

        private void ButtonDetachDataGrid_Click(object sender, RoutedEventArgs e)
        {
            var parentContainer = this.Parent as ContentControl; // Or Grid, StackPanel, etc.

            if (parentContainer != null)
            {
                var width = parentContainer.ActualWidth;
                var height = parentContainer.ActualHeight;

                // Detach UserControl from parent
                parentContainer.Content = null;

                // Create new window
                Window newWindow = new Window
                {
                    Title = "UserControl in New Window",
                    Content = this, // Assign UserControl to new window
                    Width = width + 20,
                    Height = height + 20,
                    ResizeMode = ResizeMode.CanResizeWithGrip, 
                };

                // Handle closing event to return UserControl to parent
                newWindow.Closing += (s, args) =>
                {
                    if (parentContainer != null)
                    {
                        parentContainer.Content = this; // Re-attach UserControl to parent
                        ButtonDetachDataGrid.Visibility = Visibility.Visible;
                    }
                };
                ButtonDetachDataGrid.Visibility = Visibility.Collapsed;
                newWindow.Show(); // Or newWindow.ShowDialog();
            }
        }
    }
}
