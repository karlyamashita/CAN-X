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
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public event EventHandler<MenuBarEventArgs> MenuBarEvent;

        public class MenuBarEventArgs : EventArgs
        {
            public string EventType { get; set; }
            // Add other properties as needed

            public MenuBarEventArgs(string eventType)
            {
                EventType = eventType;
            }
        }

        // Helper method to raise the event
        protected virtual void OnMyCustomEvent(MenuBarEventArgs e)
        {
            MenuBarEvent?.Invoke(this, e);
        }

        public MenuBar()
        {
            InitializeComponent();
        }

        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new MenuBarEventArgs("MenuItemNew"));
        }

        private void MenuItemOpenProject_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new MenuBarEventArgs("MenuItemOpen"));
        }

        private void MenuItemSaveProject_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new MenuBarEventArgs("MenuItemSave"));
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new MenuBarEventArgs("MenuItemExit"));
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new MenuBarEventArgs("MenuItemAbout"));
        }

        private void MenuItemTxPanel_Click(object sender, RoutedEventArgs e)
        {
            OnMyCustomEvent(new MenuBarEventArgs("MenuItemTxPanel"));
        }
    }
}
