using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Core.Printer
{
    public static class TemplateStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static SlipTemplate Load(string role)
        {
            SlipTemplate template;
            string path = GetPath(role);
            if (!File.Exists(path))
            {
                template = GetDefaultTemplate(role);
                Save(role, template);
            }
            else
            {
                try
                {
                    var t = JsonSerializer.Deserialize<SlipTemplate>(File.ReadAllText(path));
                    if (t != null)
                    {
                        Normalize(t);
                        template = t;
                    }
                    else
                    {
                        template = GetDefaultTemplate(role);
                    }
                }
                catch
                {
                    template = GetDefaultTemplate(role);
                }
            }

            // Smart migration / auto-fix:
            // If the template does not contain Ara Toplam or Genel Toplam in its footer,
            // automatically append the modern totals to make sure they are always printed!
            bool hasTotals = false;
            if (template.Footer != null)
            {
                foreach (var el in template.Footer)
                {
                    if (el.Content != null && (el.Content.Contains("{toplam_tutar}") || el.Content.Contains("{genel_toplam}") || el.Content.Contains("{ara_toplam}")))
                    {
                        hasTotals = true;
                        break;
                    }
                }
            }
            if (!hasTotals)
            {
                var cleanFooter = new List<TemplateElement>();
                if (template.Footer != null)
                {
                    foreach (var el in template.Footer)
                    {
                        if (el.Type == "separator" || (el.Content != null && (el.Content.Contains("Afiyet") || el.Content.Contains("Powered"))))
                            continue;
                        cleanFooter.Add(el);
                    }
                }
                
                cleanFooter.Add(new TemplateElement { Type = "separator" });
                cleanFooter.Add(new TemplateElement { Type = "text", Content = "Ara Toplam: {ara_toplam} TL", Align = "left" });
                cleanFooter.Add(new TemplateElement { Type = "text", Content = "Ekstra Toplam: {ekstra_toplam} TL", Align = "left" });
                cleanFooter.Add(new TemplateElement { Type = "text", Content = "KDV: {kdv_toplam} TL", Align = "left" });
                cleanFooter.Add(new TemplateElement { Type = "text", Content = "Genel Toplam: {toplam_tutar} TL", Font = "B", Align = "left" });
                cleanFooter.Add(new TemplateElement { Type = "separator" });
                cleanFooter.Add(new TemplateElement { Type = "text", Content = "Afiyet Olsun!", Align = "center" });
                cleanFooter.Add(new TemplateElement { Type = "separator" });
                cleanFooter.Add(new TemplateElement { Type = "text", Content = "Powered by NlkCheffie", Align = "center" });

                template.Footer = cleanFooter;
                Save(role, template); // Save the upgraded template back to disk
            }

            return template;
        }

        public static void Save(string role, SlipTemplate template)
        {
            Normalize(template);

            string path = GetPath(role);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(template, JsonOptions));
        }

        public static SlipTemplate GetDefaultTemplate(string role)
        {
            var template = new SlipTemplate();
            
            // Rich default template matching modern restaurant receipts
            template.Header.Add(new TemplateElement { Type = "logo", Align = "center" });
            template.Header.Add(new TemplateElement { Type = "text", Content = "{restoran_adi}", Font = "B", Align = "center" });
            template.Header.Add(new TemplateElement { Type = "text", Content = "{restoran_adres}", Align = "center" });
            template.Header.Add(new TemplateElement { Type = "text", Content = "{restoran_telefon}", Align = "center" });
            template.Header.Add(new TemplateElement { Type = "separator" });

            template.Header.Add(new TemplateElement { Type = "text", Content = "Masa: {masa_adi}", Font = "B", Align = "left" });
            template.Header.Add(new TemplateElement { Type = "text", Content = "Sipariş No: #{siparis_no}", Align = "left" });
            template.Header.Add(new TemplateElement { Type = "text", Content = "Tarih: {tarih}   Saat: {saat}", Align = "left" });
            template.Header.Add(new TemplateElement { Type = "separator" });

            template.Body.Add(new TemplateElement { Type = "items", Align = "left", ShowPrice = true, ShowCustomizations = true, ShowNotes = true });

            template.Footer.Add(new TemplateElement { Type = "separator" });
            template.Footer.Add(new TemplateElement { Type = "text", Content = "Ara Toplam: {ara_toplam} TL", Align = "left" });
            template.Footer.Add(new TemplateElement { Type = "text", Content = "Ekstra Toplam: {ekstra_toplam} TL", Align = "left" });
            template.Footer.Add(new TemplateElement { Type = "text", Content = "KDV: {kdv_toplam} TL", Align = "left" });
            template.Footer.Add(new TemplateElement { Type = "text", Content = "Genel Toplam: {toplam_tutar} TL", Font = "B", Align = "left" });
            template.Footer.Add(new TemplateElement { Type = "separator" });
            template.Footer.Add(new TemplateElement { Type = "text", Content = "Afiyet Olsun!", Align = "center" });
            template.Footer.Add(new TemplateElement { Type = "separator" });
            template.Footer.Add(new TemplateElement { Type = "text", Content = "Powered by NlkCheffie", Align = "center" });

            return template;
        }

        private static string GetPath(string role)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string slug = ConfigManager.Current.App.RestaurantSlug;
            string directory = string.IsNullOrWhiteSpace(slug)
                ? Path.Combine(appData, "nlkCheffiePrint", "config")
                : Path.Combine(appData, "nlkCheffiePrint", "profiles", slug);

            return Path.Combine(directory, $"{role}_template.json");
        }

        private static void Normalize(SlipTemplate template)
        {
            NormalizeSection(template.Header);
            NormalizeSection(template.Body);
            NormalizeSection(template.Footer);
        }

        private static void NormalizeSection(List<TemplateElement> elements)
        {
            foreach (var element in elements)
            {
                if (element.Type.Equals("table", StringComparison.OrdinalIgnoreCase)) element.Type = "items";

                element.Align = element.Align.ToUpperInvariant() switch
                {
                    "C" => "center",
                    "R" => "right",
                    "L" => "left",
                    _ => element.Align
                };

                if (element.Size.Equals("double_width", StringComparison.OrdinalIgnoreCase) ||
                    element.Size.Equals("double_height", StringComparison.OrdinalIgnoreCase))
                {
                    element.Size = "2x";
                }
                else if (element.Size.Equals("normal", StringComparison.OrdinalIgnoreCase))
                {
                    element.Size = "1x";
                }
            }
        }
    }
}
