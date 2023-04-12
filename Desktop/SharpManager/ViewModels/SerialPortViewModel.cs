using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager.ViewModels
{
    public class SerialPortViewModel : BaseViewModel // , IDisposable
    {
        /*
        private MainViewModel mainViewModel;

        public string Name { get; }

        public bool IsSelected 
        { 
            get => _isSelected; 
            set
            {
                if (!SetProperty(ref _isSelected, value)) return;
                foreach (var serialPort in mainViewModel.SerialPorts)
                {
                    if (serialPort != this) serialPort.IsSelected = false;
                }
            } 
        }
        private bool _isSelected;
        private bool disposedValue;
          
        public bool IsConnected { get; private set; }

        public SerialPort? serialPort = null;

        public SerialPortViewModel(MainViewModel mainViewModel, string name)
        {
            this.mainViewModel = mainViewModel;
            Name = name;
            OnPropertyChanged(nameof(Name));
        }

        public void Connect()
        {
            serialPort = new SerialPort(Name, 115200);
            serialPort.DtrEnable = false;
            serialPort.Open();
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.ErrorReceived += SerialPort_ErrorReceived;
            IsConnected = true;
            OnPropertyChanged(nameof(IsConnected));
        }

        /// <summary>
        /// Handles the ErrorReceived event of the SerialPort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialErrorReceivedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="Exception">Serial port error: {e.EventType}</exception>
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            throw new Exception($"Serial port error: {e.EventType}");
        }

        /// <summary>
        /// Handles the DataReceived event of the SerialPort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialDataReceivedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
        }

        public void Disconnect()
        {
            serialPort?.Close();
            serialPort?.Dispose();
            serialPort = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    serialPort?.Dispose();
                    serialPort = null;
                }
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SerialPortViewModel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        */
    }
}
