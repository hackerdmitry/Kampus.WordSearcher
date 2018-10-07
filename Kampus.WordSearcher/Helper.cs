using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace Kampus.WordSearcher
{
    static class Helper
    {
        public static string GetHeader(this HttpResponseMessage message, string name)
        {
            return message.Content.Headers.GetValues(name).Single();
        }

        public static Result<T> Deserialize<T>(this Result<string> result)
        {
            return result.Select(JsonConvert.DeserializeObject<T>);
        }

        public static Result<T> ToResult<T>(this T value)
        {
            return Result<T>.Success(value);
        }
    }
}