using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Management; // Required for WMI
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CAN_X_CAN_Analyzer
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

    public class ComPortInfo
    {
        public string PortName { get; set; }
        public string FullName { get; set; }
    }

    public class ComPortViewModel
    {
        private ManagementEventWatcher _watcher;
        public ObservableCollection<ComPortInfo> AvailablePorts { get; } = new ObservableCollection<ComPortInfo>();

        public ComPortViewModel()
        {
            // Initial population of ports
            RefreshPortNames();

            // Set up a WMI watcher to detect device changes (including COM port additions/removals)
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            _watcher = new ManagementEventWatcher(query);
            _watcher.EventArrived += (s, e) => RefreshPortNames();
            _watcher.Start();
        }

        private void RefreshPortNames()
        {
            // Ensure UI updates happen on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailablePorts.Clear();
                foreach (string portName in SerialPort.GetPortNames())
                {
                    // You can add logic here to get a more descriptive name if needed
                    // For simplicity, we'll use the port name as the full name for now.
                    AvailablePorts.Add(new ComPortInfo { PortName = portName, FullName = $"COM Port: {portName}" });
                }
            });
        }

        // Important: Implement IDisposable to properly stop the watcher when the ViewModel is no longer needed
        public void Dispose()
        {
            _watcher?.Stop();
            _watcher?.Dispose();
        }
    }
}
