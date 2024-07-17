using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using ArduinoUploader;
using ArduinoUploader.Hardware;

namespace SharpManager
{
    public class ArduinoHardware
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The arduino model
        /// </summary>
        private readonly ArduinoModel arduinoModel;

        /// <summary>
        /// The firmware resource filename
        /// </summary>
        private readonly string firmware;

        /// <summary>The arduino hardware list</summary>
        private static readonly List<ArduinoHardware> arduinoHardwareList = new();

        /// <summary>Gets the supported hardware models.</summary>
        public static IEnumerable<ArduinoHardware> Models => arduinoHardwareList;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArduinoHardware"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arduinoModel">The arduino model.</param>
        /// <param name="firmware">The firmware.</param>
        private ArduinoHardware(string name, ArduinoModel arduinoModel, string firmware)
        {
            Name = name;
            this.arduinoModel = arduinoModel;
            this.firmware = firmware;
        }

        /// <summary>
        /// Uploads the firmware for this hardware model
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="debugTarget">The debug target.</param>
        /// <param name="progress">The progress.</param>
        public async Task UploadFirmware(string port, IDebugTarget debugTarget, IProgress<double> progress)
        {
            // Create the uploader
            var uploader = new ArduinoUploader.ArduinoSketchUploader(new ArduinoSketchUploaderOptions
            {
                PortName = port,
                ArduinoModel = arduinoModel
            }, new DebugLogTranslator(debugTarget), progress);

            await Task.Run(() => uploader.UploadSketch(ReadHexFirmware(firmware)));
        }

        /// <summary>
        /// Initializes the <see cref="ArduinoHardware"/> class.
        /// </summary>
        static ArduinoHardware()
        {
            arduinoHardwareList.Add(new ArduinoHardware("Nano R3", ArduinoModel.NanoR3, "NanoR3"));
            arduinoHardwareList.Add(new ArduinoHardware("Nano R3 (Old Bootloader)", ArduinoModel.NanoR3Old, "NanoR3"));
            arduinoHardwareList.Add(new ArduinoHardware("Nano R2", ArduinoModel.NanoR2, "NanoR2"));
            arduinoHardwareList.Add(new ArduinoHardware("Mega 2560", ArduinoModel.Mega2560, "Mega2560"));
            arduinoHardwareList.Add(new ArduinoHardware("Mega 1284", ArduinoModel.Mega1284, "Mega1284"));
            arduinoHardwareList.Add(new ArduinoHardware("Leonardo", ArduinoModel.Leonardo, "Leonardo"));
            arduinoHardwareList.Add(new ArduinoHardware("Micro", ArduinoModel.Micro, "Micro"));
        }

        /// <summary>
        /// Reads the hexadecimal firmware.
        /// </summary>
        /// <param name="firmwareName">Name of the firmware.</param>
        /// <returns></returns>
        private static IEnumerable<string> ReadHexFirmware(string firmwareName)
        {
            // Get the resource file name
            var assembly = Assembly.GetExecutingAssembly();
            string namespaceName = typeof(Arduino).Namespace!;
            var resourceName = namespaceName + ".Firmware." + firmwareName + ".hex";

            // Open the file and read all the lines
            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Arduino firmware {firmwareName} not found");
            using StreamReader reader = new StreamReader(stream);
            string? line;
            while ((line = reader.ReadLine()) != null) yield return line;
        }

        /// <summary>
        /// Translate IArduinoUploaderLogger calls to IDebugTarget calls
        /// </summary>
        /// <seealso cref="ArduinoUploader.IArduinoUploaderLogger" />
        private class DebugLogTranslator : IArduinoUploaderLogger
        {
            /// <summary>The debug target</summary>
            private readonly IDebugTarget debugTarget;

            /// <summary>
            /// Initializes a new instance of the <see cref="DebugLogTranslator"/> class.
            /// </summary>
            /// <param name="debugTarget">The debug target.</param>
            public DebugLogTranslator(IDebugTarget debugTarget)
            {
                this.debugTarget = debugTarget;
            }

            /// <summary>
            /// Debugs the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            void IArduinoUploaderLogger.Debug(string message)
            {
                debugTarget.DebugWriteLine(message);
            }

            /// <summary>
            /// Errors the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            void IArduinoUploaderLogger.Error(string message, Exception exception)
            {
                throw exception;
            }

            /// <summary>
            /// Informations the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            void IArduinoUploaderLogger.Info(string message)
            {
                debugTarget.WriteLine(message);
            }

            /// <summary>
            /// Traces the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            void IArduinoUploaderLogger.Trace(string message)
            {
                debugTarget.DebugWriteLine(message);
            }

            /// <summary>
            /// Warns the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            void IArduinoUploaderLogger.Warn(string message)
            {
                debugTarget.WriteLine(message);
            }
        }
    }
}
