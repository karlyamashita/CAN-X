using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAN_X_CAN_Analyzer
{
    public class Devices_Data
    {

        public ulong Key { get; set; } = 0;
        public string Description { get; set; } = "";

        public string COM_Port { get; set; } = "";

        public string Node { get; set; } = "";

        public string APB1_Clock { get; set; } = "";

        public string Baud_Rate { get; set; } = "";

        public string CAN_Mode { get; set; } = "";

        public string Connected_Status { get; set; } = "";


        public Devices_Data()
        {

        }



    }
}
