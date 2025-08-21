using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAN_X_CAN_Analyzer
{
    public class EnumDefines
    {
        public enum Nodes
        {
            CAN1,
            CAN2,
            CANFD1,
            CANFD2,
            ETH1,            
            LIN1,
            LIN2,
            LSFTCAN1,          
            LSFTCAN2,
            SWCAN1,
            SWCAN2,
        }

        public enum TxRate
        {
            _100,
            _125,
            _250,
            _300,
            _500,
            _750,
            _800,
            _1000,
            _1200,
            _1500,
            _2000,
            _2500,
            _5000,
            _10000
        }

        public enum DataByteType
        {
            Hex,
            Binary,
            Decimal,
        }

        public enum APB1_Freq
        {
            APB1_48mHz,
            APB1_42mHz,
            APB1_36mHz
        }

        public enum Frequency
        {
            _48000000,
            _42000000,
            _36000000
        }
    }
}
