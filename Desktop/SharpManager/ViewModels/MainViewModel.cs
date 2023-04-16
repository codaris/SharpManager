using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets the arduino interface instance.
        /// </summary>
        public Arduino Arduino { get; }

        /// <summary>
        /// Gets the available serial ports.
        /// </summary>
        public ObservableCollection<string> SerialPorts { get; } = new();

        /// <summary>
        /// Gets or sets the selected serial port.
        /// </summary>
        public string? SelectedSerialPort
        {
            get => GetProperty<string?>();
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets a value indicating whether the arduino is connected.
        /// </summary>
        public bool IsConnected => Arduino.IsConnected;

        /// <summary>
        /// Gets a value indicating whether the arduino is disconnected.
        /// </summary>
        public bool IsDisconnected => !Arduino.IsConnected;


        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="messageLog">The message log.</param>
        public MainViewModel(IMessageLog messageLog)
        {
            Arduino = new Arduino(messageLog);
            this.PropagatePropertyChanged(Arduino, a => a.IsConnected, t => t.IsConnected);
            this.PropagatePropertyChanged(Arduino, a => a.IsConnected, t => t.IsDisconnected);
            SerialPortService.PortsChanged += SerialPortService_PortsChanged;
            UpdateSerialPorts(SerialPortService.GetAvailableSerialPorts());
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            if (SelectedSerialPort == null) return false;
            Arduino.Connect(SelectedSerialPort);
            return true;
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            Arduino.Disconnect();
        }

        /// <summary>
        /// Serials the port service ports changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SerialPortService_PortsChanged(object? sender, PortsChangedArgs e)
        {
            _ = App.RunOnUIThread(() => UpdateSerialPorts(e.SerialPorts));
        }

        /// <summary>
        /// Updates the serial ports.
        /// </summary>
        /// <param name="ports">The ports.</param>
        private void UpdateSerialPorts(string[] ports)
        {
            foreach (var serialPort in SerialPorts.ToList())
            {
                if (ports.Contains(serialPort)) continue;
                Arduino.Disconnect();
                SerialPorts.Remove(serialPort);
            }

            foreach (var name in ports)
            {
                if (!SerialPorts.Contains(name))
                {
                    var beforePort = SerialPorts.FirstOrDefault(item => string.Compare(item, name, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (beforePort != null) SerialPorts.Insert(SerialPorts.IndexOf(beforePort), name);
                    else SerialPorts.Add(name);
                    SelectedSerialPort = name;
                }
            }

            SelectedSerialPort ??= SerialPorts.FirstOrDefault();
        }
    }
}
