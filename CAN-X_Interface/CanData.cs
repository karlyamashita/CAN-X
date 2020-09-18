using System;
using System.ComponentModel;

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
        public bool RTR { get; set; } = false;
        public string ArbID { get; set; } = "";
        public string DLC { get; set; } = "";
        //data bytes

        private string byte1 = "";
        public string Byte1 
        {
            get 
            {
                return byte1; 
            }
            set
            {
                byte1 = value;
            }          
        }

        private string byte2 = "";
        public string Byte2
        {
            get
            {
                return byte2;
            }
            set
            {
                byte2 = value;
            }
        }

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

        public CanTxData(CanRxData canRxData)
        {
            IDE = canRxData.IDE;
            Description = canRxData.Description;
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
        }
    }

    public class CanRxData : INotifyPropertyChanged
    {
        public ulong Key { get; set; } = 0;

        private UInt32 line = 0;
        public UInt32 Line
        {
            get
            {
                return line;
            }
            set
            {
                if (line == value) return;
                line = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Line"));
            }
        }

        private string timeAbs = "";
        public string TimeAbs
        {
            get
            {
                return timeAbs;
            }
            set
            {
                if (timeAbs == value) return;
                timeAbs = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TimeAbs"));
            }
        }
        public bool Tx { get; set; } = false;
        public bool Err { get; set; } = false;
        public string Description { get; set; } = "";
        public string IDE { get; set; } = "";
        public bool RTR { get; set; } = false;
        public string ArbID { get; set; } = "";

        private string dlc = "";
        public string DLC
        {
            get
            {
                return dlc;
            }
            set
            {
                if (dlc == value) return;
                dlc = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DLC"));
            }
        }
        //data bytes

        private string byte1 = "";
        public string Byte1
        {
            get
            {
                return byte1;
            }
            set
            {
                if (byte1 == value) return;
                byte1 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Byte1"));
            }
        }

        private string byte2 = "";
        public string Byte2
        {
            get
            {
                return byte2;
            }
            set
            {
                if (byte2 == value) return;
                byte2 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Byte2"));
            }
        }

        private string byte3 = "";
        public string Byte3
        {
            get
            {
                return byte3;
            }
            set
            {
                if (byte3 == value) return;
                byte3 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Byte3"));
            }
        }

        private string byte4 = "";
        public string Byte4
        {
            get
            {
                return byte4;
            }
            set
            {
                if (byte4 == value) return;
                byte4 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Byte4"));
            }
        }

        private string byte5 = "";
        public string Byte5
        {
            get
            {
                return byte5;
            }
            set
            {
                if (byte5 == value) return;
                byte5 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Byte5"));
            }
        }

        private string byte6 = "";
        public string Byte6
        {
            get
            {
                return byte6;
            }
            set
            {
                if (byte6 == value) return;
                byte6 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Byte6"));
            }
        }

        private string byte7 = "";
        public string Byte7
        {
            get
            {
                return byte7;
            }
            set
            {
                if (byte7 == value) return;
                byte7 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Byte7"));
            }
        }

        private string byte8 = "";
        public string Byte8
        {
            get
            {
                return byte8;
            }
            set
            {
                if (byte8 == value) return;
                byte8 = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Byte8"));
            }
        }


        public string Node { get; set; } = "CAN1";
        public string ASCII { get; set; } = "";

        private string _Count = "1";
        public string Count
        {
            get
            {
                return _Count;
            }
            set
            {
                if (_Count == value) return;
                _Count = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            }
        }
        public string CountSaved { get; set; } = "1";

        private string _TxCount = "1";
        public string TxCount
        {
            get
            {
                return _TxCount;
            }
            set
            {
                if (_TxCount == value) return;
                _TxCount = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TxCount"));
            }
        }
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
            
            RTR = (data[2] & 0x01) == 1 ? true: false; // bit0
            
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}