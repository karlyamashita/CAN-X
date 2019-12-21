using System;

namespace CAN_X_CAN_Analyzer
{
    public class CanTxData
    {
        public ulong Key { get; set; } = 0;
        public string Description { get; set; } = "";
        public bool AutoTx { get; set; } = false;
        public string Rate { get; set; } = "0";
        public double RateTimer { get; set; } = 0;
        public string IDE { get; set; } = "";
        public int RTR { get; set; } = 0;
        public string ArbID { get; set; } = "";
        public string DLC { get; set; } = "";
        //data bytes
        public string Byte1 { get; set; } = "";
        public string Byte2 { get; set; } = "";
        public string Byte3 { get; set; } = "";
        public string Byte4 { get; set; } = "";
        public string Byte5 { get; set; } = "";
        public string Byte6 { get; set; } = "";
        public string Byte7 { get; set; } = "";
        public string Byte8 { get; set; } = "";

        public string Node { get; set; } = "CAN1";
        public string Count { get; set; } = "1";
        public string Notes { get; set; } = "";
        public string Color { get; set; } = "";

        public CanTxData()
        {

        }

        public CanTxData(CanTxData canTxData) // makes a copy
        {
            IDE = canTxData.IDE;
            Description = canTxData.Description;
            RTR = canTxData.RTR;
            ArbID = canTxData.ArbID;
            DLC = canTxData.DLC;
            Byte1 = canTxData.Byte1;
            Byte2 = canTxData.Byte2;
            Byte3 = canTxData.Byte3;
            Byte4 = canTxData.Byte4;
            Byte5 = canTxData.Byte5;
            Byte6 = canTxData.Byte6;
            Byte7 = canTxData.Byte7;
            Byte8 = canTxData.Byte8;
            Node = canTxData.Node;
        }
    }

    public class CanRxData
    {
        public ulong Key { get; set; } = 0;
        public ulong Line { get; set; } = 0;
        public string TimeAbs { get; set; } = "";
        public bool Tx { get; set; } = false;
        public bool Err { get; set; } = false;
        public string Description { get; set; } = "";
        public string IDE { get; set; } = "";
        public int RTR { get; set; } = 0;
        public string ArbID { get; set; } = "";
        public string DLC { get; set; } = "";
        //data bytes
        public string Byte1 { get; set; } = "";
        public string Byte2 { get; set; } = "";
        public string Byte3 { get; set; } = "";
        public string Byte4 { get; set; } = "";
        public string Byte5 { get; set; } = "";
        public string Byte6 { get; set; } = "";
        public string Byte7 { get; set; } = "";
        public string Byte8 { get; set; } = "";

        public string Node { get; set; } = "CAN1";
        public string ASCII { get; set; } = "";
        public string Count { get; set; } = "1";
        public string CountSaved { get; set; } = "1";
        public string TxCount { get; set; } = "1";
        public string TxCountSaved { get; set; } = "1";
        public string Notes { get; set; } = "";
        public string Color { get; set; } = "";

        public CanRxData()
        {

        }

        // The structure of the data from device
        public CanRxData(byte[] data)
        {
            #region CanRxData(byte[] data)
            if (data[1] == 0)
            {
                IDE = "S";
            }
            else
            {
                IDE = "X";
            }
            
            // RTR
            RTR = data[2] & 0x01; // bit0
            
            // Node
            int nodeNumber = data[3] & 0x0F;
            int i = 0;
            foreach (var en in Enum.GetNames(typeof(EnumDefines.Nodes)))
            {
                if(i == nodeNumber)
                {
                    Node = en;
                    break;
                }
                i++;
            }

            UInt32 id = (UInt32)(data[4] | data[5] << 8 | data[6] << 16 | data[7] << 24);
            ArbID = Convert.ToString(id, 16).ToUpper();

            DLC = data[8].ToString();

            if (data[8] >= 1)
            {
                Byte1 = data[9].ToString("X2");
            }
            if (data[8] >= 2)
            {
                Byte2 = data[10].ToString("X2");
            }
            if (data[8] >= 3)
            {
                Byte3 = data[11].ToString("X2");
            }
            if (data[8] >= 4)
            {
                Byte4 = data[12].ToString("X2");
            }
            if (data[8] >= 5)
            {
                Byte5 = data[13].ToString("X2");
            }
            if (data[8] >= 6)
            {
                Byte6 = data[14].ToString("X2");
            }
            if (data[8] >= 7)
            {
                Byte7 = data[15].ToString("X2");
            }
            if (data[8] == 8)
            {
                Byte8 = data[16].ToString("X2");
            }

            #endregion
        }

        public CanRxData(CanTxData canTxData)
        {
            IDE = canTxData.IDE;
            Description = canTxData.Description;
            ArbID = canTxData.ArbID;
            DLC = canTxData.DLC;
            Byte1 = canTxData.Byte1;
            Byte2 = canTxData.Byte2;
            Byte3 = canTxData.Byte3;
            Byte4 = canTxData.Byte4;
            Byte5 = canTxData.Byte5;
            Byte6 = canTxData.Byte6;
            Byte7 = canTxData.Byte7;
            Byte8 = canTxData.Byte8;
            Node = canTxData.Node;
        }

        public CanRxData(CanRxData canRxData)
        {
            Key = canRxData.Key;
            Line = canRxData.Line;
            TimeAbs = canRxData.TimeAbs;
            Tx = canRxData.Tx;
            Err = canRxData.Err;
            Description = canRxData.Description;
            IDE = canRxData.IDE;
            RTR = canRxData.RTR;
            ArbID = canRxData.ArbID;
            DLC = canRxData.DLC;
            Byte1 = canRxData.Byte1;
            Byte2 = canRxData.Byte2;
            Byte3 = canRxData.Byte3;
            Byte4 = canRxData.Byte4;
            Byte5 = canRxData.Byte5;
            Byte6 = canRxData.Byte6;
            Byte7 = canRxData.Byte7;
            Byte8 = canRxData.Byte8;
            Node = canRxData.Node;
            Count = canRxData.Count;
        }
    }
}