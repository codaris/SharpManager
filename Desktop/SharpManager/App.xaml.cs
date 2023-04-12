using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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
    }
}
