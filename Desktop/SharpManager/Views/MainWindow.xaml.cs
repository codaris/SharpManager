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
using Microsoft.WindowsAPICodePack.Dialogs;

using SharpManager.ViewModels;

using static System.Net.Mime.MediaTypeNames;

namespace SharpManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMessageTarget
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

            viewModel = new MainViewModel(this);
            DataContext = viewModel;

            viewModel.Arduino.DiskDrive.DiskDirectory = "C:\\Projects\\Handhelds\\Sharp Pocket\\SharpManager\\Disk";

            // Apppend newline after version text
            Log.AppendText("\r\n");
            Log.ScrollToEnd();
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
            openFileDialog.Filter = "Tape files (*.tap)|*.tap|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Log.AppendText($"Sending file: {openFileDialog.FileName}\r\n");
                using var fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                await viewModel.Arduino.SendTapeFile(fileStream);
            }
        }

        /// <summary>
        /// Handles the Click event of the OpenFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ReceiveFile_Click(object sender, RoutedEventArgs e)
        {
            // The file data
            byte[] data;
            
            try
            {
                // Read the tape data
                data = await viewModel.Arduino.ReadTapeFile();
            }
            catch (System.OperationCanceledException)
            {
                Log.AppendText("Cancelled.");
                return;
            }

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Tape files (*.tap)|*.tap|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                Log.AppendText($"Saving file: {saveFileDialog.FileName}\r\n");
                using var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write);
                fileStream.Write(data, 0, data.Length);
            }
        }

        private void SelectDiskFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = viewModel.Arduino.DiskDrive.DiskDirectory;
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                viewModel.Arduino.DiskDrive.DiskDirectory = dialog.FileName;
            }
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

        /// <summary>
        /// Handles the Click event of the Connect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.Connect();
        }

        /// <summary>
        /// Handles the Click event of the Disconnect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Disconnect();
        }

        private async void Ping_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.Arduino.Ping();
        }

        /// <summary>
        /// Handles the Click event of the Cancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Cancel();
        }

        /// <summary>
        /// Handles the PreviewKeyDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && viewModel.CanCancel)
            {
                viewModel.Cancel();
                e.Handled = true;   
            }
        }

        /// <summary>
        /// Write the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void IMessageTarget.Write(string message)
        {
            Dispatcher.InvokeAsync(() => {
                Log.AppendText(message);
                Log.ScrollToEnd();
            });
        }

    }
}
 