/*
 * Project: COM Listener v2.0.0
 * File: MainForm.Designer.cs
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


using System;   // Required for EventHandler
using System.Drawing;
using System.Windows.Forms;

namespace COMListenerGUI
{
    partial class MainForm
    {
        private TextBox txtLog;
        private TextBox txtPort;
        private TextBox txtBaud;
        private TextBox txtSend;   // <-- new textbox for sending
        private Button btnConnect, btnDisconnect, btnSaveLog, btnSend;
        private Label lblStatus, lblBytesReceived, lblBytesSent, lblPackets, lblErrors, lblUptime;
        private Panel statusIcon, topBar;

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Top bar panel
            topBar = new Panel();
            topBar.Dock = DockStyle.Top;
            topBar.Height = 100;
            topBar.BackColor = Color.FromArgb(10, 10, 20);
            this.Controls.Add(topBar);

            // Labels inside top bar
            lblBytesReceived = CreateLabel("Received: 0 KB", 10, 10);
            lblBytesSent = CreateLabel("Sent: 0 KB", 150, 10);
            lblPackets = CreateLabel("Packets: 0", 290, 10);
            lblErrors = CreateLabel("Errors: 0", 430, 10);
            lblUptime = CreateLabel("Uptime: 00:00:00", 570, 10);
            lblStatus = CreateLabel("DISCONNECTED", 730, 10);
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            statusIcon = new Panel { Width = 20, Height = 20, BackColor = Color.Red };
            statusIcon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            statusIcon.Location = new Point(topBar.Width - 30, 10);
            statusIcon.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(statusIcon.BackColor), 0, 0, 20, 20);
            };

            topBar.Controls.AddRange(new Control[] { lblBytesReceived, lblBytesSent, lblPackets, lblErrors, lblUptime, lblStatus, statusIcon });

            // Port + Baud
            txtPort = new TextBox { Text = "COM3", Left = 10, Top = 40, Width = 80, BackColor = Color.Black, ForeColor = Color.Cyan, BorderStyle = BorderStyle.FixedSingle };
            txtBaud = new TextBox { Text = "115200", Left = 100, Top = 40, Width = 80, BackColor = Color.Black, ForeColor = Color.Cyan, BorderStyle = BorderStyle.FixedSingle };
            topBar.Controls.AddRange(new Control[] { txtPort, txtBaud });

            // Buttons
            btnConnect = CreateButton("Connect", 200, 40, new EventHandler(btnConnect_Click));
            btnDisconnect = CreateButton("Disconnect", 300, 40, new EventHandler(btnDisconnect_Click));
            btnSaveLog = CreateButton("Save Log", 400, 40, new EventHandler(btnSaveLog_Click));
            topBar.Controls.AddRange(new Control[] { btnConnect, btnDisconnect, btnSaveLog });

            // Send TextBox + Button
            txtSend = new TextBox { Left = 510, Top = 40, Width = 200, BackColor = Color.Black, ForeColor = Color.Cyan, BorderStyle = BorderStyle.FixedSingle };
            btnSend = CreateButton("Send", 720, 40, new EventHandler(btnSend_Click));
            topBar.Controls.AddRange(new Control[] { txtSend, btnSend });

            // Log window (fills remaining space)
            txtLog = new TextBox();
            txtLog.Multiline = true;
            txtLog.Dock = DockStyle.Fill;
            txtLog.BackColor = Color.Black;
            txtLog.ForeColor = Color.Cyan;
            txtLog.Font = new Font("Consolas", 10, FontStyle.Bold);
            txtLog.ScrollBars = ScrollBars.Vertical;
            this.Controls.Add(txtLog);

            // MainForm
            this.Text = "Audib COM Listener v2.0.0";
            this.BackColor = Color.FromArgb(10, 10, 20);
            this.ClientSize = new Size(920, 500);
            this.ResumeLayout(false);
        }

        private Label CreateLabel(string text, int left, int top)
        {
            return new Label
            {
                Text = text,
                Left = left,
                Top = top,
                ForeColor = Color.Cyan,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };
        }

        private Button CreateButton(string text, int left, int top, EventHandler handler)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Left = left;
            btn.Top = top;
            btn.Width = 90;
            btn.Height = 30;
            btn.BackColor = Color.FromArgb(30, 30, 60);
            btn.ForeColor = Color.Cyan;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.DeepSkyBlue;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 90);
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Click += handler;
            return btn;
        }
    }
}
