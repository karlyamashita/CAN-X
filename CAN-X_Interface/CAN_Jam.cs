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

        public UInt32 CAN_Jam_Node { get; set; }

        public UInt32 Index { get; set; }
        public UInt32 ARB_ID { get; set; }

        public UInt32 ModType { get; set; }

        public byte ByteToModify { get; set; }

        private byte[] _byteValues { get; set; }
        
        public byte[] ByteValues
        {
            get
            {
                if (_byteValues == null)
                {
                    _byteValues = new byte[8];
                }
                return _byteValues;
            }
            set
            {
                if(value != null)
                {
                    _byteValues = (byte[])value.Clone();
                }
                else
                {
                    _byteValues = null;
                }
            }
        }
        /*
         * // Assign a new byte array
            byte[] newBytes = { 1, 2, 3 };
            CAN_Jam can_jam = new CAN_Jam(); // Create an instance of CAN_Jam
            can_jam.DataBytes = newBytes; // Clone the array
            can_jam.DataBytes = null; // Set to null
         */

        private byte[] _bitsToToggle { get; set; }

        public byte[] BitsToToggle
        {
            get
            {
                if (_bitsToToggle == null)
                {
                    _bitsToToggle = new byte[8];
                }
                return _bitsToToggle;
            }
            set
            {
                if (value != null)
                {
                    _bitsToToggle = (byte[])value.Clone();
                }
                else
                {
                    _bitsToToggle = null;
                }
            }
        }

        private byte[] _bitsToHigh { get; set; }

        public byte[] BitsToHigh
        {
            get
            {
                if (_bitsToHigh == null)
                {
                    _bitsToHigh = new byte[8];
                }
                return _bitsToHigh;
            }
            set
            {
                if (value != null)
                {
                    _bitsToHigh = (byte[])value.Clone();
                }
                else
                {
                    _bitsToHigh = null;
                }
            }
        }

        private byte[] _bitsToLow { get; set; }

        public byte[] BitsToLow
        {
            get
            {
                if (_bitsToLow == null)
                {
                    _bitsToLow = new byte[8];
                }
                return _bitsToLow;
            }
            set
            {
                if (value != null)
                {
                    _bitsToLow = (byte[])value.Clone();
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
            bytes.Add((byte)(CAN_Jam_Node & 0xFF));
            bytes.Add((byte)(Index & 0xFF));
            bytes.Add((byte)((CAN_Jam_Node >> 8) & 0xFF));
            bytes.Add((byte)((CAN_Jam_Node >> 16) & 0xFF));
            bytes.Add((byte)((CAN_Jam_Node >> 24) & 0xFF));
            bytes.Add((byte)(ARB_ID & 0xFF));
            bytes.Add((byte)((ARB_ID >> 8) & 0xFF));
            bytes.Add((byte)((ARB_ID >> 16) & 0xFF));
            bytes.Add((byte)((ARB_ID >> 24) & 0xFF));
            bytes.Add((byte)(ModType & 0xFF));
            bytes.Add((byte)((ModType >> 8) & 0xFF));
            bytes.Add((byte)((ModType >> 16) & 0xFF));
            bytes.Add((byte)((ModType >> 24) & 0xFF));
            bytes.Add(ByteToModify);
            if (ByteValues != null && ByteValues.Length == 8)
            {
                bytes.AddRange(ByteValues);
            }
            else
            {
                bytes.AddRange(new byte[8]);
            }
            if (BitsToToggle != null && BitsToToggle.Length == 8)
            {
                bytes.AddRange(BitsToToggle);
            }
            else
            {
                bytes.AddRange(new byte[8]);
            }
            if (BitsToHigh != null && BitsToHigh.Length == 8)
            {
                bytes.AddRange(BitsToHigh);
            }
            else
            {
                bytes.AddRange(new byte[8]);
            }
            if (BitsToLow != null && BitsToLow.Length == 8)
            {
                bytes.AddRange(BitsToLow);
            }
            else
            {
                bytes.AddRange(new byte[8]);
            }
            return bytes.ToArray();
        }

    }
}
