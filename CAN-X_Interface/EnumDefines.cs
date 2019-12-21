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
            SWCAN1,
            LSFTCAN1,
            LIN1,
            ETH1,
            SWCAN2,
            LSFTCAN2
        }

        public enum TxRate
        {
            _100,
            _250,
            _500,
            _1000,
            _2000,
            _5000
        }

        public enum DataByteType
        {
            Hex,
            Binary,
            Decimal,
        }
    }
}
