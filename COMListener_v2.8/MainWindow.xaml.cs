using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ComPortVisualizer
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;

        public MainWindow()
        {
            InitializeComponent();
            LoadAvailablePorts();
        }

        // Dynamically load ports when the user clicks the dropdown
        private void CmbPorts_DropDownOpened(object sender, EventArgs e)
        {
            LoadAvailablePorts();
        }

        private void LoadAvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            CmbPorts.ItemsSource = ports;

            // Auto-select the first port if available
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

                // Read the selected Baud Rate from the UI
                string baudRateStr = ((System.Windows.Controls.ComboBoxItem)CmbBaudRate.SelectedItem).Content.ToString();
                int baudRate = int.Parse(baudRateStr);

                // Initialize the serial port
                _serialPort = new SerialPort(selectedPort, baudRate, Parity.None, 8, StopBits.One);

                // Subscribe to the data received event
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                UpdateUIState(true);
                LogMessage($"[System] Connected to {selectedPort} at {baudRate} bps.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect: {ex.Message}\nMake sure the port isn't being used by another program.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                // Unsubscribe from the event before closing
                _serialPort.DataReceived -= SerialPort_DataReceived;

                _serialPort.Close();
                _serialPort.Dispose();

                UpdateUIState(false);
                LogMessage("[System] Disconnected.");
            }
        }

        // Helper method to strip ANSI color codes and garbage characters from the ESP32
        private string CleanIncomingData(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
                return rawData;

            // 1. Remove ANSI escape sequences (color codes like \x1b[0;32m)
            string cleaned = Regex.Replace(rawData, @"\x1B\[[0-9;]*[a-zA-Z]", "");

            // 2. Remove stray standalone '>' characters that often glitch into ESP32 logs
            cleaned = cleaned.Replace("> ", "");

            return cleaned;
        }

        // Event handler for incoming data (Fires on a background thread)
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Read the raw messy data
                string rawData = _serialPort.ReadExisting();

                // Clean it up using the helper method
                string cleanData = CleanIncomingData(rawData);

                // Push ONLY the clean data to the UI thread
                if (!string.IsNullOrEmpty(cleanData))
                {
                    Dispatcher.Invoke(() =>
                    {
                        LogMessage(cleanData, isIncoming: true);
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogMessage($"[Error Reading]: {ex.Message}"));
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            SendCommand();
        }

        // Allow sending commands by pressing the Enter key
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
                    // WriteLine automatically appends a newline (\n) character
                    _serialPort.WriteLine(command);
                    LogMessage($"< {command}");
                    TxtSend.Clear();
                }
                catch (Exception ex)
                {
                    LogMessage($"[Error Sending]: {ex.Message}");
                }
            }
        }

        // Helper method to write to the terminal view
        private void LogMessage(string message, bool isIncoming = false)
        {
            // If it's outgoing system info, add a newline. If incoming, let the device dictate formatting.
            TxtConsole.AppendText(message + (isIncoming ? "" : Environment.NewLine));
            TxtConsole.ScrollToEnd();
        }

        // Toggle buttons, dropdowns, and status lights based on connection state
        private void UpdateUIState(bool isConnected)
        {
            BtnConnect.IsEnabled = !isConnected;
            BtnDisconnect.IsEnabled = isConnected;
            CmbPorts.IsEnabled = !isConnected;
            CmbBaudRate.IsEnabled = !isConnected; // Locks when connected, unlocks when disconnected

            StatusIndicator.Fill = isConnected ? new SolidColorBrush(Color.FromRgb(3, 218, 198)) : new SolidColorBrush(Colors.Red);
            TxtStatus.Text = isConnected ? "Connected" : "Disconnected";
            TxtStatus.Foreground = isConnected ? new SolidColorBrush(Color.FromRgb(3, 218, 198)) : new SolidColorBrush(Colors.Gray);
        }

        // Ensure the port closes cleanly when the user clicks the 'X' to close the app
        protected override void OnClosed(EventArgs e)
        {
            DisconnectPort();
            base.OnClosed(e);
        }
    }
}
