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
            CAN3,
            CAN4
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

        public enum CAN_Mode
        {
            Normal,
            Loopback,
            Silent
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

        public enum FDCAN_Data_Length
        {
            FDCAN_DLC_BYTES_0,
            FDCAN_DLC_BYTES_1,
            FDCAN_DLC_BYTES_2,
            FDCAN_DLC_BYTES_3,
            FDCAN_DLC_BYTES_4,
            FDCAN_DLC_BYTES_5,
            FDCAN_DLC_BYTES_6,
            FDCAN_DLC_BYTES_7,
            FDCAN_DLC_BYTES_8,
            FDCAN_DLC_BYTES_12,
            FDCAN_DLC_BYTES_16,
            FDCAN_DLC_BYTES_20,
            FDCAN_DLC_BYTES_24,
            FDCAN_DLC_BYTES_32,
            FDCAN_DLC_BYTES_48,
            FDCAN_DLC_BYTES_64
        }
    }
}
