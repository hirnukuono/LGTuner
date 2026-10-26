using AmorLib.Dependencies;
using AmorLib.Utils;
using System.Text.Json;

namespace LGTuner.Utils
{
    internal static class JSON
    {
        private static readonly JsonSerializerOptions _setting = JsonSerializerUtil.CreateDefaultSettings(true, PartialData_Wrapper.IsLoaded, false);

        static JSON()
        {
            
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _setting)!;
        }

        public static object Deserialize(Type type, string json)
        {
            return JsonSerializer.Deserialize(json, type, _setting)!;
        }

        public static string Serialize(object value, Type type)
        {
            return JsonSerializer.Serialize(value, type, _setting);
        }
    }
}
