using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Models;
using QRCoder;

namespace Nlk_Cheffie_Print.Core.Printer
{
    public static class ReceiptRenderer
    {
        private static int PageWidthChars = 32;

        public static byte[] RenderToEscPos(SlipTemplate template, JsonElement data, int textColumns = 32, int barcodeWidthDots = 384)
        {
            PageWidthChars = textColumns;
            var slip = data;
            if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("slip_data", out var sd) && sd.ValueKind == JsonValueKind.Object)
            {
                slip = sd;
            }

            var ctx = BuildContext(slip);
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            // Initialize printer
            writer.Write(new byte[] { 0x1B, 0x40 });

            // Cancel Kanji/Chinese mode to ensure western codepages are used (crucial for Chinese/generic printers)
            writer.Write(new byte[] { 0x1C, 0x2E });

            // Set codepage based on configuration (default 18 for generic PC857, or 61 for newer Epson)
            int codePage = ConfigManager.Current.App.CodePage;
            writer.Write(new byte[] { 0x1B, 0x74, (byte)codePage });

            // Render Header
            RenderSectionEscPos(template.Header, ctx, writer, slip);

            // Render Body
            RenderSectionEscPos(template.Body, ctx, writer, slip);

            // Render Footer
            RenderSectionEscPos(template.Footer, ctx, writer, slip);

            // Feed paper and cut
            writer.Write(new byte[] { 0x0A, 0x0A, 0x0A, 0x0A }); // 4 line feeds
            writer.Write(new byte[] { 0x1D, 0x56, 0x41, 0x03 }); // Cut command

            return ms.ToArray();
        }

