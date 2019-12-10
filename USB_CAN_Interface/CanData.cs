using System;

namespace CAN_X_CAN_Analyzer
{
    public class CanTxData
    {
        public ulong Key { get; set; }
        public string Description { get; set; }  
        public bool AutoTx { get; set; }
        public string Rate { get; set; }
        public string IDE { get; set; } // vspy3 calls it "Type"
        public int RTR { get; set; }
        public string ArbID { get; set; }         
        public string DLC { get; set; }
        //data bytes
        public string Byte1 { get; set; }
        public string Byte2 { get; set; }
        public string Byte3 { get; set; }
        public string Byte4 { get; set; }
        public string Byte5 { get; set; }
        public string Byte6 { get; set; }
        public string Byte7 { get; set; }
        public string Byte8 { get; set; }

        public string Node { get; set; }
        public string Count { get; set; }
        public string Color { get; set; }      
    }

    public class CanRxData
    {
        public ulong Key { get; set; }
        public ulong Line { get; set; } // when scolling messages this is Line
        public string TimeAbs { get; set; }
        public bool Tx { get; set; } // indicator, image
        public bool Err { get; set; } // indicator, image
        public string Description { get; set; }
        public string IDE { get; set; }
        public int RTR { get; set; }
        public string ArbID { get; set; }
        public string DLC { get; set; }
        //data bytes
        public string Byte1 { get; set; }
        public string Byte2 { get; set; }
        public string Byte3 { get; set; }
        public string Byte4 { get; set; }
        public string Byte5 { get; set; }
        public string Byte6 { get; set; }
        public string Byte7 { get; set; }
        public string Byte8 { get; set; }

        public string Node { get; set; }
        public string ASCII { get; set; }
        public string Count { get; set; }
        public string Color { get; set; }

        public CanRxData()
        {

        }

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
            
            RTR = data[2] & 0x01; // bit0
            
            //data[3] is not used

            UInt32 id = (UInt32)(data[4] | data[5] << 8 | data[6] << 16 | data[7] << 24);
            ArbID = Convert.ToString(id, 16).ToUpper();

            DLC = data[8].ToString();

            if (data[8] >= 1)
            {
                Byte1 = data[10].ToString("X2");
            }
            if (data[8] >= 2)
            {
                Byte2 = data[11].ToString("X2");
            }
            if (data[8] >= 3)
            {
                Byte3 = data[12].ToString("X2");
            }
            if (data[8] >= 4)
            {
                Byte4 = data[13].ToString("X2");
            }
            if (data[8] >= 5)
            {
                Byte5 = data[14].ToString("X2");
            }
            if (data[8] >= 6)
            {
                Byte6 = data[15].ToString("X2");
            }
            if (data[8] >= 7)
            {
                Byte7 = data[16].ToString("X2");
            }
            if (data[8] == 8)
            {
                Byte8 = data[17].ToString("X2");
            }

            // get from RTR byte
            int nodeNumber = data[2] >> 2 & 0x04;
            switch(nodeNumber)
            {
                case 0:
                    Node = "CAN1";
                    break;
                case 1:
                    Node = "CAN2";
                    break;
                case 2:
                    Node = "SWCAN1";
                    break;
                    // todo - add the rest of the nodes
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