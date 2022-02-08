using ConsoleControlAPI;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Avrdude_GUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ArrayList clockRates;
        ArrayList programmer;
        string version = "v1.0.0";

        public MainWindow()
        {
            InitializeComponent();


            Version.Content = version;
            string[] ports = SerialPort.GetPortNames();

            ComPortCombo.Items.Add("USB");
            foreach (string port in ports)
            {
                ComPortCombo.Items.Add(port);
            }

            clockRates = new ArrayList();
            clockRates = new ArrayList();
            clockRates.Add("-");
            clockRates.Add("5");
            clockRates.Add("10");
            clockRates.Add("15");
            clockRates.Add("20");
            clockRates.Add("50");
            clockRates.Add("100");
            clockRates.Add("200");
            clockRates.Add("1000");



            /*
           baudRates = new ArrayList();
            baudRates.Add("-");
            baudRates.Add("300");
            baudRates.Add("1200");
            baudRates.Add("2400");
            baudRates.Add("4800");
            baudRates.Add("9600");
            baudRates.Add("19200");
            baudRates.Add("38400");
            baudRates.Add("57600");
            baudRates.Add("74880");
            baudRates.Add("115200");

             */

            foreach (String item in clockRates) {
                ClockRateCombo.Items.Add(item);
            }

            programmer = new ArrayList();
            programmer.Add("USBAsp");
            programmer.Add("AVRIsp");

            foreach (String item in programmer)
            {
                ProgrammerCombo.Items.Add(item);
            }

            ClockRateCombo.SelectedIndex = 0;
            ComPortCombo.SelectedIndex = 0;
            ProgrammerCombo.SelectedIndex = 0;
        }

        private void openConfigFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog configDialog = new OpenFileDialog();
            configDialog.InitialDirectory = "C:\\Users";

            if (configDialog.ShowDialog() == true)
            {
                ConfigFile.Text = configDialog.FileName;
            }
        }

        private void openHexFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog hexFileDialog = new OpenFileDialog();
            hexFileDialog.InitialDirectory = "C:\\Users";

            if (hexFileDialog.ShowDialog() == true)
            {
                HexFile.Text = hexFileDialog.FileName;
            }

        }

        private void openAvrdudeBtn(object sender, RoutedEventArgs e)
        {
            OpenFileDialog avrdudeDialog = new OpenFileDialog();
            avrdudeDialog.InitialDirectory = "C:\\Users";

            if (avrdudeDialog.ShowDialog() == true)
            {
                AvrdudeExe.Text = avrdudeDialog.FileName;
            }
        }

        private string getAvrdudeCommand(bool erase)
        {
            string avrdude = AvrdudeExe.Text;
            string config = ConfigFile.Text;
            string hexFile = HexFile.Text;

            string prog = ProgrammerCombo.SelectedValue.ToString();
            string port = ComPortCombo.SelectedValue.ToString();
            string clockRate = ClockRateCombo.SelectedValue.ToString();
            string avr = AVRDevice.Text;

            string cmd = avrdude;

            if(avrdude.Equals(""))
            {
                MessageBox.Show("Avrdude-GUI error: No avrdude.exe specified.");
                return "";
            }

            if (config.Equals(""))
            {
                MessageBox.Show("Avrdude-GUI error: No avrdude config file specified.");
                return "";
            }

            if (hexFile.Equals("") && !erase)
            {
                MessageBox.Show("Avrdude-GUI error: No hex file specified.");
                return "";
            }

            if (avr.Equals(""))
            {
                MessageBox.Show("Avrdude-GUI error: No AVR device specified.");
                return "";
            }

            string cmdParams = " -C\"" + config + "\" -v -p " + avr + " -c" + prog + " -P" + port + " -Uflash:w:" + hexFile + ":i";
            if (erase)
            {
                cmdParams = " -C\"" + config + "\" -v -p " + avr + " -c" + prog + " -P" + port + " -e";
            }

            if (SafeMode.IsChecked == true)
            {
                cmdParams += " -s";
            }

            if(DontWriteOnDevice.IsChecked == true)
            {
                cmdParams += " -n";
            }

            if (DontVerify.IsChecked == true)
            {
                cmdParams += " -V";
            }

            if (!clockRate.Equals("-") && !clockRate.Equals(""))
            {
                cmdParams += " -B " + clockRate;
            }

            if (OverrideOption.IsChecked == true)
            {
                cmdParams += " -F";
            }

            if (DisableEraseOption.IsChecked == true)
            {
                cmdParams += " -D";
            }



            return cmd + cmdParams;
        }

        private void startCommandProcess(string cmd)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            // startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + cmd;
            startInfo.Verb = "runas";
            ConsoleControl.StartProcess(startInfo);
            StartProgess();
            while (ConsoleControl.IsProcessRunning)
            {
                RunProgess();
                if (!ConsoleControl.IsProcessRunning)
                {
                    StopProgess();
                }
            }
        }

        private void flash(object sender, RoutedEventArgs e)
        {
            ConsoleControl.ClearOutput();
           // MessageBox.Show(getAvrdudeCommand(false));
            if (!getAvrdudeCommand(false).Equals(""))
            {
                startCommandProcess(getAvrdudeCommand(false));
            }
        }

        private void StartProgess()
        {
            ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value = 50, DispatcherPriority.Background);

        }

        private void RunProgess()
        {
            ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value += 1, DispatcherPriority.Background);
        }

        private void StopProgess()
        {
            ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value = 100, DispatcherPriority.Background);
            ProgressBar.Value = 0;
        }

        private void clearForm(object sender, RoutedEventArgs e)
        {
            ConsoleControl.ClearOutput();
            DisableEraseOption.IsChecked = false;
            OverrideOption.IsChecked = false;
            SafeMode.IsChecked = false;
            DontWriteOnDevice.IsChecked = false;
            DontVerify.IsChecked = false;
            ClockRateCombo.SelectedIndex = 0;
            ComPortCombo.SelectedIndex = 0;
            ProgrammerCombo.SelectedIndex = 0;
            AVRDevice.Text = "";
            HexFile.Text = "";
            ConfigFile.Text = "";
            ProgressBar.Value = 0;
        }

        private void erase(object sender, RoutedEventArgs e)
        {
            ConsoleControl.ClearOutput();
            //MessageBox.Show(getAvrdudeCommand(true));
            if (!getAvrdudeCommand(true).Equals(""))
            {
                startCommandProcess(getAvrdudeCommand(true));
            }

        }

        private void openHelp(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.globment.de/avrdude-gui.html");
        }


    }
}
