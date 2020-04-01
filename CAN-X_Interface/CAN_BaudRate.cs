using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAN_X_CAN_Analyzer
{
    class CAN_BaudRate
    {
        public BaudStructure baud_1000;
        public BaudStructure baud_500;
        public BaudStructure baud_250;
        public BaudStructure baud_125;
        public BaudStructure baud_100;
        public BaudStructure baud_83_333;
        public BaudStructure baud_50;
        public BaudStructure baud_33_333;

        public List<BaudStructure> baudList = new List<BaudStructure>();

        public CAN_BaudRate(string freq)
        {
            if(freq == "APB1_48mHz") {
                 baud_1000 = new BaudStructure("1000", "0x001C0002");
                 baud_500 = new BaudStructure("500", "0x001C0005");
                 baud_250 = new BaudStructure("250", "0x001C000B");
                 baud_125 = new BaudStructure("125", "0x001C0017");
                 baud_100 = new BaudStructure("100", "0x001C001D");
                 baud_83_333 = new BaudStructure("83.333", "0x001C0023");
                 baud_50 = new BaudStructure("50", "0x001C003B");
                 baud_33_333 = new BaudStructure("33.333", "0x001C0059");
            }
            else if(freq == "APB1_42mHz") 
            {
                 baud_1000 = new BaudStructure("1000", "0x001A0002");
                 baud_500 = new BaudStructure("500", "0x001A0005");
                 baud_250 = new BaudStructure("250", "0x001A000B");
                 baud_125 = new BaudStructure("125", "0x001A0017");
                 baud_100 = new BaudStructure("100", "0x001A001D");
                 baud_83_333 = new BaudStructure("83.333", "0x001A0023");
                 baud_50 = new BaudStructure("50", "0x001A003B");
                 baud_33_333 = new BaudStructure("33.333", "0x001A0059");
            }
            else if(freq == "APB1_36mHz")
            {
                baud_1000 = new BaudStructure("1000", "0x001E0001");
                baud_500 = new BaudStructure("500", "0x001E0003");
                baud_250 = new BaudStructure("250", "0x001C0008");
                baud_125 = new BaudStructure("125", "0x001C0011");
                baud_100 = new BaudStructure("100", " 0x001B0017");
                baud_83_333 = new BaudStructure("83.333", "0x001C001A");
                baud_50 = new BaudStructure("50", "0x001C002C");
                baud_33_333 = new BaudStructure("33.333", "0x001B0047");
            }

            baudList.Add(baud_1000);
            baudList.Add(baud_500);
            baudList.Add(baud_250);
            baudList.Add(baud_125);
            baudList.Add(baud_100);
            baudList.Add(baud_83_333);
            baudList.Add(baud_50);
            baudList.Add(baud_33_333);
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
