using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nlk_Cheffie_Print.Models
{
    public class TemplateElement
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text"; // text, separator, qrcode, logo, barcode, items

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";

        [JsonPropertyName("align")]
        public string Align { get; set; } = "left"; // left, center, right

        [JsonPropertyName("font")]
        public string Font { get; set; } = "A"; // A (normal), B (bold)

        [JsonPropertyName("size")]
        public string Size { get; set; } = "1x"; // 1x, 2x

        [JsonPropertyName("family")]
        public string Family { get; set; } = "default"; // default, arial, mono

        [JsonPropertyName("path")]
        public string Path { get; set; } = ""; // logo path

        [JsonPropertyName("show_price")]
        public bool ShowPrice { get; set; } = true;

        [JsonPropertyName("right_align_price")]
        public bool RightAlignPrice { get; set; } = false;

        [JsonPropertyName("currency_symbol")]
        public string CurrencySymbol { get; set; } = "";

        [JsonPropertyName("show_customizations")]
        public bool ShowCustomizations { get; set; } = true;

        [JsonPropertyName("show_notes")]
        public bool ShowNotes { get; set; } = true;

        [JsonPropertyName("show_tax")]
        public bool ShowTax { get; set; } = true;
    }

    public class SlipTemplate
    {
        [JsonPropertyName("header")]
        public List<TemplateElement> Header { get; set; } = new List<TemplateElement>();

        [JsonPropertyName("body")]
        public List<TemplateElement> Body { get; set; } = new List<TemplateElement>();

        [JsonPropertyName("footer")]
        public List<TemplateElement> Footer { get; set; } = new List<TemplateElement>();
    }
}
