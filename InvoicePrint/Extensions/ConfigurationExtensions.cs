using Microsoft.Extensions.Configuration;

namespace InvoicePrint.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationRoot Current { get; set; }

        public static string Environment { set; get; }

        public static bool IsDevelopment()
        {
            return Environment.ToLower() == "development";
        }
    }
}
