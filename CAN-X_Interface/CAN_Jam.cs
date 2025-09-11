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

        private string _arb_id { get; set; }
        public string Arb_ID 
        {
            get 
            {
                return _arb_id;
            }
            set 
            {
                if(value != null)
                {
                    _arb_id = value;
                }
                else
                {
                    _arb_id = null;
                }
            }
        }

        public byte CAN_Jam_Node { get; set; }

        public byte RelayDisabled { get; set; }

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
        /*
         * // Assign a new byte array
            byte[] newBytes = { 1, 2, 3 };
            CAN_Jam can_jam = new CAN_Jam(); // Create an instance of CAN_Jam
            can_jam.DataBytes = newBytes; // Clone the array
            can_jam.DataBytes = null; // Set to null
         */

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

           // bytes.Add((byte)(Arb_ID & 0xFF));


            bytes.Add((byte)(Key & 0xFF));
            bytes.Add(CAN_Jam_Node);
            bytes.Add(RelayDisabled);

            bytes.Add((byte)Convert.ToInt32(ByteToModify,2));
            if (ByteValues != null && ByteValues.Length == 8)
            {
               // bytes.AddRange(ByteValues);
            }
            else
            {
                bytes.AddRange(new byte[8]);
            }
            if (BitsToToggle != null && BitsToToggle.Length == 8)
            {
               // bytes.AddRange(BitsToToggle);
            }
            else
            {
                bytes.AddRange(new byte[8]);
            }
            if (BitsToHigh != null && BitsToHigh.Length == 8)
            {
               // bytes.AddRange(BitsToHigh);
            }
            else
            {
                bytes.AddRange(new byte[8]);
            }
            if (BitsToLow != null && BitsToLow.Length == 8)
            {
               // bytes.AddRange(BitsToLow);
            }
            else
            {
                bytes.AddRange(new byte[8]);
            }
            return bytes.ToArray();
        }

    }
}
