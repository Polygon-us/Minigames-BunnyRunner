using Newtonsoft.Json;

namespace FirebaseCore.Utils
{
    public static class Converter
    {
        public static T ConvertTo<T>(this string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }

        public static T ConvertTo<T>(this object obj)
        {
            return ConvertTo<T>(JsonConvert.SerializeObject(obj));
        }
    }
}