using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace Kampus.WordSearcher
{
    internal static class Helper
    {
        public static string GetHeader(this HttpResponseMessage message, string name) => message.Content.Headers.GetValues(name).Single();

        public static Result<T> Deserialize<T>(this Result<string> result) => result.Select(JsonConvert.DeserializeObject<T>);

        public static Result<T> ToResult<T>(this T value) => Result<T>.Success(value);
    }
}