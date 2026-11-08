using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace SolidOps.SubZero
{
    public static class Serializer
    {
        public static JsonSerializerOptions GetOptions(bool preserveReference, bool camelCase = false)
        {
            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };
            if (preserveReference)
                options.ReferenceHandler = ReferenceHandler.Preserve;

            if (camelCase)
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            return options;
        }

        public static string Serialize(object obj, bool preserveReference = false, bool safeMode = true, bool camelCase = false)
        {
            if (obj == null) return null;

            try
            {
                return JsonSerializer.Serialize(obj, GetOptions(preserveReference, camelCase));
            }
            catch (JsonException e)
            {
                if (!preserveReference && safeMode && e.Message.StartsWith("A possible object cycle was detected"))
                    return JsonSerializer.Serialize(obj, GetOptions(true, camelCase));
                else
                    throw;
            }
        }

        public static T Deserialize<T>(string json, bool preserveReference = false, bool safeMode = true, bool camelCase = false)
        {
            return (T)Deserialize(json, typeof(T), preserveReference, safeMode, camelCase);
        }

        public static object Deserialize(string json, Type type, bool preserveReference = false, bool safeMode = true, bool camelCase = false)
        {
            if (string.IsNullOrEmpty(json)) return null;

            // A false positive (a string value equal to "$id") is harmless: Preserve reads metadata-free JSON unchanged.
            if (safeMode && !preserveReference && json.Contains("\"$id\"", StringComparison.Ordinal))
                preserveReference = true;

            return JsonSerializer.Deserialize(json, type, GetOptions(preserveReference, camelCase));
        }
    }
}
