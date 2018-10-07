using Newtonsoft.Json;

namespace Kampus.WordSearcher
{
    public class PointsStatistic
    {
        [JsonProperty("points")] public int Points { get; set; }
    }
}