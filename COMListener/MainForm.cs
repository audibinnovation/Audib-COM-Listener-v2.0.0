/*
 * Project: COM Listener v2.0.0
 * File: MainForm.cs
 * Author: Arun (Audib Innovation)
 * Created: May 2026
 * 
 * Description:
 *   Windows Forms application for monitoring and debugging serial communication
 *   with embedded devices (ESP32, STM32, Arduino). Provides a clean dashboard
 *   with connection stats, live log window, and command send box.
 * 
 * Features:
 *   - Connect/disconnect to COM ports with configurable baud rate
 *   - Real-time statistics (Received KB, Sent KB, Packets, Errors, Uptime)
 *   - Connection status indicator (green/red)
 *   - Scrollable log window with save-to-file option
 *   - Command input box with Send button and Enter key shortcut
 * 
 * License:
 *   MIT License. See LICENSE file in repository for details.
 */


using System;
using System.IO.Ports;
using System.Windows.Forms;
using System.Drawing;

namespace COMListenerGUI
{
    public partial class MainForm : Form
    {
        private SerialPort port;
        private int bytesReceived = 0;
        private int bytesSent = 0;
        private int packets = 0;
        private int errors = 0;
        private DateTime startTime;

        public MainForm()
        {
            InitializeComponent();
            startTime = DateTime.Now;

            // Timer for uptime
            Timer uptimeTimer = new Timer { Interval = 1000 };
            uptimeTimer.Tick += (s, e) =>
            {
                TimeSpan up = DateTime.Now - startTime;
                lblUptime.Text = $"Uptime: {up:hh\\:mm\\:ss}";
            };
            uptimeTimer.Start();

            // Allow Enter key to send directly from txtSend
            txtSend.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnSend_Click(s, e);
                    e.SuppressKeyPress = true; // prevent ding sound
                }
            };
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                port = new SerialPort(txtPort.Text, int.Parse(txtBaud.Text), Parity.None, 8, StopBits.One);
                port.DataReceived += Port_DataReceived;
                port.Open();
                lblStatus.Text = "CONNECTED";
                statusIcon.BackColor = Color.Lime;
                AppendLog($"Connected to {txtPort.Text} at {txtBaud.Text} baud...\r\n");
            }
            catch (Exception ex)
            {
                errors++;
                lblErrors.Text = $"Errors: {errors}";
                AppendLog($"Error: {ex.Message}\r\n");
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
                lblStatus.Text = "DISCONNECTED";
                statusIcon.BackColor = Color.Red;
                AppendLog("Disconnected.\r\n");
            }
        }

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog { Filter = "Text Files|*.txt" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(sfd.FileName, txtLog.Text);
                AppendLog($"Log saved to {sfd.FileName}\r\n");
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (port != null && port.IsOpen)
            {
                string cmd = txtSend.Text;
                if (!string.IsNullOrWhiteSpace(cmd))
                {
                    port.WriteLine(cmd);
                    bytesSent += cmd.Length;

                    lblBytesSent.Text = $"Sent: {bytesSent / 1024.0:F2} KB";
                    AppendLog($"Sent: {cmd}\r\n");
                    txtSend.Clear();
                }
            }
            else
            {
                AppendLog("Port not open. Cannot send.\r\n");
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = port.ReadLine();
                bytesReceived += data.Length;
                packets++;

                lblBytesReceived.Text = $"Received: {bytesReceived / 1024.0:F2} KB";
                lblPackets.Text = $"Packets: {packets}";

                AppendLog($"Received: {data}\r\n");
            }
            catch
            {
                errors++;
                lblErrors.Text = $"Errors: {errors}";
            }
        }

        private void AppendLog(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendLog), text);
            }
            else
            {
                txtLog.AppendText(text);
            }
        }
    }
}
