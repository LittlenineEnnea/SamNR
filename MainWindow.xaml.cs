// MainWindow.xaml.cs
using System.IO.Ports;
using System.Windows;
using System.Windows.Documents;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Management;

namespace SamNR
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshComPorts();
        }

private void RefreshComPorts()
    {
        List<string> portList = new List<string>();

        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'"))
        {
            foreach (var obj in searcher.Get())
            {
                string name = obj["Name"]?.ToString();
                if (!string.IsNullOrEmpty(name) && name.Contains("(COM"))
                {
                    portList.Add(name);
                }
            }
        }

        Dispatcher.Invoke(() =>
        {
            cmbComPorts.ItemsSource = null; 
            cmbComPorts.ItemsSource = portList;
            if (portList.Count > 0)
            {
                cmbComPorts.SelectedIndex = 0; 
            }
            else
            {
                AddLog("No COM ports detected.");
            }
        });
    }
        private async void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            if (cmbComPorts.SelectedItem == null || string.IsNullOrWhiteSpace(cmbComPorts.SelectedItem.ToString()))
            {
                AddLog("No Devices");
                return;
            }

            string fullPortName = cmbComPorts.SelectedItem.ToString();
            string portName = System.Text.RegularExpressions.Regex.Match(fullPortName, @"\(COM\d+\)").Value.Trim('(', ')');

            if (string.IsNullOrEmpty(portName) || !SerialPort.GetPortNames().Contains(portName))
            {
                AddLog("Device disconnected");
                return;
            }

            string[] commands = {
        "AT+SWATD=0",
        "AT+ACTIVATE=0,0,0",
        "AT+SWATD=1",
        "AT+PRECONFG=2,XAA",
        "AT+KSTRINGB=0,3",
        "AT+KSTRINGB=1,1"
    };

            await Task.Run(() =>
            {
                try
                {
                    using (var port = new SerialPort(portName, 9600))
                    {
                        port.Open();
                        foreach (var cmd in commands)
                        {
                            ExecuteATCommand(port, cmd).Wait();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => AddLog($"Error: {ex.Message}"));
                }
            });
        }

        private async void btn5Gb_Click(object sender, RoutedEventArgs e)
        {
            if (cmbComPorts.SelectedItem == null || string.IsNullOrWhiteSpace(cmbComPorts.SelectedItem.ToString()))
            {
                AddLog("No Devices");
                return;
            }

            string fullPortName = cmbComPorts.SelectedItem.ToString();
            string portName = System.Text.RegularExpressions.Regex.Match(fullPortName, @"\(COM\d+\)").Value.Trim('(', ')');

            if (string.IsNullOrEmpty(portName) || !SerialPort.GetPortNames().Contains(portName))
            {
                AddLog("Device disconnected");
                return;
            }

            string[] commands = {
        "AT+SWATD=0",
        "AT+ACTIVATE=0,0,0",
        "AT+SWATD=1",
        "AT+PRECONFG=2,WWD"
    };

            await Task.Run(() =>
            {
                try
                {
                    using (var port = new SerialPort(portName, 9600))
                    {
                        port.Open();
                        foreach (var cmd in commands)
                        {
                            ExecuteATCommand(port, cmd).Wait();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => AddLog($"Error: {ex.Message}"));
                }
            });
        }





        private async Task ExecuteATCommand(SerialPort port, string command)
        {
            AddLog($"> {command}");
            port.WriteLine(command);

            await Task.Delay(200);
            string response = port.ReadExisting();
            AddLog($"< {response.Trim()}");
        }

        private void btn5G_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Plz Confirm u have open the Secret Menu via Menu button!",
                "Confirmation",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                ExecuteAdbCommand("devices");
                ExecuteAdbCommand("help");
            }
        }


        private async void ExecuteAdbCommand(string arguments)
        {
            string adbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "adb.exe");

            AddLog($"$ {adbPath} {arguments}");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                if (!string.IsNullOrEmpty(output)) AddLog(output);
                if (!string.IsNullOrEmpty(error)) AddLog($"Error: {error}");
            }
            catch (Exception ex)
            {
                AddLog($"ADB Error: {ex.Message}");
            }
        }


        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void AddLog(string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                rtbLog.Document.Blocks.Add(new Paragraph(new Run(message)));
                rtbLog.ScrollToEnd();
            });
        }
    }

    // AboutWindow.xaml
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            Title = "About";
            Width = 300;
            Height = 150;
            Content = new TextBlock
            {
                Text = "Littlenine",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }
    }


}