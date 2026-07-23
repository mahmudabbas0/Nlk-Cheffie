using System;
using System.Media;
using Nlk_Cheffie_Print.Core;

namespace Nlk_Cheffie_Print.Core
{
    public static class AudioAlertService
    {
        public static void PlayNewOrderSound()
        {
            if (!ConfigManager.Current.App.EnableOrderSound) return;

            try
            {
                // Play system exclamation / notification chime
                SystemSounds.Exclamation.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play order sound: {ex.Message}");
                try
                {
                    SystemSounds.Beep.Play();
                }
                catch
                {
                    // Ignore sound error
                }
            }
        }
    }
}
