using Newtonsoft.Json;

namespace E_Ticaret.WEBUI.ExtensionMethods
{
    public static class SessionExtensionMethods
    {
        public static void SetJson<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T? GetJson<T>(this ISession session, string key) where T : class
        {
            var value = session.GetString(key);
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
