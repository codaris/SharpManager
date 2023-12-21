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
    public class MainViewModel : BaseViewModel, IDebugTarget
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
        /// Gets a value indicating whether the current operation can be cancelled
        /// </summary>
        public bool CanCancel => Arduino.CanCancel;

        /// <summary>
        /// Gets the status.
        /// </summary>
        public string Status => IsConnected ? "Connected" : "Disconnected";

        /// <summary>
        /// Gets or sets whether to show debug messages
        /// </summary>
        public bool ShowDebug
        {
            get => GetProperty(false);
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets the disk directory text.
        /// </summary>
        public string DiskDirectoryText
        {
            get
            {
                if (Arduino.DiskDrive.DiskDirectory == null) return "No Disk Folder Specified";
                return "Disk Folder: " + Arduino.DiskDrive.DiskDirectory;
            }
        }

        /// <summary>
        /// The message target
        /// </summary>
        private readonly IMessageTarget messageTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="messageTarget">The message log.</param>
        public MainViewModel(IMessageTarget messageTarget)
        {
            this.messageTarget = messageTarget;
            Arduino = new Arduino(this);
            this.PropagatePropertyChanged(Arduino, a => a.IsConnected, t => t.IsConnected);
            this.PropagatePropertyChanged(Arduino, a => a.IsConnected, t => t.IsDisconnected);
            this.PropagatePropertyChanged(Arduino, a => a.IsConnected, t => t.Status);
            this.PropagatePropertyChanged(Arduino, a => a.CanCancel, t => t.CanCancel);
            this.PropagatePropertyChanged(Arduino.DiskDrive, d => d.DiskDirectory!, t => t.DiskDirectoryText);

            SerialPortService.PortsChanged += SerialPortService_PortsChanged;
            UpdateSerialPorts(SerialPortService.GetAvailableSerialPorts());
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Connect()
        {
            if (SelectedSerialPort == null) return false;
            await Arduino.Connect(SelectedSerialPort);
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
        /// Cancels this instance.
        /// </summary>
        public void Cancel()
        {
            Arduino.Cancel();
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

        /// <summary>
        /// Write the specified debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        void IDebugTarget.DebugWrite(string message)
        {
            if (ShowDebug) messageTarget.Write(message);
        }

        /// <summary>
        /// Write the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void IMessageTarget.Write(string message)
        {
            messageTarget.Write(message);
        }
    }
}
