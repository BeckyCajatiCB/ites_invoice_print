using Newtonsoft.Json;

namespace ContractFeatures.Models
{
    public class LogData
    {
        public LogData(string description, string detail)
        {
            Description = description;
            Detail = detail;
        }
        [JsonProperty]
        private string Description { get; }
        [JsonProperty]
        private string Detail { get; }
    }
}
