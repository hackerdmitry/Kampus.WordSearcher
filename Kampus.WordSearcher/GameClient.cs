using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Kampus.WordSearcher
{
    public class GameClient : IGameClient, IDisposable
    {
        public GameClient(string url, string authToken, int tryTimeout = 1000, int retryTimeout = 2000, int retriesCount = 10)
        {
            baseUri = new Uri(url.Trim('/'));
            client = new HttpClientWithRetries(retriesCount, TimeSpan.FromMilliseconds(tryTimeout), TimeSpan.FromMilliseconds(retryTimeout));
            client.DefaultHeaders.Add("Authorization", $"token {authToken}");
        }

        public Result<SessionInfo> InitSession()
        {
            var r = client.PostWithRetriesRaw(GetUri("task/game/start"), "", "");
            if (r.IsFaulted)
                return Result<SessionInfo>.Fail(r.Status);

            return new SessionInfo
            {
                Expires = TimeSpan.FromSeconds(int.Parse(r.Value.GetHeader("Expires"))),
                Created = DateTime.Parse(r.Value.GetHeader("Last-Modified"))
            }.ToResult();
        }

        public Result<PointsStatistic> FinishSession()
        {
            return client.PostWithRetries(GetUri("task/game/finish"), "", "").Deserialize<PointsStatistic>();
        }

        public Result<FullStatistic> GetStatistics()
        {
            return client.GetWithRetries(GetUri("task/game/stats")).Deserialize<FullStatistic>();
        }

        public Result<bool[,]> MakeMove(Direction direction)
        {
            var result = client.PostWithRetries(GetUri("task/move", direction.ToString().ToLower()), "", "");
            if (result.IsFaulted)
                return Result<bool[,]>.Fail(result.Status);

            var array = result.Value.Split('\n').Select(l => l.Trim().ToCharArray()).ToArray();
            var map = new bool[array.Length, array.First().Length];
            for (var row = 0; row < map.GetLength(0); row++)
            for (var column = 0; column < map.GetLength(1); column++)
                map[row, column] = array[row][column] == '1';
            return map.ToResult();
        }

        public Result<PointsStatistic> SendWords(IEnumerable<string> words)
        {
            return client.PostWithRetries(GetUri("task/words"), JsonConvert.SerializeObject(words.ToArray()), "application/json").Deserialize<PointsStatistic>();
        }

        private Uri GetUri(params string[] path) => new Uri(baseUri, string.Join("/", path));

        public void Dispose() => client.Dispose();
        ~GameClient() => Dispose();

        private readonly HttpClientWithRetries client;
        private readonly Uri baseUri;
    }
}
