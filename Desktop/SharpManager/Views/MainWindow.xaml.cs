using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using SharpManager.ViewModels;

using static System.Net.Mime.MediaTypeNames;

namespace SharpManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The view model
        /// </summary>
        private readonly MainViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            System.Windows.Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            viewModel = new MainViewModel();
            DataContext = viewModel;
        }

        /// <summary>
        /// Tasks the scheduler unobserved task exception.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnobservedTaskExceptionEventArgs"/> instance containing the event data.</param>
        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            if (e.Exception.Flatten().InnerExceptions.All(ex => ex is TaskCanceledException)) return;
            Dispatcher.InvokeAsync(() =>
            {
                foreach (var exception in e.Exception.Flatten().InnerExceptions)
                {
                    if (exception is TaskCanceledException) continue;
                    MessageBox.Show(this, e.Exception.Message, "An error occurred", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        /// <summary>
        /// Handles the DispatcherUnhandledException event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (e.Exception is TaskCanceledException) return;
            Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(this, e.Exception.Message, "An error occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        /// <summary>
        /// Handles the Click event of the OpenFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            // openFileDialog.Filter = "WAV files (*.wav)|*.wav|Tape files (*.tap)|*.tap|All files (*.*)|*.*";
            openFileDialog.Filter = "Tape files (*.tap)|*.tap|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Log.AppendText($"Sending file: {openFileDialog.FileName}\r\n");
                await viewModel.SendFile(openFileDialog.FileName, AppendMessage);
            }

            /*
            var data = ParseWAVFile(openFileDialog.FileName);
            for (int i = 0; i < data.Length; i++) 
            {
                Log.AppendText($"{data[i]}, ");
                if (i % 40 == 39) Log.AppendText("\r\n");
            }
            Log.AppendText("\r\n");
            */
        }

        /// <summary>
        /// Handles the Click event of the Exit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private const int SampleRate = 16000;
        private const int BitsPerSample = 8;
        private const int OneBitFrequency = 4000;
        private const int ZeroBitFrequency = 2000;
        private const int SamplesPerCycleOneBit = SampleRate / OneBitFrequency;
        private const int SamplesPerCycleZeroBit = SampleRate / ZeroBitFrequency;
        private const int CyclesPerOneBit = 4;
        private const int CyclesPerZeroBit = 2;

        public static byte[] ParseWAVFile(string filePath)
        {
            var bitList = new List<byte>();
            using (var fileStream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(fileStream))
            {
                // Check if file is little-endian or big-endian
                uint riff = reader.ReadUInt32();
                bool isLittleEndian = (riff == 0x46464952); // "RIFF"
                bool isBigEndian = (riff == 0x52494646); // "RIFX"

                if (!isLittleEndian && !isBigEndian)
                {
                    throw new ArgumentException("Invalid WAV file format.");
                }

                int ReadInt32()
                {
                    return isLittleEndian ? reader.ReadInt32() : BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
                }

                int ReadInt16()
                {
                    return isLittleEndian ? reader.ReadInt16() : BitConverter.ToInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
                }

                uint ReadUInt32()
                {
                    return isLittleEndian ? reader.ReadUInt32() : BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
                }

                // Verify WAV header
                if (ReadInt32() != reader.BaseStream.Length - 8 || ReadUInt32() != (isLittleEndian ? 0x45564157U : 0x57415645U))
                {
                    throw new ArgumentException("Invalid WAV file format.");
                }

                bool foundDataChunk = false;
                int dataSize = 0;
                byte[]? buffer = null;

                while (!foundDataChunk)
                {
                    uint chunkId = ReadUInt32();
                    int chunkSize = ReadInt32();

                    switch (chunkId)
                    {
                        case 0x20746d66: // "fmt "
                        case 0x666d7420: // " fmt"
                            int audioFormat = ReadInt16();
                            int numChannels = ReadInt16();
                            int sampleRate = ReadInt32();
                            ReadInt32(); // ByteRate
                            ReadInt16(); // BlockAlign
                            int bitsPerSample = ReadInt16();

                            // Format check
                            if (audioFormat != 1 || numChannels != 1 || sampleRate != SampleRate || bitsPerSample != BitsPerSample)
                            {
                                throw new ArgumentException("Unsupported WAV file format.");
                            }

                            reader.BaseStream.Seek(chunkSize - 16, SeekOrigin.Current);
                            break;

                        case 0x64617461: // "data"
                        case 0x61746164: // "ataD"
                            dataSize = chunkSize;
                            buffer = reader.ReadBytes(chunkSize);
                            foundDataChunk = true;
                            break;

                        default:
                            reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                            break;
                    }
                }

                int sampleCount = 0;
                bool currentState = false;
                bool previousState = false;

                int highCycleCount = 0;
                int lowCycleCount = 0;

                for (int i = 0; buffer != null && i < buffer.Length; i++)
                {
                    currentState = buffer[i] > 127;
                    sampleCount++;
                    if (currentState != previousState)
                    {
                        int count = sampleCount;
                        sampleCount = 0;
                        previousState = currentState;
                        if (count == 2) highCycleCount++;
                        else if (count == 4) lowCycleCount++;
                        else continue;

                        if (highCycleCount >= 16) bitList.Add(1);
                        else if (lowCycleCount >= 8) bitList.Add(0);
                        else continue;

                        highCycleCount = 0;
                        lowCycleCount = 0;
                    }
                }
            }

            return bitList.ToArray();

            /*
            // Convert the list of bits into a byte array
            int numBytes = (bitList.Count + 7) / 8;
            byte[] result = new byte[numBytes];

            for (int i = 0; i < bitList.Count; i++)
            {
                if (bitList[i] == 1)
                {
                    result[i / 8] |= (byte)(1 << (7 - (i % 8)));
                }
            }

            return result;
            */
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Connect();
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Disconnect();
        }

        private async void Test_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.Test(AppendMessage);
        }

        private void AppendMessage(string message)
        {
            Dispatcher.InvokeAsync(() => Log.AppendText(message));
        }
    }
}
 