using System.Text.Json;

namespace Nlk_Cheffie_Print.Models
{
    public class PrintJob
    {
        public string Role { get; set; } = "kitchen"; // kitchen, cashier, courier
        public string Template { get; set; } = "";
        public JsonElement Data { get; set; }
        public string JobId { get; set; } = "";
        public bool SkipOrderLog { get; set; } = false;
        public bool ForcePrint { get; set; } = false;
    }
}
