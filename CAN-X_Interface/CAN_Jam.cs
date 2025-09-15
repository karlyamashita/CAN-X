using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAN_X_CAN_Analyzer
{
    internal class CAN_Jam
    {
        public CAN_Jam() 
        {
        
        }

        public UInt32 Key { get; set; }

        public string Description { get; set; }

        private string _arbId { get; set; }
        public string ArbID 
        {
            get 
            {
                return _arbId;
            }
            set 
            {
                if(value != null)
                {
                    _arbId = value;
                }
                else
                {
                    _arbId = null;
                }
            }
        }

        public string Node { get; set; }

        public bool Jam { get; set; }

        public string RelayDisabled { get; set; }

        public string ByteToModify { get; set; }

        private string _byteValues { get; set; }
        
        public string ByteValues
        {
            get
            {
                return _byteValues;
            }
            set
            {
                if(value != null)
                {
                    _byteValues = value;
                }
                else
                {
                    _byteValues = null;
                }
            }
        }

        private string _bitsToToggle { get; set; }

        public string BitsToToggle
        {
            get
            {
                return _bitsToToggle;
            }
            set
            {
                if(value != null)
                {
                    _bitsToToggle = value;
                }
                else
                {
                    _bitsToToggle = null;
                }
            }
        }

        private string _bitsToHigh { get; set; }

        public string BitsToHigh
        {
            get
            {
                return _bitsToHigh;
            }
            set
            {
                if (value != null)
                {
                    _bitsToHigh = value;
                }
                else
                {
                    _bitsToHigh = null;
                }
            }
        }

        private string _bitsToLow { get; set; }

        public string BitsToLow
        {
            get
            {
                return _bitsToLow;
            }
            set
            {
                if (value != null)
                {
                    _bitsToLow = value;
                }
                else
                {
                    _bitsToLow = null;
                }
            }
        }

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();

            

            UInt32 hex = Convert.ToUInt32(ArbID, 16);

            bytes.Add((byte)hex);
            bytes.Add((byte)(hex >> 8));
            bytes.Add((byte)(hex >> 16));
            bytes.Add((byte)(hex >> 24));

            bytes.Add((byte)Key); // index 4

            byte val = 0;
            val = (byte)Convert.ToInt32(Node, 2);
            val |= (byte)(Convert.ToInt32(Jam) << 1);
            bytes.Add(val);
            bytes.Add((byte)Convert.ToInt32(RelayDisabled, 2));

            bytes.Add((byte)Convert.ToInt32(ByteToModify, 2));

            bytes.Add((byte)Convert.ToInt32(BitsToToggle?.Split(' ')[0] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToToggle?.Split(' ')[1] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToToggle?.Split(' ')[2] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToToggle?.Split(' ')[3] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToToggle?.Split(' ')[4] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToToggle?.Split(' ')[5] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToToggle?.Split(' ')[6] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToToggle?.Split(' ')[7] ?? "00000000", 2));

            bytes.Add((byte)Convert.ToInt32(BitsToHigh?.Split(' ')[0] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToHigh?.Split(' ')[1] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToHigh?.Split(' ')[2] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToHigh?.Split(' ')[3] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToHigh?.Split(' ')[4] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToHigh?.Split(' ')[5] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToHigh?.Split(' ')[6] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToHigh?.Split(' ')[7] ?? "00000000", 2));

            bytes.Add((byte)Convert.ToInt32(BitsToLow?.Split(' ')[0] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToLow?.Split(' ')[1] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToLow?.Split(' ')[2] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToLow?.Split(' ')[3] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToLow?.Split(' ')[4] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToLow?.Split(' ')[5] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToLow?.Split(' ')[6] ?? "00000000", 2));
            bytes.Add((byte)Convert.ToInt32(BitsToLow?.Split(' ')[7] ?? "00000000", 2));

            bytes.Add((byte)Convert.ToInt32(ByteValues?.Split(' ')[0] ?? "00", 16));
            bytes.Add((byte)Convert.ToInt32(ByteValues?.Split(' ')[1] ?? "00", 16));
            bytes.Add((byte)Convert.ToInt32(ByteValues?.Split(' ')[2] ?? "00", 16));
            bytes.Add((byte)Convert.ToInt32(ByteValues?.Split(' ')[3] ?? "00", 16));
            bytes.Add((byte)Convert.ToInt32(ByteValues?.Split(' ')[4] ?? "00", 16));
            bytes.Add((byte)Convert.ToInt32(ByteValues?.Split(' ')[5] ?? "00", 16));
            bytes.Add((byte)Convert.ToInt32(ByteValues?.Split(' ')[6] ?? "00", 16));
            bytes.Add((byte)Convert.ToInt32(ByteValues?.Split(' ')[7] ?? "00", 16));

            return bytes.ToArray();
        }

    }
}
