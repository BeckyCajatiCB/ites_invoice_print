using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace InvoicePrint.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(
                obj,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }).Replace(@"\", " ");
        }
    }
}
