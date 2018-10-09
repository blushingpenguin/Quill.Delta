using Newtonsoft.Json.Linq;

namespace Quill.Delta
{
    public static class JsonHelpers
    {
        static bool IsSimpleValue(JToken t)
        {
            return t != null && t.Type != JTokenType.Object &&
                t.Type != JTokenType.Array;
        }

        public static string GetStringValue(this JToken t, string key)
        {
            var v = t[key];
            return IsSimpleValue(v) ? v.Value<string>() : "";
        }

        public static int? GetIntValue(this JToken t, string key)
        {
            var v = t[key];
            return IsSimpleValue(v) ? v.Value<int?>() : null;
        }

        public static bool? GetBoolValue(this JToken t, string key)
        {
            var v = t[key];
            bool b;
            return IsSimpleValue(v) && bool.TryParse(
                v.Value<string>(), out b) ? (bool?)b : null;
        }
    }
}
