using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace SharpManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Runs the action on UI thread (if not on the UI thread)
        /// </summary>
        /// <param name="action">The action.</param>
        public static async Task RunOnUIThread(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (Current?.Dispatcher?.CheckAccess() ?? true) action();
            else await Current.Dispatcher.InvokeAsync(action);
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version!;

        /// <summary>
        /// Handles the Exit event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExitEventArgs"/> instance containing the event data.</param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Save the settings on exit
            SharpManager.Properties.Settings.Default.Save();
        }
    }
}
