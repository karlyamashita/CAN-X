using System;

namespace CAN_X_CAN_Analyzer
{
    public class CanTxData
    {
        public ulong Key { get; set; }
        public string Description { get; set; }  
        public string AutoTx { get; set; }
        public string Rate { get; set; }
        public string IDE { get; set; } // vspy3 calls it "Type"
        public string RTR { get; set; }
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

        public string Color { get; set; }      
    }

    public class CanRxData
    {
        public ulong Key { get; set; }
        public ulong Line { get; set; } // when scolling messages this is Line
        public ulong Count { get; set; } // this is Count when not scolling messages
        public string TimeAbs { get; set; }
        public bool Tx { get; set; } // indicator, image
        public bool Err { get; set; } // indicator, image
        public string Description { get; set; }
        public string IDE { get; set; }
        public string RTR { get; set; }
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

        public int ChangeCnt { get; set; }
        public string Color { get; set; }
    }
}