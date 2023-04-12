using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // public ObservableCollection<SerialPortViewModel> SerialPorts { get; } = new();

        public ObservableCollection<string> SerialPorts { get; } = new();

        public string? SelectedSerialPort
        {
            get => GetProperty<string?>();
            set => SetProperty(value);
        }

        /*
        public SerialPortViewModel? SelectedSerialPort
        {
            get
            {
                foreach (var serialPort in SerialPorts)
                {
                    if (serialPort.IsSelected) return serialPort;
                }
                return null;
            }
        }
        */

        public MainViewModel()
        {
            SerialPortService.PortsChanged += SerialPortService_PortsChanged;
            UpdateSerialPorts(SerialPortService.GetAvailableSerialPorts());
        }

        private void SerialPortService_PortsChanged(object? sender, PortsChangedArgs e)
        {
            _ = App.RunOnUIThread(() => UpdateSerialPorts(e.SerialPorts));
        }

        private void UpdateSerialPorts(string[] ports)
        {
            foreach (var serialPort in SerialPorts.ToList())
            {
                if (ports.Contains(serialPort)) continue;
                SerialPorts.Remove(serialPort);
            }

            foreach (var name in ports)
            {
                if (!SerialPorts.Contains(name))
                {
                    var beforePort = SerialPorts.FirstOrDefault(item => string.Compare(item, name, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (beforePort != null) SerialPorts.Insert(SerialPorts.IndexOf(beforePort), name);
                    else SerialPorts.Add(name);
                }
            }

            if (SelectedSerialPort == null) SelectedSerialPort = SerialPorts.FirstOrDefault();

            /*
            foreach (var serialPort in SerialPorts.ToList()) 
            {
                if (ports.Contains(serialPort.Name)) continue;
                if (serialPort.IsConnected) serialPort.Disconnect();
                SerialPorts.Remove(serialPort);
            }
            foreach(var name in ports)
            {
                if (!SerialPorts.Any(p => p.Name == name))
                {
                    var beforePort = SerialPorts.FirstOrDefault(item => string.Compare(item.Name, name, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (beforePort != null) SerialPorts.Insert(SerialPorts.IndexOf(beforePort), new SerialPortViewModel(this, name));
                    else SerialPorts.Add(new SerialPortViewModel(this, name));
                }
            }
            // Ensure one serial port is selected
            if (SerialPorts.Count > 0 && !SerialPorts.Any(p => p.IsSelected))
            {
                SerialPorts.First().IsSelected = true;
            }
            */
        }
    }
}
