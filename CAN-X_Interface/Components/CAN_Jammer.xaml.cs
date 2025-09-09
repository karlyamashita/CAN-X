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
    /// Interaction logic for CAN_Jammer.xaml
    /// </summary>
    public partial class CAN_Jammer : UserControl
    {

        List<CAN_Jam> can_jam_list = new List<CAN_Jam>();

        public CAN_Jammer()
        {
            InitializeComponent();

            //can_jam_list.Add(new CAN_Jam { ARB_ID = 0x000, ModType = 0 });
        }

        public void Send_CAN_Jam_Parameters()
        {
            // todo: send parameters to device
        }
    }
}
