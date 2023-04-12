using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public static class SerialPortService
    {
        /// <summary>The serial ports</summary>
        private static string[] _serialPorts;

        /// <summary>The arrival watcher</summary>
        private static ManagementEventWatcher? arrival;

        /// <summary>The removal watcher</summary>
        private static ManagementEventWatcher? removal;

        /// <summary>
        /// Initializes the <see cref="SerialPortService"/> class.
        /// </summary>
        static SerialPortService()
        {
            _serialPorts = GetAvailableSerialPorts();
            MonitorDeviceChanges();
        }

        /// <summary>
        /// If this method isn't called, an InvalidComObjectException will be thrown (like below):
        /// System.Runtime.InteropServices.InvalidComObjectException was unhandled
        ///Message=COM object that has been separated from its underlying RCW cannot be used.
        ///Source=mscorlib
        ///StackTrace:
        ///     at System.StubHelpers.StubHelpers.StubRegisterRCW(Object pThis, IntPtr pThread)
        ///     at System.Management.IWbemServices.CancelAsyncCall_(IWbemObjectSink pSink)
        ///     at System.Management.SinkForEventQuery.Cancel()
        ///     at System.Management.ManagementEventWatcher.Stop()
        ///     at System.Management.ManagementEventWatcher.Finalize()
        ///InnerException: 
        /// </summary>
        public static void CleanUp()
        {
            arrival?.Stop();
            removal?.Stop();
        }

        /// <summary>
        /// Occurs when the ports changed.
        /// </summary>
        public static event EventHandler<PortsChangedArgs>? PortsChanged;

        /// <summary>
        /// Monitors the device changes.
        /// </summary>
        private static void MonitorDeviceChanges()
        {
            try
            {
                var deviceArrivalQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
                var deviceRemovalQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");

                arrival = new ManagementEventWatcher(deviceArrivalQuery);
                removal = new ManagementEventWatcher(deviceRemovalQuery);

                arrival.EventArrived += (o, args) => RaisePortsChangedIfNecessary(EventType.Insertion);
                removal.EventArrived += (sender, eventArgs) => RaisePortsChangedIfNecessary(EventType.Removal);

                // Start listening for events
                arrival.Start();
                removal.Start();
            }
            catch (ManagementException)
            {
            }
        }

        /// <summary>
        /// Raises the ports changed if necessary.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        private static void RaisePortsChangedIfNecessary(EventType eventType)
        {
            lock (_serialPorts)
            {
                var availableSerialPorts = GetAvailableSerialPorts();
                if (!_serialPorts.SequenceEqual(availableSerialPorts))
                {
                    _serialPorts = availableSerialPorts;
                    PortsChanged?.Raise(null, new PortsChangedArgs(eventType, _serialPorts));
                }
            }
        }

        /// <summary>
        /// Gets the available serial ports.
        /// </summary>
        public static string[] GetAvailableSerialPorts()
        {
            return SerialPort.GetPortNames();
        }
    }

    /// <summary>
    /// The event type
    /// </summary>
    public enum EventType
    {
        Insertion,
        Removal,
    }

    /// <summary>
    /// Ports changed args
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class PortsChangedArgs : EventArgs
    {
        /// <summary>The event type</summary>
        private readonly EventType _eventType;

        /// <summary>The serial ports</summary>
        private readonly string[] _serialPorts;

        /// <summary>Initializes a new instance of the <see cref="PortsChangedArgs" /> class.</summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="serialPorts">The serial ports.</param>
        public PortsChangedArgs(EventType eventType, string[] serialPorts)
        {
            _eventType = eventType;
            _serialPorts = serialPorts;
        }

        /// <summary>
        /// Gets the serial ports.
        /// </summary>
        public string[] SerialPorts => _serialPorts;

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public EventType EventType => _eventType;
    }
}
