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

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
            }
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TransmitMessages)))
            {
                if (e.Data.GetData(typeof(TransmitMessages)) is TransmitMessages draggedControl)
                {
                    if (draggedControl.Parent is Panel parentPanel)
                    {
                        parentPanel.Children.Remove(draggedControl);
                    }

                    Window newWindow = new Window
                    {
                        Content = draggedControl,
                        Width = draggedControl.ActualWidth + 20,
                        Height = draggedControl.ActualHeight + 20,
                        Title = "Transmit Messages",
                    //    Owner = Window.GetWindow(this) // Set owner if possible
                    };
                    newWindow.Show();
                    newWindow.Closed += (s, args) =>
                    {
                        // Optionally, you can add the control back to its original parent when the window is closed
                        if (this.Parent is Panel originalParent)
                        {
                            originalParent.Children.Add(draggedControl);
                        }
                    };
                }
            }
        }


    }
}
