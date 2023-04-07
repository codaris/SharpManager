using System;
using System.Collections.Generic;
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

namespace SharpManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Tape files (*.tap)|*.tap|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Log.AppendText($"File selected: {openFileDialog.FileName}\r\n");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SerialPortService.PortsChanged += SerialPortService_PortsChanged;
            foreach (var serialPort in SerialPortService.GetAvailableSerialPorts())
            {
                Log.AppendText(serialPort);
            }
        }

        private void SerialPortService_PortsChanged(object? sender, PortsChangedArgs e)
        {
            foreach (var serialPort in SerialPortService.GetAvailableSerialPorts())
            {
                Log.AppendText(serialPort);
            }
        }
    }
}
