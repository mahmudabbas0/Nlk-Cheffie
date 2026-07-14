using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Nlk_Cheffie_Print.Core.Printer
{
    public static class EscPosDriver
    {
        // Struct declarations for winspool.drv APIs
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        /// <summary>
        /// Sends raw bytes directly to an IP Network printer.
        /// </summary>
        public static void SendRawToIP(string ip, int port, byte[] bytes)
        {
            using (var client = new TcpClient())
            {
                var result = client.BeginConnect(ip, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
                if (!success)
                {
                    throw new TimeoutException($"Yazıcıya bağlanılamadı (IP: {ip}:{port})");
                }
                client.EndConnect(result);

                using (var stream = client.GetStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
            }
        }

        /// <summary>
        /// Sends raw ESC/POS bytes directly to a local/USB printer via Windows Spooler APIs.
        /// </summary>
        public static void SendRawToWin32(string printerName, byte[] bytes)
        {
            IntPtr hPrinter;
            if (!OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                int err = Marshal.GetLastWin32Error();
                throw new Exception($"Yazıcı açılamadı (Win32 Hata Kodu: {err})");
            }

            try
            {
                var di = new DOCINFOA
                {
                    pDocName = "Cheffie Print Job",
                    pDataType = "RAW"
                };

                if (!StartDocPrinter(hPrinter, 1, di))
                {
                    int err = Marshal.GetLastWin32Error();
                    throw new Exception($"Yazdırma belgesi başlatılamadı (Hata: {err})");
                }

                try
                {
                    if (!StartPagePrinter(hPrinter))
                    {
                        int err = Marshal.GetLastWin32Error();
                        throw new Exception($"Yazdırma sayfası başlatılamadı (Hata: {err})");
                    }

                    IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                    Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);

                    try
                    {
                        int dwWritten;
                        if (!WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out dwWritten))
                        {
                            int err = Marshal.GetLastWin32Error();
                            throw new Exception($"Yazıcıya veri yazılamadı (Hata: {err})");
                        }
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pUnmanagedBytes);
                        EndPagePrinter(hPrinter);
                    }
                }
                finally
                {
                    EndDocPrinter(hPrinter);
                }
            }
            finally
            {
                ClosePrinter(hPrinter);
            }
        }

        /// <summary>
        /// Spools a System.Drawing bitmap to a local Windows printer using standard GDI+ printing.
        /// </summary>
        public static void SendBitmapToWin32GDI(string printerName, Bitmap bitmap)
        {
            using (var pd = new PrintDocument())
            {
                pd.PrinterSettings.PrinterName = printerName;
                pd.PrintPage += (s, e) =>
                {
                    if (e.Graphics != null)
                    {
                        // Calculate printing width fitting the page margins
                        float printWidth = e.PageBounds.Width - e.MarginBounds.Left - e.MarginBounds.Right;
                        float ratio = printWidth / bitmap.Width;
                        float printHeight = bitmap.Height * ratio;

                        // Draw image fitting margins
                        e.Graphics.DrawImage(bitmap, e.MarginBounds.Left, e.MarginBounds.Top, printWidth, printHeight);
                        e.HasMorePages = false;
                    }
                };

                pd.Print();
            }
        }
    }
}
