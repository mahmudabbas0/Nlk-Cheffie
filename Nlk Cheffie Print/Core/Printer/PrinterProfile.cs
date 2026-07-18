using System;
using System.Text.RegularExpressions;

namespace Nlk_Cheffie_Print.Core.Printer
{
    public sealed class PrinterProfile
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int RasterWidthDots { get; }
        public int TextColumns { get; }
        public int BarcodeWidthDots { get; }
        public bool UsesWindowsGdi { get; }

        public PrinterProfile(string id, string displayName, int rasterWidthDots, int textColumns, int barcodeWidthDots, bool usesWindowsGdi = false)
        {
            Id = id;
            DisplayName = displayName;
            RasterWidthDots = rasterWidthDots;
            TextColumns = textColumns;
            BarcodeWidthDots = barcodeWidthDots;
            UsesWindowsGdi = usesWindowsGdi;
        }
    }

    public static class PrinterProfileResolver
    {
        public const string EscPos58 = "escpos_58";
        public const string EscPos80 = "escpos_80";
        public const string WindowsGdi = "windows_gdi";

        private static readonly PrinterProfile Profile58 = new(EscPos58, "ESC/POS 58 mm", 384, 32, 384);
        private static readonly PrinterProfile Profile80 = new(EscPos80, "ESC/POS 80 mm", 576, 48, 512);
        private static readonly PrinterProfile ProfileWindowsGdi = new(WindowsGdi, "Windows yazıcı sürücüsü (GDI)", 576, 48, 512, true);

        public static PrinterProfile Resolve(PrinterInfo? printer)
        {
            string profileId = printer?.Profile?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrEmpty(profileId))
            {
                profileId = InferProfileId(printer);
            }

            return profileId switch
            {
                EscPos58 => Profile58,
                WindowsGdi => ProfileWindowsGdi,
                _ => Profile80
            };
        }

        public static string InferProfileId(PrinterInfo? printer)
        {
            if (printer != null && string.Equals(printer.Type, "win32_gdi", StringComparison.OrdinalIgnoreCase))
            {
                return WindowsGdi;
            }

            string name = $"{printer?.Name} {printer?.Id}";
            return Regex.IsMatch(name, @"(?i)(?:pos[- ]?58|58\s*mm|\b58\b)")
                ? EscPos58
                : EscPos80;
        }
    }
}
