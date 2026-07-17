using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Nlk_Cheffie_Print.Core
{
    public class AppConfigData
    {
        private string _deviceToken = "";
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

        [JsonIgnore]
        public string DeviceToken
        {
            get => _deviceToken;
            set => _deviceToken = value ?? "";
        }

        // Legacy plain-text value is accepted when reading an existing configuration,
        // but never written again. New installations use the current Windows user's DPAPI key.
        [JsonPropertyName("device_token")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LegacyDeviceToken
        {
            get => null;
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrEmpty(_deviceToken))
                {
                    _deviceToken = value;
                }
            }
        }

        [JsonPropertyName("device_token_protected")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ProtectedDeviceToken
        {
            get => ProtectToken(_deviceToken);
            set
            {
                string? token = UnprotectToken(value);
                if (!string.IsNullOrEmpty(token)) _deviceToken = token;
            }
        }

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

        private static string? ProtectToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            try
            {
                byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(token), null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        private static string? UnprotectToken(string? protectedToken)
        {
            if (string.IsNullOrWhiteSpace(protectedToken)) return null;
            try
            {
                byte[] encrypted = Convert.FromBase64String(protectedToken);
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser));
            }
            catch (Exception ex) when (ex is CryptographicException or FormatException)
            {
                return null;
            }
        }
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

        // Empty values are resolved from the printer name for backward compatibility.
        [JsonPropertyName("profile")]
        public string Profile { get; set; } = "";
        
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
        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NlkCheffiePrint");
        private static readonly string ConfigPath = Path.Combine(ConfigDirectory, "config.json");
        private static readonly string LegacyConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        public static RootConfig Current { get; private set; } = new RootConfig();

        public static bool IsSecureApiUrl(string? value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && uri.Scheme == Uri.UriSchemeHttps
                && !string.IsNullOrWhiteSpace(uri.Host);
        }

        static ConfigManager()
        {
            Load();
        }

        public static void Load()
        {
            try
            {
                string sourcePath = File.Exists(ConfigPath) ? ConfigPath : LegacyConfigPath;
                if (File.Exists(sourcePath))
                {
                    string json = File.ReadAllText(sourcePath);
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
                Directory.CreateDirectory(ConfigDirectory);
                if (!string.IsNullOrEmpty(Current.App.DeviceToken) && Current.App.ProtectedDeviceToken == null)
                {
                    throw new CryptographicException("Cihaz tokeni Windows kullanıcı korumasıyla saklanamadı.");
                }
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Current, options);
                string temporaryPath = ConfigPath + ".tmp";
                File.WriteAllText(temporaryPath, json, Encoding.UTF8);
                File.Move(temporaryPath, ConfigPath, true);

                if (File.Exists(LegacyConfigPath) && !string.Equals(LegacyConfigPath, ConfigPath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(LegacyConfigPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}
