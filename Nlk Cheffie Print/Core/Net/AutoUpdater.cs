using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;

namespace Nlk_Cheffie_Print.Core.Net
{
    public class AutoUpdater
    {
        public const string CurrentVersion = "1.0.0";
        private readonly string _apiBaseUrl;

        public event Action<string, string, bool>? UpdateAvailable; // (latestVersion, downloadUrl, isMandatory)
        public event Action<int>? DownloadProgressChanged;
        public event Action<string>? DownloadCompleted;
        public event Action<string>? ErrorOccurred;

        public AutoUpdater(string apiBaseUrl)
        {
            _apiBaseUrl = apiBaseUrl.TrimEnd('/');
        }

        public async Task CheckForUpdatesAsync()
        {
            string url = $"{_apiBaseUrl}/public/desktop/latest-version";
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        using (var doc = JsonDocument.Parse(json))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
                            {
                                string latestVersion = root.GetProperty("version").GetString() ?? "";
                                string downloadUrl = root.GetProperty("download_url").GetString() ?? "";
                                bool isMandatory = root.TryGetProperty("is_mandatory", out var manProp) && manProp.GetBoolean();

                                if (IsNewerVersion(latestVersion, CurrentVersion))
                                {
                                    UpdateAvailable?.Invoke(latestVersion, downloadUrl, isMandatory);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex.Message);
            }
        }

        public async Task DownloadAndInstallUpdateAsync(string downloadUrl)
        {
            try
            {
                string tempDir = Path.GetTempPath();
                string fileName = "nlkCheffie-Print-Setup.exe";

                // Strip query parameters to get file name if possible
                string uriPath = new Uri(downloadUrl).AbsolutePath;
                string potentialName = Path.GetFileName(uriPath);
                if (potentialName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = potentialName;
                }

                string tempFile = Path.Combine(tempDir, fileName);

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);
                    using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        long? totalBytes = response.Content.Headers.ContentLength;

                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            var buffer = new byte[8192];
                            long totalRead = 0;
                            int read;

                            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;

                                if (totalBytes.HasValue)
                                {
                                    int progress = (int)((double)totalRead / totalBytes.Value * 100);
                                    DownloadProgressChanged?.Invoke(progress);
                                }
                                else
                                {
                                    DownloadProgressChanged?.Invoke(0);
                                }
                            }
                        }
                    }
                }

                DownloadCompleted?.Invoke(tempFile);
                TriggerInstaller(tempFile);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex.Message);
            }
        }

        private void TriggerInstaller(string filePath)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(psi);

                // Exit app instantly to let installer overwrite locked files
                Application.Exit();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Kurulum başlatılamadı: {ex.Message}");
            }
        }

        private bool IsNewerVersion(string latest, string current)
        {
            try
            {
                var vLatest = new Version(latest);
                var vCurrent = new Version(current);
                return vLatest > vCurrent;
            }
            catch
            {
                // Fallback to simple split comparison if version string is non-standard
                try
                {
                    string[] lParts = latest.Split('.');
                    string[] cParts = current.Split('.');
                    for (int i = 0; i < Math.Min(lParts.Length, cParts.Length); i++)
                    {
                        int l = int.Parse(lParts[i]);
                        int c = int.Parse(cParts[i]);
                        if (l > c) return true;
                        if (l < c) return false;
                    }
                    return lParts.Length > cParts.Length;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
