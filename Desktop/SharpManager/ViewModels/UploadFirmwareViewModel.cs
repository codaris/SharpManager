using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SharpManager.ViewModels
{
    public class UploadFirmwareViewModel : BaseViewModel, IProgress<double>
    {
        /// <summary>The debug target</summary>
        private IDebugTarget debugTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadFirmwareViewModel"/> class.
        /// </summary>
        /// <param name="debugTarget">The debug target.</param>
        public UploadFirmwareViewModel(IDebugTarget debugTarget)
        {
            this.debugTarget = debugTarget;
            foreach (var model in ArduinoHardware.Models) ArduinoModels.Add(model); 
            SerialPortService.PortsChanged += SerialPortService_PortsChanged;
            UpdateSerialPorts(SerialPortService.GetAvailableSerialPorts());
        }

        /// <summary>
        /// Gets the available serial ports.
        /// </summary>
        public ObservableCollection<string> SerialPorts { get; } = new();

        /// <summary>
        /// Gets the arduino models.
        /// </summary>
        public ObservableCollection<ArduinoHardware> ArduinoModels { get; } = new();

        /// <summary>
        /// Gets or sets the selected serial port.
        /// </summary>
        public string? SelectedSerialPort
        {
            get => GetProperty<string?>(Properties.Settings.Default.SerialPort, nameof(SelectedSerialPort));
            set
            {
                SetProperty(value);
                Properties.Settings.Default.SerialPort = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected arduino model.
        /// </summary>
        public ArduinoHardware? SelectedArduinoModel
        {
            get => GetProperty<ArduinoHardware?>();
            set => SetProperty(value);
        }

        /// <summary>
        /// Gets the transfer percentage text.
        /// </summary>
        public string TransferPercentageText => TransferPercentage.HasValue ? TransferPercentage.Value.ToString("n0") + "%" : string.Empty;

        /// <summary>
        /// Gets the transfer percentage.
        /// </summary>
        public int? TransferPercentage { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the send is not running.
        /// </summary>
        public bool IsEnabled
        {
            get => GetProperty<bool>(true);
            private set => SetProperty(value);
        }

        /// <summary>
        /// Sends the file.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns></returns>
        public async Task<bool> Upload(Window owner)
        {
            if (string.IsNullOrEmpty(SelectedSerialPort))
            {
                MessageBox.Show(owner, "Select the serial port for the Arduino.", "Update Firmware", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (SelectedArduinoModel == null)
            {
                MessageBox.Show(owner, "Select the Arduino model.", "Update Firmware", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }


            try
            {
                IsEnabled = false;
                await SelectedArduinoModel.UploadFirmware(SelectedSerialPort, debugTarget, this);
                return true;
            }
            finally
            {
                IsEnabled = true;
            }
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
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            SerialPortService.PortsChanged -= SerialPortService_PortsChanged;
        }

        /// <summary>
        /// Reports a progress update.
        /// </summary>
        /// <param name="value">The value of the updated progress.</param>
        void IProgress<double>.Report(double value)
        {
            TransferPercentage = Convert.ToInt32(value * 100);
            OnPropertyChanged(nameof(TransferPercentage));
            OnPropertyChanged(nameof(TransferPercentageText));
        }
    }
}
