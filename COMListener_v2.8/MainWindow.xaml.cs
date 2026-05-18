using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading; // Added for the Uptime Timer

namespace ComPortVisualizer
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;

        // --- STATS TRACKING VARIABLES ---
        private DispatcherTimer _uptimeTimer;
        private DateTime _connectionStartTime;
        private long _bytesReceived = 0;
        private long _bytesSent = 0;
        private int _packetsCount = 0;
        private int _errorCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            LoadAvailablePorts();

            // Setup the uptime timer to tick every 1 second
            _uptimeTimer = new DispatcherTimer();
            _uptimeTimer.Interval = TimeSpan.FromSeconds(1);
            _uptimeTimer.Tick += UptimeTimer_Tick;
        }

        private void CmbPorts_DropDownOpened(object sender, EventArgs e)
        {
            LoadAvailablePorts();
        }

        private void LoadAvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            CmbPorts.ItemsSource = ports;

            if (ports.Length > 0 && CmbPorts.SelectedIndex == -1)
            {
                CmbPorts.SelectedIndex = 0;
            }
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (CmbPorts.SelectedItem == null)
            {
                MessageBox.Show("Please select a COM port.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string selectedPort = CmbPorts.SelectedItem.ToString();
                string baudRateStr = ((System.Windows.Controls.ComboBoxItem)CmbBaudRate.SelectedItem).Content.ToString();
                int baudRate = int.Parse(baudRateStr);

                _serialPort = new SerialPort(selectedPort, baudRate, Parity.None, 8, StopBits.One);
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                // Reset and Start Stats
                ResetStats();
                _connectionStartTime = DateTime.Now;
                _uptimeTimer.Start();

                UpdateUIState(true);
                LogMessage($"[System] Connected to {selectedPort} at {baudRate} bps.");
            }
            catch (Exception ex)
            {
                IncrementError();
                MessageBox.Show($"Failed to connect: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            DisconnectPort();
        }

        private void DisconnectPort()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                _serialPort.Close();
                _serialPort.Dispose();

                _uptimeTimer.Stop();

                UpdateUIState(false);
                LogMessage("[System] Disconnected.");
            }
        }

        private string CleanIncomingData(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
                return rawData;

            string cleaned = Regex.Replace(rawData, @"\x1B\[[0-9;]*[a-zA-Z]", "");
            cleaned = cleaned.Replace("> ", "");
            return cleaned;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string rawData = _serialPort.ReadExisting();

                // Track received stats
                _bytesReceived += rawData.Length;
                _packetsCount++;
                UpdateStatsUI();

                string cleanData = CleanIncomingData(rawData);

                if (!string.IsNullOrEmpty(cleanData))
                {
                    Dispatcher.Invoke(() => LogMessage(cleanData, isIncoming: true));
                }
            }
            catch (Exception ex)
            {
                IncrementError();
                Dispatcher.Invoke(() => LogMessage($"[Error Reading]: {ex.Message}"));
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            SendCommand();
        }

        private void TxtSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendCommand();
            }
        }

        private void SendCommand()
        {
            if (_serialPort != null && _serialPort.IsOpen && !string.IsNullOrWhiteSpace(TxtSend.Text))
            {
                string command = TxtSend.Text;

                try
                {
                    _serialPort.WriteLine(command);

                    // Track sent stats (+2 for the \r\n that WriteLine adds)
                    _bytesSent += command.Length + 2;
                    _packetsCount++;
                    UpdateStatsUI();

                    LogMessage($"< {command}");
                    TxtSend.Clear();
                }
                catch (Exception ex)
                {
                    IncrementError();
                    LogMessage($"[Error Sending]: {ex.Message}");
                }
            }
        }

        private void LogMessage(string message, bool isIncoming = false)
        {
            TxtConsole.AppendText(message + (isIncoming ? "" : Environment.NewLine));
            TxtConsole.ScrollToEnd();
        }

        // --- STATS LOGIC METHODS ---

        private void UptimeTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan uptime = DateTime.Now - _connectionStartTime;
            TxtUptime.Text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                uptime.Hours, uptime.Minutes, uptime.Seconds);
        }

        private void UpdateStatsUI()
        {
            Dispatcher.Invoke(() =>
            {
                // Convert bytes to Kilobytes
                TxtRxKB.Text = (_bytesReceived / 1024.0).ToString("0.00");
                TxtTxKB.Text = (_bytesSent / 1024.0).ToString("0.00");
                TxtPackets.Text = _packetsCount.ToString();
                TxtErrors.Text = _errorCount.ToString();
            });
        }

        private void ResetStats()
        {
            _bytesReceived = 0;
            _bytesSent = 0;
            _packetsCount = 0;
            _errorCount = 0;
            UpdateStatsUI();
            TxtUptime.Text = "00:00:00";
        }

        private void IncrementError()
        {
            _errorCount++;
            UpdateStatsUI();
        }

        private void UpdateUIState(bool isConnected)
        {
            BtnConnect.IsEnabled = !isConnected;
            BtnDisconnect.IsEnabled = isConnected;
            CmbPorts.IsEnabled = !isConnected;
            CmbBaudRate.IsEnabled = !isConnected;

            StatusIndicator.Fill = isConnected ? new SolidColorBrush(Color.FromRgb(0, 229, 255)) : new SolidColorBrush(Colors.Red);
            TxtStatus.Text = isConnected ? "Connected" : "Disconnected";
            TxtStatus.Foreground = isConnected ? new SolidColorBrush(Color.FromRgb(0, 229, 255)) : new SolidColorBrush(Colors.White);
        }

        protected override void OnClosed(EventArgs e)
        {
            DisconnectPort();
            base.OnClosed(e);
        }
    }
}
