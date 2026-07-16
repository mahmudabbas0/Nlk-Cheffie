using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Nlk_Cheffie_Print.Core
{
    public static class LocalizationService
    {
        public static event Action? LanguageChanged;

        private static string _currentLanguage = "tr";
        private static Dictionary<string, object> _translations = new Dictionary<string, object>();

        public static string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    LoadTranslations(value);
                    LanguageChanged?.Invoke();
                }
            }
        }

        static LocalizationService()
        {
            // Initial load of the configured language
            string defaultLang = ConfigManager.Current?.App?.Language ?? "tr";
            _currentLanguage = defaultLang;
            LoadTranslations(defaultLang);
        }

        private static Dictionary<string, object> _fallbackTranslations = new Dictionary<string, object>();

        private static void LoadFallbackTranslations()
        {
            try
            {
                string localePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Locales", "tr.json");
                if (File.Exists(localePath))
                {
                    string json = File.ReadAllText(localePath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (dict != null)
                    {
                        _fallbackTranslations = dict;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load fallback translations: {ex.Message}");
            }
        }

        private static void LoadTranslations(string langCode)
        {
            try
            {
                // Locales folder next to the app executable
                string localePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Locales", $"{langCode}.json");
                
                if (!File.Exists(localePath))
                {
                    // Fallback to English if file doesn't exist
                    localePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Locales", "en.json");
                }

                if (File.Exists(localePath))
                {
                    string json = File.ReadAllText(localePath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (dict != null)
                    {
                        _translations = dict;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load translations for {langCode}: {ex.Message}");
            }

            _translations = new Dictionary<string, object>();
        }

        private static string ResolveKey(Dictionary<string, object> translations, string key)
        {
            try
            {
                string[] parts = key.Split('.');
                object current = translations;

                for (int i = 0; i < parts.Length; i++)
                {
                    if (current is Dictionary<string, object> dict)
                    {
                        if (dict.TryGetValue(parts[i], out var next))
                        {
                            current = next;
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else if (current is JsonElement element)
                    {
                        if (element.ValueKind == JsonValueKind.Object)
                        {
                            if (element.TryGetProperty(parts[i], out var nextProp))
                            {
                                current = nextProp;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else
                    {
                        return "";
                    }
                }

                if (current is JsonElement jsonEl)
                {
                    if (jsonEl.ValueKind == JsonValueKind.String)
                    {
                        return jsonEl.GetString() ?? "";
                    }
                    return jsonEl.ToString();
                }

                return current?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        public static string T(string key, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;

            string val = ResolveKey(_translations, key);
            if (string.IsNullOrEmpty(val))
            {
                if (_fallbackTranslations.Count == 0)
                {
                    LoadFallbackTranslations();
                }
                string fallbackVal = ResolveKey(_fallbackTranslations, key);
                if (!string.IsNullOrEmpty(fallbackVal))
                {
                    return fallbackVal;
                }
            }
            return string.IsNullOrEmpty(val) ? (string.IsNullOrEmpty(defaultValue) ? key : defaultValue) : val;
        }

        public static List<KeyValuePair<string, string>> GetAvailableLanguages()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("tr", "Türkçe (TR)"),
                new KeyValuePair<string, string>("en", "English (EN)"),
                new KeyValuePair<string, string>("de", "Deutsch (DE)"),
                new KeyValuePair<string, string>("ar", "العربية (AR)"),
                new KeyValuePair<string, string>("fr", "Français (FR)"),
                new KeyValuePair<string, string>("es", "Español (ES)")
            };
        }
    }
}
