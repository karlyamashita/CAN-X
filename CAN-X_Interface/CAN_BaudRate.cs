using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAN_X_CAN_Analyzer
{
    class CAN_BaudRate
    {
        public BaudStructure baud_1000k;
        public BaudStructure baud_500k;
        public BaudStructure baud_250k;
        public BaudStructure baud_125k;
        public BaudStructure baud_100k;
        public BaudStructure baud_83_333k;
        public BaudStructure baud_50k;
        public BaudStructure baud_33_333k;

        public List<BaudStructure> baudList = new List<BaudStructure>();

        public CAN_BaudRate(string freq)
        {
            if(freq == "48mHz") {
                 baud_1000k = new BaudStructure("1000k", "0x001C0002");
                 baud_500k = new BaudStructure("500k", "0x001C0005");
                 baud_250k = new BaudStructure("250k", "0x001C000B");
                 baud_125k = new BaudStructure("125k", "0x001C0017");
                 baud_100k = new BaudStructure("100k", "0x001C001D");
                 baud_83_333k = new BaudStructure("83.333k", "0x001C0023");
                 baud_50k = new BaudStructure("50k", "0x001C003B");
                 baud_33_333k = new BaudStructure("33.333k", "0x001C0059");
            }
            else if(freq == "42mHz") 
            {
                 baud_1000k = new BaudStructure("1000k", "0x001a0002");
                 baud_500k = new BaudStructure("500k", "0x001a0005");
                 baud_250k = new BaudStructure("250k", "0x001a000b");
                 baud_125k = new BaudStructure("125k", "0x001a0017");
                 baud_100k = new BaudStructure("100k", "0x001a001d");
                 baud_83_333k = new BaudStructure("83.333k", "0x001a0023");
                 baud_50k = new BaudStructure("50k", "0x001a003b");
                 baud_33_333k = new BaudStructure("33.333k", "0x001a0059");
            }

            baudList.Add(baud_1000k);
            baudList.Add(baud_500k);
            baudList.Add(baud_250k);
            baudList.Add(baud_125k);
            baudList.Add(baud_100k);
            baudList.Add(baud_83_333k);
            baudList.Add(baud_50k);
            baudList.Add(baud_33_333k);
        }
    }

    class BaudStructure
    {
        public string baud;
        public string value;

        public BaudStructure(string baud, string value)
        {
            this.baud = baud;
            this.value = value;
        }
    }
}
