/*
 * Project: COM Listener v2.0.0
 * File: Program.cs
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
using System.Windows.Forms;

namespace COMListenerGUI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
