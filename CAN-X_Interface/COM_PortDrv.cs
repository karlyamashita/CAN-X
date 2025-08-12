using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management; // Required for WMI

namespace USB_CAN_Interface
{
    public class COM_PortDrv
    {
        private SerialPort _serialPort;

        // Define a delegate for the DataReceived event
        public delegate void DataReceivedHandler(object sender, byte[] data);

        // Declare the event based on the delegate
        public event DataReceivedHandler DataReceived;

        public COM_PortDrv(string portName)
        {
            _serialPort = new SerialPort(portName, 115200);
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Open()
        {
            _serialPort.Open();
        }

        public void Close()
        {
            _serialPort.Close();
        }

        public bool IsOpen
        {
            get {  return _serialPort.IsOpen; }
        }

        public string portName
        {
            get { return (string)_serialPort.PortName; }
        }

        public void WriteBytes(byte[] data, int size)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Write(data, 0, size);
            }
        }

        // Event handler for the SerialPort's DataReceived event
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int numOfBytes = _serialPort.BytesToRead;
            byte[] receivedData= new byte[numOfBytes];
            _serialPort.Read(receivedData,0, numOfBytes);
            OnDataReceived(receivedData);
        }

        // Method to raise the DataReceived event
        protected virtual void OnDataReceived(byte[] data)
        {
            DataReceived?.Invoke(this, data);
        }
    }

    public class ComPortHelper
    {
        public static Dictionary<string, string> GetAvailableComPorts()
        {
            Dictionary<string, string> comPorts = new Dictionary<string, string>();
            string[] portNames = SerialPort.GetPortNames();

            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%)%'"))
            {
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string caption = queryObj["Caption"].ToString();
                    // Extract the COM port name (e.g., COM1, COM2) from the caption
                    int startIndex = caption.IndexOf("(COM") + 1;
                    int endIndex = caption.IndexOf(")", startIndex);
                    if (startIndex > 0 && endIndex > startIndex)
                    {
                        string comPortName = caption.Substring(startIndex, endIndex - startIndex);
                        if (portNames.Contains(comPortName)) // Ensure it's an actual available port
                        {
                            comPorts[comPortName] = caption; // Store "COMx - Friendly Name"
                        }
                    }
                }
            }
            return comPorts;
        }
    }
}
