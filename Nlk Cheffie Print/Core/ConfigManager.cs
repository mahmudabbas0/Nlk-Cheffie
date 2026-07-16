using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Nlk_Cheffie_Print.Core
{
    public class AppConfigData
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "dark";

        [JsonPropertyName("dry_run")]
        public bool DryRun { get; set; } = true;

        [JsonPropertyName("graphic_mode")]
        public bool GraphicMode { get; set; } = false;

        [JsonPropertyName("auto_print_enabled")]
        public bool AutoPrintEnabled { get; set; } = false;

        [JsonPropertyName("auto_print_role")]
        public string AutoPrintRole { get; set; } = "kitchen";

        [JsonPropertyName("language")]
        public string Language { get; set; } = "tr";

        [JsonPropertyName("device_token")]
        public string DeviceToken { get; set; } = "";

        [JsonPropertyName("settings_password")]
        public string SettingsPassword { get; set; } = "";

        [JsonPropertyName("api_base_url")]
        public string ApiBaseUrl { get; set; } = "https://api.nlkmenu.com/api";

        [JsonPropertyName("restaurant_id")]
        public string RestaurantId { get; set; } = "";

        [JsonPropertyName("restaurant_slug")]
        public string RestaurantSlug { get; set; } = "";

        [JsonPropertyName("restaurant_name")]
        public string RestaurantName { get; set; } = "";

        [JsonPropertyName("restaurant_address")]
        public string RestaurantAddress { get; set; } = "";

        [JsonPropertyName("restaurant_phone")]
        public string RestaurantPhone { get; set; } = "";

        [JsonPropertyName("codepage")]
        public int CodePage { get; set; } = 61;

        [JsonPropertyName("encoding_name")]
        public string EncodingName { get; set; } = "ibm857";
    }

    public class PrinterInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "usb"; // usb or network

        [JsonPropertyName("id")]
        public string Id { get; set; } = ""; // USB path or Network IP

        [JsonPropertyName("port")]
        public int Port { get; set; } = 9100;
        
        [JsonPropertyName("label")]
        public string Label => Type == "network" ? $"{Name} ({Id})" : Name;
    }

    public class PrintersConfigData
    {
        [JsonPropertyName("kitchen")]
        public string Kitchen { get; set; } = "";

        [JsonPropertyName("cashier")]
        public string Cashier { get; set; } = "";

        [JsonPropertyName("courier")]
        public string Courier { get; set; } = "";

        [JsonPropertyName("available")]
        public List<PrinterInfo> Available { get; set; } = new List<PrinterInfo>();
    }

    public class RootConfig
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "dark";

        [JsonPropertyName("app")]
        public AppConfigData App { get; set; } = new AppConfigData();

        [JsonPropertyName("printers")]
        public PrintersConfigData Printers { get; set; } = new PrintersConfigData();
    }

    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        public static RootConfig Current { get; private set; } = new RootConfig();

        static ConfigManager()
        {
            Load();
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var loaded = JsonSerializer.Deserialize<RootConfig>(json);
                    if (loaded != null)
                    {
                        Current = loaded;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
            }
            
            Current = new RootConfig();
        }

        public static void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Current, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}
