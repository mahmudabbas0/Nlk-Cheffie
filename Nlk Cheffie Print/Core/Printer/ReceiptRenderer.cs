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
        private const int PageWidthChars = 32;

        public static byte[] RenderToEscPos(SlipTemplate template, JsonElement data)
        {
            var ctx = BuildContext(data);
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            // Initialize printer
            writer.Write(new byte[] { 0x1B, 0x40 });

            // Set codepage to CP857 (Turkish)
            writer.Write(new byte[] { 0x1B, 0x74, 61 });

            // Render Header
            RenderSectionEscPos(template.Header, ctx, writer, data);

            // Render Body
            RenderSectionEscPos(template.Body, ctx, writer, data);

            // Render Footer
            RenderSectionEscPos(template.Footer, ctx, writer, data);

            // Feed paper and cut
            writer.Write(new byte[] { 0x0A, 0x0A, 0x0A, 0x0A }); // 4 line feeds
            writer.Write(new byte[] { 0x1D, 0x56, 0x41, 0x03 }); // Cut command

            return ms.ToArray();
        }

        private static void RenderSectionEscPos(List<TemplateElement> section, Dictionary<string, string> ctx, BinaryWriter writer, JsonElement data)
        {
            foreach (var el in section)
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
                    WriteTextEscPos(writer, "[LOGO]\n", "center", "A", "1x");
                }
            }
        }

        private static void WriteTextEscPos(BinaryWriter writer, string text, string align, string font, string size)
        {
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

            // Encode to Turkish codepage CP857
            byte[] bytes = Encoding.GetEncoding("ibm857").GetBytes(text);
            writer.Write(bytes);
        }

        private static void RenderItemsEscPos(TemplateElement el, JsonElement data, BinaryWriter writer)
        {
            // Check order status for cancellation
            string status = data.TryGetProperty("order_info", out var oi) && oi.TryGetProperty("status", out var st) ? st.GetString() ?? "" : "";
            if (status.ToLower() == "canceled")
            {
                WriteTextEscPos(writer, "*** SIPARIS IPTAL EDILDI ***\n", "center", "B", "2x");
                WriteTextEscPos(writer, "--------------------------------\n", "left", "A", "1x");
            }

            if (data.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itemsProp.EnumerateArray())
                {
                    int qty = item.TryGetProperty("quantity", out var q) ? (q.ValueKind == JsonValueKind.Number ? q.GetInt32() : 1) : 1;
                    string name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                    string total = item.TryGetProperty("line_total", out var lt) ? lt.GetString() ?? "" : "";

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
                    if (el.ShowCustomizations)
                    {
                        var extras = ParseItemExtras(item);
                        if (extras.Count > 0)
                        {
                            WriteTextEscPos(writer, $" - Extra: {string.Join(", ", extras)}\n", "left", "A", "1x");
                        }
                    }

                    // Notes
                    if (el.ShowNotes && item.TryGetProperty("notes", out var noteProp))
                    {
                        string note = noteProp.GetString() ?? "";
                        if (!string.IsNullOrEmpty(note))
                        {
                            WriteTextEscPos(writer, $" - Not: {note}\n", "left", "A", "1x");
                        }
                    }
                }
            }
        }

        public static Bitmap RenderToBitmap(SlipTemplate template, JsonElement data, int widthPx = 550)
        {
            // Render receipt to dynamic heights using system drawing
            var ctx = BuildContext(data);

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
                    yOffset = MeasureSection(template.Header, ctx, g, yOffset, usableWidth, fnNormal, fnBold, fnHeader, data);
                    yOffset = MeasureSection(template.Body, ctx, g, yOffset, usableWidth, fnNormal, fnBold, fnHeader, data);
                    yOffset = MeasureSection(template.Footer, ctx, g, yOffset, usableWidth, fnNormal, fnBold, fnHeader, data);

                    // Create final sized bitmap
                    var finalBmp = new Bitmap(widthPx, yOffset + 30);
                    using (var finalG = Graphics.FromImage(finalBmp))
                    {
                        finalG.Clear(Color.White);
                        int drawY = 10;
                        drawY = DrawSection(template.Header, ctx, finalG, drawY, usableWidth, margin, fnNormal, fnBold, fnHeader, brush, data);
                        drawY = DrawSection(template.Body, ctx, finalG, drawY, usableWidth, margin, fnNormal, fnBold, fnHeader, brush, data);
                        drawY = DrawSection(template.Footer, ctx, finalG, drawY, usableWidth, margin, fnNormal, fnBold, fnHeader, brush, data);
                    }
                    return finalBmp;
                }
            }
        }

        private static int MeasureSection(List<TemplateElement> section, Dictionary<string, string> ctx, Graphics g, int yOffset, int usableWidth, Font fnNormal, Font fnBold, Font fnHeader, JsonElement data)
        {
            foreach (var el in section)
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
                // Measure each items list
                if (data.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in itemsProp.EnumerateArray())
                    {
                        yOffset += 18; // main item row
                        if (el.ShowCustomizations && ParseItemExtras(item).Count > 0)
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
            foreach (var el in section)
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
                string status = data.TryGetProperty("order_info", out var oi) && oi.TryGetProperty("status", out var st) ? st.GetString() ?? "" : "";
                if (status.ToLower() == "canceled")
                {
                    g.DrawString("*** SIPARIS IPTAL EDILDI ***", fnHeader, Brushes.Red, margin + (usableWidth - g.MeasureString("*** SIPARIS IPTAL EDILDI ***", fnHeader).Width) / 2, yOffset);
                    yOffset += 24;
                    g.DrawLine(Pens.Black, margin, yOffset + 4, margin + usableWidth, yOffset + 4);
                    yOffset += 12;
                }

                if (data.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in itemsProp.EnumerateArray())
                    {
                        int qty = item.TryGetProperty("quantity", out var q) ? (q.ValueKind == JsonValueKind.Number ? q.GetInt32() : 1) : 1;
                        string name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                        string total = item.TryGetProperty("line_total", out var lt) ? lt.GetString() ?? "" : "";

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
                        if (el.ShowCustomizations)
                        {
                            var extras = ParseItemExtras(item);
                            if (extras.Count > 0)
                            {
                                g.DrawString($" - Extra: {string.Join(", ", extras)}", fnNormal, Brushes.Gray, margin + 15, yOffset);
                                yOffset += 16;
                            }
                        }

                        // Notes
                        if (el.ShowNotes && item.TryGetProperty("notes", out var noteProp))
                        {
                            string note = noteProp.GetString() ?? "";
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

        private static Dictionary<string, string> BuildContext(JsonElement root)
        {
            var ctx = new Dictionary<string, string>();
            var slip = root;

            if (root.TryGetProperty("slip_data", out var sd) && sd.ValueKind == JsonValueKind.Object)
            {
                slip = sd;
            }

            var rest = slip.TryGetProperty("restaurant_info", out var r) ? r : default;
            var ord = slip.TryGetProperty("order_info", out var o) ? o : default;
            var pay = slip.TryGetProperty("payment_info", out var p) ? p : default;
            var links = slip.TryGetProperty("links", out var l) ? l : default;
            var cancel = slip.TryGetProperty("cancel_info", out var c) ? c : default;

            ctx["restoran_adi"] = GetStr(rest, "name", "CHEFFIE POS");
            ctx["restoran_adres"] = GetStr(rest, "address", "");
            ctx["restoran_telefon"] = GetStr(rest, "phone", "");
            ctx["restoran_vergi_no"] = GetStr(rest, "tax_id", "");
            
            ctx["masa_no"] = GetStr(ord, "table_name", "");
            ctx["masa_adi"] = GetStr(ord, "table_name", "");
            ctx["siparis_no"] = GetStr(ord, "order_number", "");
            ctx["garson_adi"] = GetStr(ord, "waiter_name", "-");
            ctx["tarih"] = GetStr(ord, "date", DateTime.Now.ToString("dd.MM.yyyy"));
            ctx["saat"] = GetStr(ord, "time", DateTime.Now.ToString("HH:mm"));
            
            ctx["ara_toplam"] = GetStr(pay, "subtotal", "0.00");
            ctx["ekstra_toplam"] = GetStr(pay, "extras_total", "0.00");
            ctx["kdv_toplam"] = GetStr(pay, "tax", "0.00");
            ctx["toplam_tutar"] = GetStr(pay, "total", "0.00");

            ctx["musteri_adi"] = GetStr(ord, "customer_name", "");
            ctx["musteri_telefon"] = GetStr(ord, "customer_phone", "");
            ctx["musteri_email"] = GetStr(ord, "customer_email", "");
            ctx["teslimat_adresi"] = GetStr(ord, "delivery_address", "");
            ctx["odeme_tipi"] = GetStr(ord, "payment_method", "");
            ctx["ek_not"] = GetStr(ord, "order_note", "");

            ctx["wifi_ag_adi"] = GetStr(rest, "wifi_ssid", "");
            ctx["wifi_sifresi"] = GetStr(rest, "wifi_password", "");
            ctx["odeme_linki"] = GetStr(links, "payment_url", "");
            ctx["slip_title"] = GetStr(slip, "slip_title", "");
            ctx["siparis_durumu"] = GetStr(ord, "status", "pending");
            
            ctx["iptal_sebebi"] = GetStr(cancel, "reason", "");
            ctx["iptal_saati"] = GetStr(cancel, "canceled_at", "");

            return ctx;
        }

        private static string GetStr(JsonElement parent, string propName, string fallback)
        {
            if (parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(propName, out var p))
            {
                if (p.ValueKind == JsonValueKind.Number) return p.GetDouble().ToString();
                return p.GetString() ?? fallback;
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

        private static List<string> ParseItemExtras(JsonElement item)
        {
            var extras = new List<string>();

            // 1. Try Laravel "extras" array relation
            if (item.TryGetProperty("extras", out var extrasEl) && extrasEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var ex in extrasEl.EnumerateArray())
                {
                    string exName = GetJsonStr(ex, "name");
                    if (string.IsNullOrEmpty(exName)) exName = GetJsonStr(ex, "option_name");

                    string exPrice = GetJsonStr(ex, "price");
                    if (double.TryParse(exPrice, out double exPriceVal) && exPriceVal > 0)
                    {
                        exName += $" (+{exPriceVal:0.00} TL)";
                    }
                    if (!string.IsNullOrEmpty(exName) && !extras.Contains(exName))
                    {
                        extras.Add(exName);
                    }
                }
            }

            // 2. Try standard customizations
            if (item.TryGetProperty("customizations", out var custProp))
            {
                if (custProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var c in custProp.EnumerateArray())
                    {
                        string cName = c.ValueKind == JsonValueKind.String ? c.GetString() ?? "" : GetJsonStr(c, "name");
                        if (c.ValueKind == JsonValueKind.Object)
                        {
                            string cPrice = GetJsonStr(c, "price");
                            if (double.TryParse(cPrice, out double cPriceVal) && cPriceVal > 0)
                            {
                                cName += $" (+{cPriceVal:0.00} TL)";
                            }
                        }
                        if (!string.IsNullOrEmpty(cName) && !extras.Contains(cName))
                        {
                            extras.Add(cName);
                        }
                    }
                }
                else if (custProp.ValueKind == JsonValueKind.Object && custProp.TryGetProperty("added", out var addProp) && addProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var ex in addProp.EnumerateArray())
                    {
                        string cName = ex.ValueKind == JsonValueKind.String ? ex.GetString() ?? "" : GetJsonStr(ex, "name");
                        if (ex.ValueKind == JsonValueKind.Object)
                        {
                            string cPrice = GetJsonStr(ex, "price");
                            if (double.TryParse(cPrice, out double cPriceVal) && cPriceVal > 0)
                            {
                                cName += $" (+{cPriceVal:0.00} TL)";
                            }
                        }
                        if (!string.IsNullOrEmpty(cName) && !extras.Contains(cName))
                        {
                            extras.Add(cName);
                        }
                    }
                }
            }

            return extras;
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
                        int luminance = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                        if (pixel.A > 50 && luminance < 128) value |= (byte)(0x80 >> bit);
                    }
                    writer.Write(value);
                }
            }

            writer.Write(new byte[] { 0x0A, 0x0A, 0x0A, 0x1D, 0x56, 0x41, 0x03 });
            return stream.ToArray();
        }
    }
}