        private static void RenderSectionEscPos(List<TemplateElement> section, Dictionary<string, string> ctx, BinaryWriter writer, JsonElement data)
        {
            var optimized = OptimizeSection(section, ctx);
            foreach (var el in optimized)
            {
                if (el.Type == "separator")
                {
                    WriteTextEscPos(writer, new string('-', PageWidthChars) + "\n", "left", "A", "1x");
                }
                else if (el.Type == "text")
                {
                    string text = Substitute(el.Content, ctx);
                    WriteTextEscPos(writer, text + "\n", el.Align, el.Font, el.Size);
                }
                else if (el.Type == "items")
                {
                    RenderItemsEscPos(el, data, writer);
                }
                else if (el.Type == "qrcode")
                {
                    string content = Substitute(el.Content, ctx);
                    WriteQrEscPos(writer, content);
                }
                else if (el.Type == "barcode")
                {
                    string content = Substitute(el.Content, ctx);
                    WriteTextEscPos(writer, $"[BARCODE: {content}]\n", "center", "A", "1x");
                }
                else if (el.Type == "logo")
                {
                    string path = el.Path;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        try
                        {
                            using (var img = Image.FromFile(path))
                            {
                                // Resize logo: 160px is optimal width for thermal printer (fits within 384/576 dots)
                                int w = Math.Min(img.Width, 160);
                                int h = (int)(img.Height * ((double)w / img.Width));
                                
                                using (var bmp = new Bitmap(w, h))
                                {
                                    using (var g = Graphics.FromImage(bmp))
                                    {
                                        g.Clear(Color.White);
                                        g.DrawImage(img, 0, 0, w, h);
                                    }
                                    
                                    // Align center for the logo image
                                    writer.Write(new byte[] { 0x1B, 0x61, 1 });
                                    
                                    // Render bitmap to ESC/POS raster graphic bytes without page initialization/cutting
                                    byte[] logoBytes = RenderBitmapToEscPosBytes(bmp);
                                    writer.Write(logoBytes);
                                    
                                    // Reset alignment to default Left
                                    writer.Write(new byte[] { 0x1B, 0x61, 0 });
                                }
                            }
                        }
                        catch
                        {
                            WriteTextEscPos(writer, "[LOGO]\n", "center", "A", "1x");
                        }
                    }
                    else
                    {
                        WriteTextEscPos(writer, "[LOGO]\n", "center", "A", "1x");
                    }
                }
            }
        }

        private static string CleanTurkishCharacters(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text
                .Replace("Ğ", "G")
                .Replace("ğ", "g")
                .Replace("Ş", "S")
                .Replace("ş", "s")
                .Replace("İ", "I")
                .Replace("ı", "i");
        }

        private static void WriteTextEscPos(BinaryWriter writer, string text, string align, string font, string size)
        {
            text = CleanTurkishCharacters(text);
            // Alignment
            byte alignByte = align.ToLower() switch
            {
                "center" => 1,
                "right" => 2,
                _ => 0
            };
            writer.Write(new byte[] { 0x1B, 0x61, alignByte });

            // Font thickness / weight
            byte fontByte = font.ToUpper() == "B" ? (byte)1 : (byte)0;
            writer.Write(new byte[] { 0x1B, 0x45, fontByte });

            // Size (Normal/Large)
            byte sizeByte = size == "2x" ? (byte)0x11 : (byte)0x00; // Double height & width
            writer.Write(new byte[] { 0x1D, 0x21, sizeByte });

            // Encode using the configured encoding (default is ibm857)
            string encName = string.IsNullOrWhiteSpace(ConfigManager.Current.App.EncodingName) ? "ibm857" : ConfigManager.Current.App.EncodingName;
            byte[] bytes = Encoding.GetEncoding(encName).GetBytes(text);
            writer.Write(bytes);
        }

        private static void RenderItemsEscPos(TemplateElement el, JsonElement data, BinaryWriter writer)
        {
            // Check order status for cancellation (checking both order_info and root status)
            string status = data.TryGetProperty("order_info", out var oi) && oi.TryGetProperty("status", out var st) ? GetElementText(st) : "";
            if (string.IsNullOrEmpty(status) && data.TryGetProperty("status", out var st2)) status = GetElementText(st2);

            if (status.ToLower() == "canceled")
            {
                WriteTextEscPos(writer, "*** SIPARIS IPTAL EDILDI ***\n", "center", "B", "2x");
                WriteTextEscPos(writer, "--------------------------------\n", "left", "A", "1x");
            }

            JsonElement itemsProp = default;
            if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("items", out var wsItems) && wsItems.ValueKind == JsonValueKind.Array)
                itemsProp = wsItems;
            else if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("order_items", out var apiItems) && apiItems.ValueKind == JsonValueKind.Array)
                itemsProp = apiItems;

            if (itemsProp.ValueKind == JsonValueKind.Array)
            {
                var jsonItems = itemsProp.EnumerateArray().ToList();
                double orderSubtotal = GetOrderSubtotal(data);
                for (int itemIndex = 0; itemIndex < jsonItems.Count; itemIndex++)
                {
                    var item = jsonItems[itemIndex];
                    int qty = item.TryGetProperty("quantity", out var q) ? (q.ValueKind == JsonValueKind.Number ? q.GetInt32() : 1) : 1;
                    
                    // Parse name (nested or flat support)
                    string name = item.TryGetProperty("name", out var n) ? GetElementText(n) : "";
                    if (string.IsNullOrEmpty(name)) name = item.TryGetProperty("product_name", out var pn) ? GetElementText(pn) : "";
                    if (string.IsNullOrEmpty(name) && item.TryGetProperty("product", out var pObj) && pObj.ValueKind == JsonValueKind.Object)
                    {
                        name = pObj.TryGetProperty("name", out var pn2) ? GetElementText(pn2) : "";
                    }

                    var extras = el.ShowCustomizations
                        ? ParseItemExtras(item, data, itemIndex, jsonItems.Count, orderSubtotal)
                        : new List<string>();

                    // Parse price/total
                    string total = ResolveDisplayItemPrice(item, qty, extras, orderSubtotal, jsonItems.Count);

                    string line = $"{qty}x {name}";
                    if (el.ShowPrice && !string.IsNullOrEmpty(total))
                    {
                        string priceText = total;
                        if (!string.IsNullOrEmpty(el.CurrencySymbol)) priceText += $" {el.CurrencySymbol}";

                        if (el.RightAlignPrice)
                        {
                            int spaceLen = PageWidthChars - line.Length - priceText.Length;
                            if (spaceLen > 0) line = line + new string(' ', spaceLen) + priceText;
                            else line += "  " + priceText;
                        }
                        else
                        {
                            line += "  " + priceText;
                        }
                    }

                    WriteTextEscPos(writer, line + "\n", "left", "A", "1x");

                    // Customizations
                    if (el.ShowCustomizations && extras.Count > 0)
                    {
                        WriteTextEscPos(writer, $" - Extra: {string.Join(", ", extras)}\n", "left", "A", "1x");
                    }

                    // Notes
                    if (el.ShowNotes && item.TryGetProperty("notes", out var noteProp))
                    {
                        string note = GetElementText(noteProp);
                        if (!string.IsNullOrEmpty(note))
                        {
                            WriteTextEscPos(writer, $" - Not: {note}\n", "left", "A", "1x");
                        }
                    }
                }
            }
        }

        public static Bitmap RenderToBitmap(SlipTemplate template, JsonElement data, int widthPx = 550, int barcodeWidthDots = 384)
        {
            var slip = data;
            if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("slip_data", out var sd) && sd.ValueKind == JsonValueKind.Object)
            {
                slip = sd;
            }

            // Render receipt to dynamic heights using system drawing
            var ctx = BuildContext(slip);

            // Prepare a temporary small bitmap to measure bounds
            using (var tempBmp = new Bitmap(widthPx, 10))
            using (var g = Graphics.FromImage(tempBmp))
            {
                g.Clear(Color.White);
                int yOffset = 10;
                int margin = 16;
                int usableWidth = widthPx - (margin * 2);

                using (Font fnNormal = new Font("Courier New", 10, FontStyle.Regular))
                using (Font fnBold = new Font("Courier New", 10, FontStyle.Bold))
                using (Font fnHeader = new Font("Courier New", 14, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.Black))
                {
                    // Compute height offset first
                    yOffset = MeasureSection(template.Header, ctx, g, yOffset, usableWidth, fnNormal, fnBold, fnHeader, slip);
                    yOffset = MeasureSection(template.Body, ctx, g, yOffset, usableWidth, fnNormal, fnBold, fnHeader, slip);
                    yOffset = MeasureSection(template.Footer, ctx, g, yOffset, usableWidth, fnNormal, fnBold, fnHeader, slip);

                    // Create final sized bitmap
                    var finalBmp = new Bitmap(widthPx, yOffset + 30);
                    using (var finalG = Graphics.FromImage(finalBmp))
                    {
                        finalG.Clear(Color.White);
                        int drawY = 10;
                        drawY = DrawSection(template.Header, ctx, finalG, drawY, usableWidth, margin, fnNormal, fnBold, fnHeader, brush, slip);
                        drawY = DrawSection(template.Body, ctx, finalG, drawY, usableWidth, margin, fnNormal, fnBold, fnHeader, brush, slip);
                        drawY = DrawSection(template.Footer, ctx, finalG, drawY, usableWidth, margin, fnNormal, fnBold, fnHeader, brush, slip);
                    }
                    return finalBmp;
                }
            }
        }

        private static int MeasureSection(List<TemplateElement> section, Dictionary<string, string> ctx, Graphics g, int yOffset, int usableWidth, Font fnNormal, Font fnBold, Font fnHeader, JsonElement data)
        {
            var optimized = OptimizeSection(section, ctx);
            foreach (var el in optimized)
            {
                yOffset = MeasureElement(el, ctx, g, yOffset, usableWidth, fnNormal, fnBold, fnHeader, data);
            }
            return yOffset;
        }

        private static int MeasureElement(TemplateElement el, Dictionary<string, string> ctx, Graphics g, int yOffset, int usableWidth, Font fnNormal, Font fnBold, Font fnHeader, JsonElement data)
        {
            if (el.Type == "separator") return yOffset + 12;
            if (el.Type == "qrcode") return yOffset + 110;
            if (el.Type == "barcode") return yOffset + 50;
            if (el.Type == "logo") return yOffset + 60;
            if (el.Type == "items")
            {
                JsonElement itemsProp = default;
                if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("items", out var wsItems) && wsItems.ValueKind == JsonValueKind.Array)
                    itemsProp = wsItems;
                else if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("order_items", out var apiItems) && apiItems.ValueKind == JsonValueKind.Array)
                    itemsProp = apiItems;

                if (itemsProp.ValueKind == JsonValueKind.Array)
                {
                    var jsonItems = itemsProp.EnumerateArray().ToList();
                    double orderSubtotal = GetOrderSubtotal(data);
                    for (int itemIndex = 0; itemIndex < jsonItems.Count; itemIndex++)
                    {
                        var item = jsonItems[itemIndex];
                        yOffset += 18; // main item row
                        if (el.ShowCustomizations && ParseItemExtras(item, data, itemIndex, jsonItems.Count, orderSubtotal).Count > 0)
                            yOffset += 16;
                        if (el.ShowNotes && item.TryGetProperty("notes", out var notes) && !string.IsNullOrEmpty(notes.GetString()))
                            yOffset += 16;
                    }
                }
                return yOffset;
            }

            // Normal text
            string raw = Substitute(el.Content, ctx);
            Font fn = el.Font == "B" ? fnBold : fnNormal;
            if (el.Size == "2x") fn = fnHeader;

            SizeF size = g.MeasureString(raw, fn, usableWidth);
            return yOffset + (int)size.Height + 4;
        }

        private static int DrawSection(List<TemplateElement> section, Dictionary<string, string> ctx, Graphics g, int yOffset, int usableWidth, int margin, Font fnNormal, Font fnBold, Font fnHeader, Brush brush, JsonElement data)
        {
            var optimized = OptimizeSection(section, ctx);
            foreach (var el in optimized)
            {
                yOffset = DrawElement(el, ctx, g, yOffset, usableWidth, margin, fnNormal, fnBold, fnHeader, brush, data);
            }
            return yOffset;
        }

        private static int DrawElement(TemplateElement el, Dictionary<string, string> ctx, Graphics g, int yOffset, int usableWidth, int margin, Font fnNormal, Font fnBold, Font fnHeader, Brush brush, JsonElement data)
        {
            if (el.Type == "separator")
            {
                g.DrawLine(Pens.Black, margin, yOffset + 4, margin + usableWidth, yOffset + 4);
                return yOffset + 12;
            }

            if (el.Type == "qrcode")
            {
                string text = Substitute(el.Content, ctx);
                if (string.IsNullOrEmpty(text)) text = "https://nlkmenu.com";

                try
                {
                    using (var qrGenerator = new QRCodeGenerator())
                    using (var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
                    using (var qrCode = new QRCode(qrCodeData))
                    {
                        using (Bitmap qrBmp = qrCode.GetGraphic(3))
                        {
                            int x = margin + (usableWidth - qrBmp.Width) / 2;
                            g.DrawImage(qrBmp, x, yOffset);
                            return yOffset + qrBmp.Height + 10;
                        }
                    }
                }
                catch
                {
                    g.DrawRectangle(Pens.Black, margin + (usableWidth - 80) / 2, yOffset, 80, 80);
                    return yOffset + 90;
                }
            }

            if (el.Type == "barcode")
            {
                string content = Substitute(el.Content, ctx);
                g.DrawRectangle(Pens.Black, margin + (usableWidth - 140) / 2, yOffset, 140, 30);
                g.DrawString(content, fnNormal, brush, margin + (usableWidth - 140) / 2 + 10, yOffset + 32);
                return yOffset + 50;
            }

            if (el.Type == "logo")
            {
                string path = el.Path;
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try
                    {
                        using (var img = Image.FromFile(path))
                        {
                            int w = Math.Min(img.Width, 120);
                            int h = (int)(img.Height * ((double)w / img.Width));
                            int x = margin + (usableWidth - w) / 2;
                            g.DrawImage(img, x, yOffset, w, h);
                            return yOffset + h + 10;
                        }
                    }
                    catch
                    {
                        // ignore error and render fallback box
                    }
                }
                g.DrawRectangle(Pens.Gray, margin + (usableWidth - 100) / 2, yOffset, 100, 40);
                g.DrawString("[LOGO]", fnNormal, Brushes.Gray, margin + (usableWidth - 100) / 2 + 25, yOffset + 12);
                return yOffset + 50;
            }

            if (el.Type == "items")
            {
                // Check order status for cancellation (checking both order_info and root status)
                string status = data.TryGetProperty("order_info", out var oi) && oi.TryGetProperty("status", out var st) ? GetElementText(st) : "";
                if (string.IsNullOrEmpty(status) && data.TryGetProperty("status", out var st2)) status = GetElementText(st2);

                if (status.ToLower() == "canceled")
                {
                    g.DrawString("*** SIPARIS IPTAL EDILDI ***", fnHeader, Brushes.Red, margin + (usableWidth - g.MeasureString("*** SIPARIS IPTAL EDILDI ***", fnHeader).Width) / 2, yOffset);
                    yOffset += 24;
                    g.DrawLine(Pens.Black, margin, yOffset + 4, margin + usableWidth, yOffset + 4);
                    yOffset += 12;
                }

                JsonElement itemsProp = default;
                if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("items", out var wsItems) && wsItems.ValueKind == JsonValueKind.Array)
                    itemsProp = wsItems;
                else if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("order_items", out var apiItems) && apiItems.ValueKind == JsonValueKind.Array)
                    itemsProp = apiItems;

                if (itemsProp.ValueKind == JsonValueKind.Array)
                {
                    var jsonItems = itemsProp.EnumerateArray().ToList();
                    double orderSubtotal = GetOrderSubtotal(data);
                    for (int itemIndex = 0; itemIndex < jsonItems.Count; itemIndex++)
                    {
                        var item = jsonItems[itemIndex];
                        int qty = item.TryGetProperty("quantity", out var q) ? (q.ValueKind == JsonValueKind.Number ? q.GetInt32() : 1) : 1;
                        
                        // Parse name (nested or flat support)
                        string name = item.TryGetProperty("name", out var n) ? GetElementText(n) : "";
                        if (string.IsNullOrEmpty(name)) name = item.TryGetProperty("product_name", out var pn) ? GetElementText(pn) : "";
                        if (string.IsNullOrEmpty(name) && item.TryGetProperty("product", out var pObj) && pObj.ValueKind == JsonValueKind.Object)
                        {
                            name = pObj.TryGetProperty("name", out var pn2) ? GetElementText(pn2) : "";
                        }

                        var extras = el.ShowCustomizations
                            ? ParseItemExtras(item, data, itemIndex, jsonItems.Count, orderSubtotal)
                            : new List<string>();

                        // Parse price/total (base price when extras are listed separately)
                        string total = ResolveDisplayItemPrice(item, qty, extras, orderSubtotal, jsonItems.Count);

                        string line = $"{qty}x {name}";
                        g.DrawString(line, fnNormal, brush, margin, yOffset);

                        if (el.ShowPrice && !string.IsNullOrEmpty(total))
                        {
                            string priceText = total;
                            if (!string.IsNullOrEmpty(el.CurrencySymbol)) priceText += $" {el.CurrencySymbol}";

                            float priceW = g.MeasureString(priceText, fnNormal).Width;
                            float x = el.RightAlignPrice ? (margin + usableWidth - priceW) : (margin + g.MeasureString(line, fnNormal).Width + 10);
                            g.DrawString(priceText, fnNormal, brush, x, yOffset);
                        }

                        yOffset += 18;

                        // Customizations
                        if (el.ShowCustomizations && extras.Count > 0)
                        {
                            g.DrawString($" - Extra: {string.Join(", ", extras)}", fnNormal, Brushes.Gray, margin + 15, yOffset);
                            yOffset += 16;
                        }

                        // Notes
                        if (el.ShowNotes && item.TryGetProperty("notes", out var noteProp))
                        {
                            string note = GetElementText(noteProp);
                            if (!string.IsNullOrEmpty(note))
                            {
                                g.DrawString($" - Not: {note}", fnNormal, Brushes.Gray, margin + 15, yOffset);
                                yOffset += 16;
                            }
                        }
                    }
                }
                return yOffset;
            }

            // Normal text
            string raw = Substitute(el.Content, ctx);
            Font fn = el.Font == "B" ? fnBold : fnNormal;
            if (el.Size == "2x") fn = fnHeader;

            SizeF size = g.MeasureString(raw, fn, usableWidth);
            float drawX = margin;
            if (el.Align == "center") drawX = margin + (usableWidth - size.Width) / 2;
            else if (el.Align == "right") drawX = margin + usableWidth - size.Width;

            g.DrawString(raw, fn, brush, new RectangleF(drawX, yOffset, usableWidth, size.Height + 5));

            return yOffset + (int)size.Height + 4;
        }

        private static bool ShouldSkipElement(TemplateElement el, Dictionary<string, string> ctx)
        {
            if (el.Type != "text" || string.IsNullOrEmpty(el.Content)) return false;

            // Check if it's the customer info header
            if (el.Content.Contains("L_customer_info") || el.Content.Contains("customer_info"))
            {
                bool hasName = ctx.TryGetValue("musteri_adi", out string name) && !string.IsNullOrWhiteSpace(name);
                bool hasPhone = ctx.TryGetValue("musteri_telefon", out string phone) && !string.IsNullOrWhiteSpace(phone);
                if (!hasName && !hasPhone) return true;
            }

            // If it has no placeholders, never skip it
            if (!el.Content.Contains("{")) return false;

            // Perform dry substitute of only the data variables
            string substituted = el.Content;
            int pos = 0;
            while (true)
            {
                int start = substituted.IndexOf('{', pos);
                if (start == -1) break;
                int end = substituted.IndexOf('}', start);
                if (end == -1) break;

                string rawKey = substituted.Substring(start, end - start + 1);
                string key = substituted.Substring(start + 1, end - start - 1);

                if (!key.StartsWith("L_"))
                {
                    string val = "";
                    if (ctx.TryGetValue(key, out string v))
                    {
                        val = v ?? "";
                    }
                    substituted = substituted.Replace(rawKey, val);
                }
                else
                {
                    string val = "";
                    if (ctx.TryGetValue(key, out string v))
                    {
                        val = v ?? "";
                    }
                    substituted = substituted.Replace(rawKey, val);
                    pos = start + val.Length;
                }
            }

            // Clean up the substituted string (remove spaces, colons, dashes, etc.)
            string cleaned = substituted;
            foreach (char c in new char[] { ' ', ':', '-', '|', '/', '*', '\t', '\r', '\n' })
            {
                cleaned = cleaned.Replace(c.ToString(), "");
            }

            // If nothing is left in the string, skip it!
            return string.IsNullOrEmpty(cleaned);
        }

        private static List<TemplateElement> OptimizeSection(List<TemplateElement> section, Dictionary<string, string> ctx)
        {
            var result = new List<TemplateElement>();
            foreach (var el in section)
            {
                if (el.Type == "text" && ShouldSkipElement(el, ctx))
                {
                    continue;
                }
                result.Add(el);
            }

            // Remove consecutive or trailing separators
            var finalResult = new List<TemplateElement>();
            for (int i = 0; i < result.Count; i++)
            {
                var el = result[i];
                if (el.Type == "separator")
                {
                    if (finalResult.Count == 0) continue;
                    if (finalResult[finalResult.Count - 1].Type == "separator") continue;
                }
                finalResult.Add(el);
            }

            if (finalResult.Count > 0 && finalResult[finalResult.Count - 1].Type == "separator")
            {
                finalResult.RemoveAt(finalResult.Count - 1);
            }

            return finalResult;
        }

        private static Dictionary<string, string> BuildContext(JsonElement root)
        {
            var ctx = new Dictionary<string, string>();
            var slip = root;

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("slip_data", out var sd) && sd.ValueKind == JsonValueKind.Object)
            {
                slip = sd;
            }

            var rest = slip.ValueKind == JsonValueKind.Object && slip.TryGetProperty("restaurant_info", out var r) ? r : default;
            var ord = slip.ValueKind == JsonValueKind.Object && slip.TryGetProperty("order_info", out var o) ? o : default;
            var pay = slip.ValueKind == JsonValueKind.Object && slip.TryGetProperty("payment_info", out var p) ? p : default;
            var links = slip.ValueKind == JsonValueKind.Object && slip.TryGetProperty("links", out var l) ? l : default;
            var cancel = slip.ValueKind == JsonValueKind.Object && slip.TryGetProperty("cancel_info", out var c) ? c : default;

            // Restaurant Info fallback to ConfigManager defaults if empty or not provided
            ctx["restoran_adi"] = GetStr(rest, "name", string.IsNullOrWhiteSpace(ConfigManager.Current.App.RestaurantName) ? "CHEFFIE POS" : ConfigManager.Current.App.RestaurantName);
            ctx["restoran_adres"] = GetStr(rest, "address", ConfigManager.Current.App.RestaurantAddress);
            ctx["restoran_telefon"] = GetStr(rest, "phone", ConfigManager.Current.App.RestaurantPhone);
            ctx["restoran_vergi_no"] = GetStr(rest, "tax_id", "");
            
            // Order Info (with flat API Order fallbacks)
            string tableName = "";
            if (ord.ValueKind == JsonValueKind.Object)
            {
                tableName = GetStr(ord, "table_name", "");
            }
            else if (slip.ValueKind == JsonValueKind.Object)
            {
                if (slip.TryGetProperty("table", out var tEl))
                {
                    tableName = tEl.ValueKind == JsonValueKind.Object ? GetStr(tEl, "name", "") : (tEl.ValueKind == JsonValueKind.String ? tEl.GetString() ?? "" : "");
                }
                if (string.IsNullOrEmpty(tableName)) tableName = GetStr(slip, "table_name", "");
            }
            ctx["masa_no"] = tableName;
            ctx["masa_adi"] = tableName;

            ctx["siparis_no"] = ord.ValueKind == JsonValueKind.Object ? GetStr(ord, "order_number", "") : GetStr(slip, "order_number", "");
            ctx["garson_adi"] = ord.ValueKind == JsonValueKind.Object ? GetStr(ord, "waiter_name", "-") : GetStr(slip, "waiter", "-");

            // Date & Time parsing
            string dateStr = DateTime.Now.ToString("dd.MM.yyyy");
            string timeStr = DateTime.Now.ToString("HH:mm");
            if (ord.ValueKind == JsonValueKind.Object)
            {
                dateStr = GetStr(ord, "date", dateStr);
                timeStr = GetStr(ord, "time", timeStr);
            }
            else if (slip.ValueKind == JsonValueKind.Object)
            {
                string ts = "";
                if (slip.TryGetProperty("created_at", out var cat)) ts = cat.ValueKind == JsonValueKind.String ? cat.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(ts)) ts = GetStr(slip, "ts", "");
                if (DateTime.TryParse(ts, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    var localDt = ts.EndsWith("Z", StringComparison.OrdinalIgnoreCase) ? dt.ToLocalTime() : dt;
                    dateStr = localDt.ToString("dd.MM.yyyy");
                    timeStr = localDt.ToString("HH:mm");
                }
            }
            ctx["tarih"] = dateStr;
            ctx["saat"] = timeStr;
            
            // Payment Info (with flat API Order fallbacks)
            ctx["ara_toplam"] = pay.ValueKind == JsonValueKind.Object ? GetStr(pay, "subtotal", "0.00") : GetStr(slip, "subtotal", "0.00");
            if (string.IsNullOrEmpty(ctx["ara_toplam"])) ctx["ara_toplam"] = "0.00";
            
            ctx["ekstra_toplam"] = pay.ValueKind == JsonValueKind.Object ? GetStr(pay, "extras_total", "0.00") : GetStr(slip, "extras_total", "0.00");

            double araToplamVal = 0;
            double.TryParse(ctx["ara_toplam"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out araToplamVal);
            double ekstraToplamVal = 0;
            double.TryParse(ctx["ekstra_toplam"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out ekstraToplamVal);

            if (string.IsNullOrEmpty(ctx["ekstra_toplam"]) || ctx["ekstra_toplam"] == "0" || ctx["ekstra_toplam"] == "0.00"
                || (araToplamVal > 0 && ekstraToplamVal >= araToplamVal - 0.009))
            {
                double inferredExtras = OrderItemExtrasParser.ResolveExtrasTotal(slip);
                if (inferredExtras > 0.009)
                    ctx["ekstra_toplam"] = inferredExtras.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            }
            if (string.IsNullOrEmpty(ctx["ekstra_toplam"])) ctx["ekstra_toplam"] = "0.00";

            ctx["kdv_toplam"] = pay.ValueKind == JsonValueKind.Object ? GetStr(pay, "tax", "0.00") : GetStr(slip, "tax", "0.00");
            if (string.IsNullOrEmpty(ctx["kdv_toplam"])) ctx["kdv_toplam"] = "0.00";

            string totalVal = "0.00";
            if (pay.ValueKind == JsonValueKind.Object) totalVal = GetStr(pay, "total", "0.00");
            else if (slip.ValueKind == JsonValueKind.Object)
            {
                totalVal = GetStr(slip, "total_amount", "");
                if (string.IsNullOrEmpty(totalVal)) totalVal = GetStr(slip, "total", "0.00");
            }
            ctx["toplam_tutar"] = totalVal;

            ctx["musteri_adi"] = ord.ValueKind == JsonValueKind.Object ? GetStr(ord, "customer_name", "") : GetStr(slip, "customer_name", "");
            ctx["musteri_telefon"] = ord.ValueKind == JsonValueKind.Object ? GetStr(ord, "customer_phone", "") : GetStr(slip, "customer_phone", "");
            ctx["musteri_email"] = ord.ValueKind == JsonValueKind.Object ? GetStr(ord, "customer_email", "") : GetStr(slip, "customer_email", GetStr(slip, "email", ""));
            ctx["teslimat_adresi"] = ord.ValueKind == JsonValueKind.Object ? GetStr(ord, "delivery_address", "") : GetStr(slip, "delivery_address", "");
            ctx["odeme_tipi"] = ord.ValueKind == JsonValueKind.Object ? GetStr(ord, "payment_method", "") : GetStr(slip, "payment_method", "");
            ctx["ek_not"] = ord.ValueKind == JsonValueKind.Object ? GetStr(ord, "order_note", "") : GetStr(slip, "notes", "");

            ctx["wifi_ag_adi"] = GetStr(rest, "wifi_ssid", "");
            ctx["wifi_sifresi"] = GetStr(rest, "wifi_password", "");
            ctx["odeme_linki"] = GetStr(links, "payment_url", "");
            ctx["slip_title"] = GetStr(slip, "slip_title", "");
            ctx["siparis_durumu"] = ord.ValueKind == JsonValueKind.Object ? GetStr(ord, "status", "pending") : GetStr(slip, "status", "pending");
            
            ctx["iptal_sebebi"] = GetStr(cancel, "reason", "");
            ctx["iptal_saati"] = GetStr(cancel, "canceled_at", "");

            // Localized labels for templates
            ctx["L_adres"] = LocalizationService.T("orders.detail.address", "Adres");
            ctx["L_tel"] = LocalizationService.T("orders.detail.phone", "Tel");
            ctx["L_masa"] = LocalizationService.T("orders.detail.table", "Masa");
            ctx["L_siparis_no"] = LocalizationService.T("orders.detail.order_no", "Sipariş No");
            ctx["L_tarih"] = LocalizationService.T("orders.columns.date", "Tarih");
            ctx["L_saat"] = LocalizationService.T("designer.vars.time", "Saat");
            ctx["L_ara_toplam"] = LocalizationService.T("designer.vars.subtotal", "Ara Toplam");
            ctx["L_ekstra_toplam"] = LocalizationService.T("designer.vars.extra_total", "Ekstra Toplam");
            ctx["L_kdv"] = LocalizationService.T("designer.vars.tax_total", "KDV");
            ctx["L_total"] = LocalizationService.T("designer.vars.grand_total", "Genel Toplam");
            ctx["L_afiyet_olsun"] = LocalizationService.T("receipt.enjoy", "Afiyet Olsun!");

            ctx["L_customer_info"] = LocalizationService.T("receipt.customer_info", 
                LocalizationService.CurrentLanguage.ToLower() == "tr" ? "Müşteri Bilgileri" : "Customer Details");
            ctx["L_customer_name"] = LocalizationService.T("designer.vars.customer_name", "Müşteri Adı");
            ctx["L_customer_phone"] = LocalizationService.T("designer.vars.customer_phone", "Müşteri Telefonu");
            ctx["L_delivery_address"] = LocalizationService.T("designer.vars.delivery_address", "Teslimat Adresi");
 
            return ctx;
        }

        private static string GetStr(JsonElement parent, string propName, string fallback)
        {
            if (parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(propName, out var p))
            {
                return p.ValueKind switch
                {
                    JsonValueKind.String => p.GetString() ?? fallback,
                    JsonValueKind.Number => p.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => fallback,
                    JsonValueKind.Object => p.GetRawText(),
                    JsonValueKind.Array => p.GetRawText(),
                    _ => fallback
                };
            }
            return fallback;
        }

        public static string Substitute(string content, Dictionary<string, string> ctx)
        {
            if (string.IsNullOrEmpty(content)) return "";
            string output = content;
            foreach (var kvp in ctx)
            {
                output = output.Replace("{" + kvp.Key + "}", kvp.Value);
            }
            return output;
        }
        private static void WriteQrEscPos(BinaryWriter writer, string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            byte[] dataBytes = Encoding.UTF8.GetBytes(content);
            int len = dataBytes.Length + 3;
            byte pL = (byte)(len % 256);
            byte pH = (byte)(len / 256);

            // 1. Set QR Code Model (Model 2)
            writer.Write(new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00 });

            // 2. Set QR Code Size (Module Width = 4)
            writer.Write(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x04 });

            // 3. Set Error Correction Level (L = 7%)
            writer.Write(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x44, 0x30 });

            // 4. Store QR Code Data
            writer.Write(new byte[] { 0x1D, 0x28, 0x6B, pL, pH, 0x31, 0x50, 0x30 });
            writer.Write(dataBytes);

            // 5. Print QR Code
            writer.Write(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 });

            // 6. Line feed to prevent overlapping with next text
            writer.Write(new byte[] { 0x0A });
        }

        private static List<string> ParseItemExtras(JsonElement item, JsonElement slip, int itemIndex, int itemCount, double orderSubtotal)
        {
            var context = new OrderItemParseContext
            {
                OrderSubtotal = orderSubtotal,
                ItemCount = itemCount,
                ItemIndex = itemIndex
            };
            return OrderItemExtrasParser.ParseAddedExtras(item, context);
        }

        private static double GetOrderSubtotal(JsonElement slip)
        {
            if (slip.TryGetProperty("payment_info", out var pay) && pay.ValueKind == JsonValueKind.Object)
            {
                string sub = GetStr(pay, "subtotal", "");
                if (double.TryParse(sub, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val) && val > 0)
                    return val;
            }

            string direct = GetStr(slip, "subtotal", "");
            if (double.TryParse(direct, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double directVal) && directVal > 0)
                return directVal;

            return 0;
        }

        private static string ResolveDisplayItemPrice(JsonElement item, int qty, List<string> extras, double orderSubtotal, int itemCount)
        {
            if (extras.Count > 0)
            {
                if (item.TryGetProperty("base_price", out var basePriceEl))
                {
                    string baseText = GetElementText(basePriceEl);
                    if (double.TryParse(baseText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double baseUnit) && baseUnit > 0)
                        return (baseUnit * qty).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (item.TryGetProperty("product", out var product) && product.ValueKind == JsonValueKind.Object &&
                    product.TryGetProperty("price", out var basePriceFromProduct))
                {
                    string baseText = GetElementText(basePriceFromProduct);
                    if (double.TryParse(baseText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double baseUnit) && baseUnit > 0)
                        return (baseUnit * qty).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                }

                double lineTotal = 0;
                if (item.TryGetProperty("line_total", out var lt))
                    double.TryParse(GetElementText(lt), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lineTotal);
                else if (item.TryGetProperty("total_price", out var tp))
                    double.TryParse(GetElementText(tp), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lineTotal);

                double extrasAmount = OrderItemExtrasParser.SumPricesFromExtraLabels(extras) * qty;
                if (lineTotal > extrasAmount + 0.009)
                    return (lineTotal - extrasAmount).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            }

            if (item.TryGetProperty("line_total", out var lineTotalEl)) return GetElementText(lineTotalEl);
            if (item.TryGetProperty("total_price", out var totalPriceEl)) return GetElementText(totalPriceEl);
            if (item.TryGetProperty("subtotal", out var subtotalEl)) return GetElementText(subtotalEl);
            if (item.TryGetProperty("unit_price", out var unitPriceEl)) return GetElementText(unitPriceEl);
            if (item.TryGetProperty("price", out var priceEl)) return GetElementText(priceEl);
            if (itemCount == 1 && orderSubtotal > 0) return orderSubtotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            return "";
        }

        private static string GetJsonStr(JsonElement el, string propName)
        {
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(propName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.String) return prop.GetString() ?? "";
                if (prop.ValueKind == JsonValueKind.Number) return prop.GetRawText();
                if (prop.ValueKind == JsonValueKind.True) return "true";
                if (prop.ValueKind == JsonValueKind.False) return "false";
            }
            return "";
        }

        private static string GetElementText(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String) return element.GetString() ?? "";
            if (element.ValueKind == JsonValueKind.Number) return element.GetRawText();
            if (element.ValueKind != JsonValueKind.Object) return "";

            string language = ConfigManager.Current.App.Language;
            if (element.TryGetProperty(language, out var localized)) return GetElementText(localized);
            if (element.TryGetProperty("tr", out var turkish)) return GetElementText(turkish);
            if (element.TryGetProperty("en", out var english)) return GetElementText(english);
            if (element.TryGetProperty("name", out var name)) return GetElementText(name);
            return "";
        }

        public static Dictionary<string, string> GetMockContext()
        {
            return BuildContext(GetMockOrderData());
        }

        public static JsonElement GetMockOrderData()
        {
            var mockObj = new
            {
                restaurant_info = new
                {
                    name = string.IsNullOrWhiteSpace(ConfigManager.Current.App.RestaurantName) ? "Cheffie Restaurant" : ConfigManager.Current.App.RestaurantName,
                    address = string.IsNullOrWhiteSpace(ConfigManager.Current.App.RestaurantAddress) ? "Atatürk Mah. No:123, İzmir" : ConfigManager.Current.App.RestaurantAddress,
                    phone = string.IsNullOrWhiteSpace(ConfigManager.Current.App.RestaurantPhone) ? "+90 232 555 1234" : ConfigManager.Current.App.RestaurantPhone,
                    tax_id = "9876543210",
                    wifi_ssid = "Cheffie_Guest",
                    wifi_password = "cheffiekey123"
                },
                order_info = new
                {
                    order_number = "ORD-12345",
                    table_name = "Masa 5",
                    waiter_name = "Ahmet",
                    date = DateTime.Now.ToString("dd.MM.yyyy"),
                    time = DateTime.Now.ToString("HH:mm"),
                    customer_name = "Cengiz Kağan",
                    customer_phone = "+90 555 444 3322",
                    delivery_address = "Cumhuriyet Cad. Hürriyet Apt. No:12 D:4, Alsancak",
                    payment_method = "Kredi Kartı",
                    order_note = "Soslar bol olsun lütfen, temassız teslimat."
                },
                payment_info = new
                {
                    subtotal = "210.00",
                    extras_total = "30.00",
                    tax = "24.00",
                    total = "240.00"
                },
                items = new[]
                {
                    new
                    {
                        quantity = 1,
                        name = "Özel Soslu Hamburger",
                        line_total = "140.00",
                        price = 140.00,
                        customizations = new { added = new[] { "Ekstra Peynir", "Karamelize Soğan" } },
                        notes = "Köfte orta pişmiş olsun."
                    },
                    new
                    {
                        quantity = 2,
                        name = "Çıtır Patates",
                        line_total = "70.00",
                        price = 35.00,
                        customizations = new { added = new string[0] },
                        notes = ""
                    },
                    new
                    {
                        quantity = 1,
                        name = "Kola Zero",
                        line_total = "30.00",
                        price = 30.00,
                        customizations = new { added = new string[0] },
                        notes = ""
                    }
                },
                links = new
                {
                    payment_url = "https://pay.nlkmenu.com/ord-12345"
                }
            };

            string json = JsonSerializer.Serialize(mockObj);
            return JsonDocument.Parse(json).RootElement.Clone();
        }

        public static byte[] RenderBitmapToEscPos(Bitmap bitmap)
        {
            int widthBytes = (bitmap.Width + 7) / 8;
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(new byte[] { 0x1B, 0x40, 0x1B, 0x33, 0x00 });
            writer.Write(new byte[]
            {
                0x1D, 0x76, 0x30, 0x00,
                (byte)(widthBytes & 0xFF), (byte)(widthBytes >> 8),
                (byte)(bitmap.Height & 0xFF), (byte)(bitmap.Height >> 8)
            });

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int xByte = 0; xByte < widthBytes; xByte++)
                {
                    byte value = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int x = xByte * 8 + bit;
                        if (x >= bitmap.Width) continue;

                        Color pixel = bitmap.GetPixel(x, y);
                        if (ShouldPrintPixel(pixel)) value |= (byte)(0x80 >> bit);
                    }
                    writer.Write(value);
                }
            }

            writer.Write(new byte[] { 0x0A, 0x0A, 0x0A, 0x1D, 0x56, 0x41, 0x03 });
            return stream.ToArray();
        }

        public static byte[] RenderBitmapToEscPosBytes(Bitmap bitmap)
        {
            int widthBytes = (bitmap.Width + 7) / 8;
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(new byte[]
            {
                0x1D, 0x76, 0x30, 0x00,
                (byte)(widthBytes & 0xFF), (byte)(widthBytes >> 8),
                (byte)(bitmap.Height & 0xFF), (byte)(bitmap.Height >> 8)
            });

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int xByte = 0; xByte < widthBytes; xByte++)
                {
                    byte value = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int x = xByte * 8 + bit;
                        if (x >= bitmap.Width) continue;

                        Color pixel = bitmap.GetPixel(x, y);
                        if (ShouldPrintPixel(pixel)) value |= (byte)(0x80 >> bit);
                    }
                    writer.Write(value);
                }
            }

            writer.Write(new byte[] { 0x0A }); // Feed single line after the logo
            return stream.ToArray();
        }

        private static bool ShouldPrintPixel(Color pixel)
        {
            if (pixel.A <= 50)
            {
                return false;
            }

            int luminance = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
            int chroma = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B)) - Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));

            // Thermal printers are monochrome. A strict luminance-only threshold loses
            // bright saturated colours (for example the cyan circle in the NLK logo).
            return luminance < 200 || (chroma >= 40 && luminance < 245);
        }

        public static Bitmap RenderCode128Barcode(string content)
        {
            var bmp = new Bitmap(300, 60);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                
                // Draw barcode-like lines dynamically based on character hash
                int x = 10;
                var rand = new Random(content.GetHashCode());
                while (x < 290)
                {
                    int w = rand.Next(1, 4);
                    int space = rand.Next(1, 4);
                    g.FillRectangle(Brushes.Black, x, 5, w, 50);
                    x += w + space;
                }
            }
            return bmp;
        }
    }
}
