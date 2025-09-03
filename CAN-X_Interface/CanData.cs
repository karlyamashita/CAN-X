using System;
using System.ComponentModel;
using System.Security.RightsManagement;

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

        public string Byte1 { get; set; } = "";
        public string Byte2 { get; set; } = "";
        public string Byte3 { get; set; } = "";
        public string Byte4 { get; set; } = "";
        public string Byte5 { get; set; } = "";
        public string Byte6 { get; set; } = "";
        public string Byte7 { get; set; } = "";
        public string Byte8 { get; set; } = "";

        // CAN FD
        public string Byte9 { get; set; } = "";
        public string Byte10 { get; set; } = "";
        public string Byte11 { get; set; } = "";
        public string Byte12 { get; set; } = "";
        public string Byte13 { get; set; } = "";
        public string Byte14 { get; set; } = "";
        public string Byte15 { get; set; } = "";
        public string Byte16 { get; set; } = "";
        public string Byte17 { get; set; } = "";
        public string Byte18 { get; set; } = "";
        public string Byte19 { get; set; } = "";    
        public string Byte20 { get; set; } = "";
        public string Byte21 { get; set; } = "";
        public string Byte22 { get; set; } = "";
        public string Byte23 { get; set; } = "";
        public string Byte24 { get; set; } = "";
        public string Byte25 { get; set; } = "";
        public string Byte26 { get; set; } = "";
        public string Byte27 { get; set; } = "";
        public string Byte28 { get; set; } = "";
        public string Byte29 { get; set; } = "";
        public string Byte30 { get; set; } = "";
        public string Byte31 { get; set; } = "";
        public string Byte32 { get; set; } = "";
        public string Byte33 { get; set; } = "";
        public string Byte34 { get; set; } = "";
        public string Byte35 { get; set; } = "";
        public string Byte36 { get; set; } = "";
        public string Byte37 { get; set; } = "";
        public string Byte38 { get; set; } = "";
        public string Byte39 { get; set; } = "";
        public string Byte40 { get; set; } = "";
        public string Byte41 { get; set; } = "";
        public string Byte42 { get; set; } = "";
        public string Byte43 { get; set; } = "";
        public string Byte44 { get; set; } = "";
        public string Byte45 { get; set; } = "";
        public string Byte46 { get; set; } = "";
        public string Byte47 { get; set; } = "";
        public string Byte48 { get; set; } = "";
        public string Byte49 { get; set; } = "";
        public string Byte50 { get; set; } = "";
        public string Byte51 { get; set; } = "";
        public string Byte52 { get; set; } = "";
        public string Byte53 { get; set; } = "";
        public string Byte54 { get; set; } = "";
        public string Byte55 { get; set; } = "";
        public string Byte56 { get; set; } = "";
        public string Byte57 { get; set; } = "";
        public string Byte58 { get; set; } = "";
        public string Byte59 { get; set; } = "";
        public string Byte60 { get; set; } = "";
        public string Byte61 { get; set; } = "";
        public string Byte62 { get; set; } = "";
        public string Byte63 { get; set; } = "";
        public string Byte64 { get; set; } = "";

        public string Node { get; set; } = "CAN1";
        public string Count { get; set; } = "1";
        public string Notes { get; set; } = "";
        public string Color { get; set; } = "";

        public UInt32 ScheduledTime { get; set; } // try something new

        public CanTxData()
        {

        }

        public CanTxData(CanTxData canTxData) // makes a copy
        {
            int dataLength = 0;
            int.TryParse(canTxData.DLC, out dataLength);

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
            if(dataLength >= 9) // 12 bytes
            {
                Byte9 = canTxData.Byte9;
                Byte10 = canTxData.Byte10;
                Byte11 = canTxData.Byte11;
                Byte12 = canTxData.Byte12;
            }
            if(dataLength >= 10) // 16 bytes
            {
                Byte13 = canTxData.Byte13;
                Byte14 = canTxData.Byte14;
                Byte15 = canTxData.Byte15;
                Byte16 = canTxData.Byte16;
            }
            if(dataLength >= 11) // 20 bytes
            {
                Byte17 = canTxData.Byte17;
                Byte18 = canTxData.Byte18;
                Byte19 = canTxData.Byte19;
                Byte20 = canTxData.Byte20;
            }
            if(dataLength >= 12) // 24 bytes
            {
                Byte21 = canTxData.Byte21;
                Byte22 = canTxData.Byte22;
                Byte23 = canTxData.Byte23;
                Byte24 = canTxData.Byte24;
            }
            if(dataLength >= 13) // 32 bytes
            {
                Byte25 = canTxData.Byte25;
                Byte26 = canTxData.Byte26;
                Byte27 = canTxData.Byte27;
                Byte28 = canTxData.Byte28;
                Byte29 = canTxData.Byte29;
                Byte30 = canTxData.Byte30;
                Byte31 = canTxData.Byte31;
                Byte32 = canTxData.Byte32;
            }
            if(dataLength >= 14) // 48 bytes
            {
                Byte33 = canTxData.Byte33;
                Byte34 = canTxData.Byte34;
                Byte35 = canTxData.Byte35;
                Byte36 = canTxData.Byte36;
                Byte37 = canTxData.Byte37;
                Byte38 = canTxData.Byte38;
                Byte39 = canTxData.Byte39;
                Byte40 = canTxData.Byte40;
                Byte41 = canTxData.Byte41;
                Byte42 = canTxData.Byte42;
                Byte43 = canTxData.Byte43;
                Byte44 = canTxData.Byte44;
                Byte45 = canTxData.Byte45;
                Byte46 = canTxData.Byte46;
                Byte47 = canTxData.Byte47;
                Byte48 = canTxData.Byte48;
            }
            if (dataLength == 15) // 64 bytes
            {
                Byte49 = canTxData.Byte49;
                Byte50 = canTxData.Byte50;
                Byte51 = canTxData.Byte51;
                Byte52 = canTxData.Byte52;
                Byte53 = canTxData.Byte53;
                Byte54 = canTxData.Byte54;
                Byte55 = canTxData.Byte55;
                Byte56 = canTxData.Byte56;
                Byte57 = canTxData.Byte57;
                Byte58 = canTxData.Byte58;
                Byte59 = canTxData.Byte59;
                Byte60 = canTxData.Byte60;
                Byte61 = canTxData.Byte61;
                Byte62 = canTxData.Byte62;
                Byte63 = canTxData.Byte63;
                Byte64 = canTxData.Byte64;
            } 

            Node = canTxData.Node;
        }

        public CanTxData(CanRxData canRxData)
        {
            int dataLength = 0;
            int.TryParse(canRxData.DLC, out dataLength);

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
            if(dataLength >= 9) // 12 bytes
            {
                Byte9 = canRxData.Byte9;
                Byte10 = canRxData.Byte10;
                Byte11 = canRxData.Byte11;
                Byte12 = canRxData.Byte12;
            }
            if(dataLength >= 10) // 16 bytes
            {
                Byte13 = canRxData.Byte13;
                Byte14 = canRxData.Byte14;
                Byte15 = canRxData.Byte15;
                Byte16 = canRxData.Byte16;
            }
            if(dataLength >= 11)
            {                 
                Byte17 = canRxData.Byte17;
                Byte18 = canRxData.Byte18;
                Byte19 = canRxData.Byte19;
                Byte20 = canRxData.Byte20;
            }
            if(dataLength >= 12)
            {
                Byte21 = canRxData.Byte21;
                Byte22 = canRxData.Byte22;
                Byte23 = canRxData.Byte23;
                Byte24 = canRxData.Byte24;
            }
            if(dataLength >= 13)
            {
                Byte25 = canRxData.Byte25;
                Byte26 = canRxData.Byte26;
                Byte27 = canRxData.Byte27;
                Byte28 = canRxData.Byte28;
                Byte29 = canRxData.Byte29;
                Byte30 = canRxData.Byte30;
                Byte31 = canRxData.Byte31;
                Byte32 = canRxData.Byte32;
            }
            if(dataLength >= 14)
            {
                Byte33 = canRxData.Byte33;
                Byte34 = canRxData.Byte34;
                Byte35 = canRxData.Byte35;
                Byte36 = canRxData.Byte36;
                Byte37 = canRxData.Byte37;
                Byte38 = canRxData.Byte38;
                Byte39 = canRxData.Byte39;
                Byte40 = canRxData.Byte40;
                Byte41 = canRxData.Byte41;
                Byte42 = canRxData.Byte42;
                Byte43 = canRxData.Byte43;
                Byte44 = canRxData.Byte44;
                Byte45 = canRxData.Byte45;
                Byte46 = canRxData.Byte46;
                Byte47 = canRxData.Byte47;
                Byte48 = canRxData.Byte48;
            }
            if (dataLength == 15)
            {
                Byte49 = canRxData.Byte49;
                Byte50 = canRxData.Byte50;
                Byte51 = canRxData.Byte51;
                Byte52 = canRxData.Byte52;
                Byte53 = canRxData.Byte53;
                Byte54 = canRxData.Byte54;
                Byte55 = canRxData.Byte55;
                Byte56 = canRxData.Byte56;
                Byte57 = canRxData.Byte57;
                Byte58 = canRxData.Byte58;
                Byte59 = canRxData.Byte59;
                Byte60 = canRxData.Byte60;
                Byte61 = canRxData.Byte61;
                Byte62 = canRxData.Byte62;
                Byte63 = canRxData.Byte63;
                Byte64 = canRxData.Byte64;
            }    

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

        // CAN FD
        public string Byte9 { get; set; } = "";
        public string Byte10 { get; set; } = "";
        public string Byte11 { get; set; } = "";
        public string Byte12 { get; set; } = "";
        public string Byte13 { get; set; } = "";
        public string Byte14 { get; set; } = "";
        public string Byte15 { get; set; } = "";
        public string Byte16 { get; set; } = "";
        public string Byte17 { get; set; } = "";
        public string Byte18 { get; set; } = "";
        public string Byte19 { get; set; } = "";
        public string Byte20 { get; set; } = "";
        public string Byte21 { get; set; } = "";
        public string Byte22 { get; set; } = "";
        public string Byte23 { get; set; } = "";
        public string Byte24 { get; set; } = "";
        public string Byte25 { get; set; } = "";
        public string Byte26 { get; set; } = "";
        public string Byte27 { get; set; } = "";
        public string Byte28 { get; set; } = "";
        public string Byte29 { get; set; } = "";
        public string Byte30 { get; set; } = "";
        public string Byte31 { get; set; } = "";
        public string Byte32 { get; set; } = "";
        public string Byte33 { get; set; } = "";
        public string Byte34 { get; set; } = "";
        public string Byte35 { get; set; } = "";
        public string Byte36 { get; set; } = "";
        public string Byte37 { get; set; } = "";
        public string Byte38 { get; set; } = "";
        public string Byte39 { get; set; } = "";
        public string Byte40 { get; set; } = "";
        public string Byte41 { get; set; } = "";
        public string Byte42 { get; set; } = "";
        public string Byte43 { get; set; } = "";
        public string Byte44 { get; set; } = "";
        public string Byte45 { get; set; } = "";
        public string Byte46 { get; set; } = "";
        public string Byte47 { get; set; } = "";
        public string Byte48 { get; set; } = "";
        public string Byte49 { get; set; } = "";
        public string Byte50 { get; set; } = "";
        public string Byte51 { get; set; } = "";
        public string Byte52 { get; set; } = "";
        public string Byte53 { get; set; } = "";
        public string Byte54 { get; set; } = "";
        public string Byte55 { get; set; } = "";
        public string Byte56 { get; set; } = "";
        public string Byte57 { get; set; } = "";
        public string Byte58 { get; set; } = "";
        public string Byte59 { get; set; } = "";
        public string Byte60 { get; set; } = "";
        public string Byte61 { get; set; } = "";
        public string Byte62 { get; set; } = "";
        public string Byte63 { get; set; } = "";
        public string Byte64 { get; set; } = "";

        public string Node { get; set; } = "CAN1";
        public string ASCII { get; set; } = "";

        private string _RxCount = "0";
        public string RxCount
        {
            get
            {
                return _RxCount;
            }
            set
            {
                if (_RxCount == value) return;
                _RxCount = value;
                OnPropertyChanged(new PropertyChangedEventArgs("RxCount"));
            }
        }
        public string RxCountSaved { get; set; } = String.Empty;

        private string _TxCount = "0";
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
        public string TxCountSaved { get; set; } = String.Empty;
        public string Notes { get; set; } = "";
        public string Color { get; set; } = "";
        public string HexDump { get; set; } = "";

        public CanRxData()
        {

        }

        // The structure of the data from device
        public CanRxData(byte[] data)
        {
            int dataLength = 0;

            #region CanRxData(byte[] data)
            if (data[0] == 0)
            {
                IDE = "S";
            }
            else
            {
                IDE = "X";
            }
            
            // RTR
            
            RTR = (data[1] & 0x01) == 1 ? true: false; // bit0
            
            // Node
            int nodeNumber = data[2] & 0x0F;
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

            // index 3 is reserved

            UInt32 id = (UInt32)(data[4] | data[5] << 8 | data[6] << 16 | data[7] << 24);
            ArbID = Convert.ToString(id, 16).ToUpper();

            DLC = data[8].ToString();
            dataLength = data[8];

            if (dataLength >= 1)
            {
                Byte1 = data[9].ToString("X2");
            }
            if (dataLength >= 2)
            {
                Byte2 = data[10].ToString("X2");
            }
            if (dataLength >= 3)
            {
                Byte3 = data[11].ToString("X2");
            }
            if (dataLength >= 4)
            {
                Byte4 = data[12].ToString("X2");
            }
            if (dataLength >= 5)
            {
                Byte5 = data[13].ToString("X2");
            }
            if (dataLength >= 6)
            {
                Byte6 = data[14].ToString("X2");
            }
            if (dataLength >= 7)
            {
                Byte7 = data[15].ToString("X2");
            }
            if (dataLength >= 8)
            {
                Byte8 = data[16].ToString("X2");
            }
            if (dataLength >= 9) // 12 bytes
            {
                Byte9 = data[17].ToString("X2");
                Byte10 = data[18].ToString("X2");
                Byte11 = data[19].ToString("X2");
                Byte12 = data[20].ToString("X2");
            }
            if (dataLength >= 10) // 16 bytes
            {
                Byte13 = data[21].ToString("X2");
                Byte14 = data[22].ToString("X2");
                Byte15 = data[23].ToString("X2");
                Byte16 = data[24].ToString("X2");
            }
            if (dataLength >= 11) // 20 bytes
            {
                Byte17 = data[25].ToString("X2");
                Byte18 = data[26].ToString("X2");
                Byte19 = data[27].ToString("X2");
                Byte20 = data[28].ToString("X2");
            }
            if (dataLength >= 12) // 24 bytes
            {
                Byte21 = data[29].ToString("X2");
                Byte22 = data[30].ToString("X2");
                Byte23 = data[31].ToString("X2");
                Byte24 = data[32].ToString("X2");
            }
            if (dataLength >= 13) // 32 bytes
            {
                Byte25 = data[33].ToString("X2");
                Byte26 = data[34].ToString("X2");
                Byte27 = data[35].ToString("X2");
                Byte28 = data[36].ToString("X2");
                Byte29 = data[37].ToString("X2");
                Byte30 = data[38].ToString("X2");
                Byte31 = data[39].ToString("X2");
                Byte32 = data[40].ToString("X2");
            }
            if (dataLength >= 14) // 48 bytes
            {
                Byte33 = data[41].ToString("X2");
                Byte34 = data[42].ToString("X2");
                Byte35 = data[43].ToString("X2");
                Byte36 = data[44].ToString("X2");
                Byte37 = data[45].ToString("X2");
                Byte38 = data[46].ToString("X2");
                Byte39 = data[47].ToString("X2");
                Byte40 = data[48].ToString("X2");
                Byte41 = data[49].ToString("X2");
                Byte42 = data[50].ToString("X2");
                Byte43 = data[51].ToString("X2");
                Byte44 = data[52].ToString("X2");
                Byte45 = data[53].ToString("X2");
                Byte46 = data[54].ToString("X2");
                Byte47 = data[55].ToString("X2");
                Byte48 = data[56].ToString("X2");
            }
            if (dataLength == 15) // 64 bytes
            {
                Byte49 = data[57].ToString("X2");
                Byte50 = data[58].ToString("X2");
                Byte51 = data[59].ToString("X2");
                Byte52 = data[60].ToString("X2");
                Byte53 = data[61].ToString("X2");
                Byte54 = data[62].ToString("X2");
                Byte55 = data[63].ToString("X2");
                Byte56 = data[64].ToString("X2");
                Byte57 = data[65].ToString("X2");
                Byte58 = data[66].ToString("X2");
                Byte59 = data[67].ToString("X2");
                Byte60 = data[68].ToString("X2");
                Byte61 = data[69].ToString("X2");
                Byte62 = data[70].ToString("X2");
                Byte63 = data[71].ToString("X2");
                Byte64 = data[72].ToString("X2");
            }

            #endregion
        }

        public CanRxData(CanTxData canTxData)
        {
            int dataLength = 0;
            int.TryParse(canTxData.DLC, out dataLength);

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
            if(dataLength >= 9) // 12 bytes
            {
                Byte9 = canTxData.Byte9;
                Byte10 = canTxData.Byte10;
                Byte11 = canTxData.Byte11;
                Byte12 = canTxData.Byte12;
            }
            if(dataLength >= 10) 
            {
                Byte13 = canTxData.Byte13;
                Byte14 = canTxData.Byte14;
                Byte15 = canTxData.Byte15;
                Byte16 = canTxData.Byte16;
            }
            if(dataLength >= 11)
            {
                Byte17 = canTxData.Byte17;
                Byte18 = canTxData.Byte18;
                Byte19 = canTxData.Byte19;
                Byte20 = canTxData.Byte20;
            }
            if (dataLength >= 12)
            {
                Byte21 = canTxData.Byte21;
                Byte22 = canTxData.Byte22;
                Byte23 = canTxData.Byte23;
                Byte24 = canTxData.Byte24;
            }
            if(dataLength >= 13)
            {
                Byte25 = canTxData.Byte25;
                Byte26 = canTxData.Byte26;
                Byte27 = canTxData.Byte27;
                Byte28 = canTxData.Byte28;
                Byte29 = canTxData.Byte29;
                Byte30 = canTxData.Byte30;
                Byte31 = canTxData.Byte31;
                Byte32 = canTxData.Byte32;
            }
            if(dataLength >= 14)
            {
                Byte33 = canTxData.Byte33;
                Byte34 = canTxData.Byte34;
                Byte35 = canTxData.Byte35;
                Byte36 = canTxData.Byte36;
                Byte37 = canTxData.Byte37;
                Byte38 = canTxData.Byte38;
                Byte39 = canTxData.Byte39;
                Byte40 = canTxData.Byte40;
                Byte41 = canTxData.Byte41;
                Byte42 = canTxData.Byte42;
                Byte43 = canTxData.Byte43;
                Byte44 = canTxData.Byte44;
                Byte45 = canTxData.Byte45;
                Byte46 = canTxData.Byte46;
                Byte47 = canTxData.Byte47;
                Byte48 = canTxData.Byte48;
            }
            if (dataLength == 15)
            {
                Byte49 = canTxData.Byte49;
                Byte50 = canTxData.Byte50;
                Byte51 = canTxData.Byte51;
                Byte52 = canTxData.Byte52;
                Byte53 = canTxData.Byte53;
                Byte54 = canTxData.Byte54;
                Byte55 = canTxData.Byte55;
                Byte56 = canTxData.Byte56;
                Byte57 = canTxData.Byte57;
                Byte58 = canTxData.Byte58;
                Byte59 = canTxData.Byte59;
                Byte60 = canTxData.Byte60;
                Byte61 = canTxData.Byte61;
                Byte62 = canTxData.Byte62;
                Byte63 = canTxData.Byte63;
                Byte64 = canTxData.Byte64;
            } 

            Node = canTxData.Node;
        }

        public CanRxData(CanRxData canRxData)
        {
            int dataLength = 0;
            int.TryParse(canRxData.DLC, out dataLength);

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
            
            if (dataLength >= 9) // 12 bytes
            {
                Byte9 = canRxData.Byte9;
                Byte10 = canRxData.Byte10;
                Byte11 = canRxData.Byte11;
                Byte12 = canRxData.Byte12;
            }
            if(dataLength >= 10) // 16 bytes
            {
                Byte13 = canRxData.Byte13;
                Byte14 = canRxData.Byte14;
                Byte15 = canRxData.Byte15;
                Byte16 = canRxData.Byte16;
            }
            if(dataLength >= 11) // 20 bytes
            {
                Byte17 = canRxData.Byte17;
                Byte18 = canRxData.Byte18;
                Byte19 = canRxData.Byte19;
                Byte20 = canRxData.Byte20;
            }
            if(dataLength >= 12) // 24 bytes
            {
                Byte21 = canRxData.Byte21;
                Byte22 = canRxData.Byte22;
                Byte23 = canRxData.Byte23;
                Byte24 = canRxData.Byte24;
            }
            if(dataLength >= 13) // 32 bytes
            {
                Byte25 = canRxData.Byte25;
                Byte26 = canRxData.Byte26;
                Byte27 = canRxData.Byte27;
                Byte28 = canRxData.Byte28;
                Byte29 = canRxData.Byte29;
                Byte30 = canRxData.Byte30;
                Byte31 = canRxData.Byte31;
                Byte32 = canRxData.Byte32;
            }
            if(dataLength >= 14) // 48 bytes
            {
                Byte33 = canRxData.Byte33;
                Byte34 = canRxData.Byte34;
                Byte35 = canRxData.Byte35;
                Byte36 = canRxData.Byte36;
                Byte37 = canRxData.Byte37;
                Byte38 = canRxData.Byte38;
                Byte39 = canRxData.Byte39;
                Byte40 = canRxData.Byte40;
                Byte41 = canRxData.Byte41;
                Byte42 = canRxData.Byte42;
                Byte43 = canRxData.Byte43;
                Byte44 = canRxData.Byte44;
                Byte45 = canRxData.Byte45;
                Byte46 = canRxData.Byte46;
                Byte47 = canRxData.Byte47;
                Byte48 = canRxData.Byte48;
            }
            if(dataLength == 15) // 64 bytes
            {
                Byte49 = canRxData.Byte49;
                Byte50 = canRxData.Byte50;
                Byte51 = canRxData.Byte51;
                Byte52 = canRxData.Byte52;
                Byte53 = canRxData.Byte53;
                Byte54 = canRxData.Byte54;
                Byte55 = canRxData.Byte55;
                Byte56 = canRxData.Byte56;
                Byte57 = canRxData.Byte57;
                Byte58 = canRxData.Byte58;
                Byte59 = canRxData.Byte59;
                Byte60 = canRxData.Byte60;
                Byte61 = canRxData.Byte61;
                Byte62 = canRxData.Byte62;
                Byte63 = canRxData.Byte63;
                Byte64 = canRxData.Byte64;
            }

            Node = canRxData.Node;
            RxCount = canRxData.RxCount;
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