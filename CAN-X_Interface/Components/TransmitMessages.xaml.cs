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
        MainWindow mainWindow;

        public event EventHandler TransmitMessageSendEvent;
        public event EventHandler TransmitMessageAutoTxEvent;

        public TransmitMessages()
        {
            InitializeComponent();

            mainWindow = Application.Current.MainWindow as MainWindow;
        }

        private void ButtonTxMessage_Click(object sender, RoutedEventArgs e)
        {
            ///TransmitMessageSendEvent?.Invoke(this, EventArgs.Empty);

            mainWindow.TransmitMessages_Send();
        }

        private void CheckBoxAutoTx_Checked(object sender, RoutedEventArgs e)
        {
           // TransmitMessageAutoTxEvent?.Invoke(this, EventArgs.Empty);
            mainWindow.TransmitMessages_AutoTx_Checked();
        }

        private void CheckBoxAutoTx_Unchecked(object sender, RoutedEventArgs e)
        {
            //TransmitMessageAutoTxEvent?.Invoke(this, EventArgs.Empty);
            mainWindow.TransmitMessages_AutoTx_Unchecked();
        }
    }
}
