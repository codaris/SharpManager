using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

using SharpManager.ViewModels;

namespace SharpManager.Views
{
    /// <summary>
    /// Interaction logic for UploadFirmware.xaml
    /// </summary>
    public partial class UploadFirmware : Window
    {
        /// <summary>
        /// The view model
        /// </summary>
        private readonly UploadFirmwareViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadFirmware"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="viewModel">The view model.</param>
        public UploadFirmware(Window owner, UploadFirmwareViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            this.viewModel = viewModel;
            Owner = owner;
        }

        /// <summary>
        /// Handles the Click event of the Upload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            if (await viewModel.Upload(this)) Close();
        }

        /// <summary>
        /// Handles the Click event of the Cancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //if (viewModel.IsEnabled) DialogResult = false;
            //else viewModel.Cancel();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="debugTarget">The debug target.</param>
        /// <returns></returns>
        public static bool ShowDialog(Window owner, IDebugTarget debugTarget)
        {
            var viewModel = new UploadFirmwareViewModel(debugTarget);
            var dialog = new UploadFirmware(owner, viewModel);
            return dialog.ShowDialog() ?? false;
        }

        /// <summary>
        /// Handles the Closing event of the Window control. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent closing the window if task is running
            e.Cancel = !viewModel.IsEnabled;
        }

        /// <summary>
        /// Handles the Closed event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            viewModel.Close();
        }

        /// <summary>
        /// Handles the RequestNavigate event of the Hyperlink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Navigation.RequestNavigateEventArgs"/> instance containing the event data.</param>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.ToString(),
                UseShellExecute = true
            });
        }
    }
}
