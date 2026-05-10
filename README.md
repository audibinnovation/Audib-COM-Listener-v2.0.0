# Audib COM Listener v2.0.0

A Windows Forms application for monitoring and debugging serial communication with embedded devices (tested with ESP32 Devkit v1).  
Provides a clean dashboard with connection stats, live log window, and command send box.

---

## ✨ Features
- Connect to any COM port with configurable baud rate
- Real‑time statistics: Received KB, Sent KB, Packets, Errors, Uptime
- Connection status indicator (green/red)
- Scrollable log window with ESP32/STM32 initialization messages
- Save logs to `.txt` file
- Command input box with **Send** button and Enter key shortcut
- Professional dark UI with cyan highlights

---

## 🛠 Requirements
- .NET 10.0 (Windows Desktop SDK)
- Windows 64‑bit (tested on Windows 10/11)
- Serial device (ESP32, STM32, Arduino, etc.)

---

## 🚀 Usage
1. Select COM port and baud rate (default: COM3, 115200).
2. Click **Connect** to start monitoring.
3. View live logs and statistics in the main window.
4. Type a command in the send box and press **Enter** or click **Send**.
5. Click **Save Log** to export session logs.
6. Use **Disconnect** to close the port safely.

---

## 🔧 Build Instructions
Clone the repo and build:

```bash
git clone https://github.com/yourusername/COMListener.git
cd COMListener
dotnet build -c Release

standalone .exe:
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

The executable will be in:
bin\Release\net10.0-windows\win-x64\publish\COMListener.exe

