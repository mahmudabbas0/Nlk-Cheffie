using System;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Views;

namespace Nlk_Cheffie_Print
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize high DPI settings, default font, etc.
            ApplicationConfiguration.Initialize();

            // Load initial configurations
            ConfigManager.Load();

            // Set active language from config
            string activeLang = ConfigManager.Current.App.Language;
            LocalizationService.CurrentLanguage = activeLang;

            // Start Main Application Form
            Application.Run(new MainForm());
        }
    }
}