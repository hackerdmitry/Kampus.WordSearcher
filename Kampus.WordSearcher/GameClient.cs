using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace Kampus.WordSearcher
{
    public class GameClient : IGameClient, IDisposable
    {
        public GameClient(string url, string authToken, int tryTimeout = 1000, int retryTimeout = 2000, int retriesCount = 10)
        {
            baseUri = new Uri(url.Trim('/'));
            client = new HttpClientWithRetries(retriesCount, TimeSpan.FromMilliseconds(tryTimeout), TimeSpan.FromMilliseconds(retryTimeout));
            client.DefaultHeaders.Add("Authorization", string.Format("token {0}", authToken));
        }

        public Result<SessionInfo> InitSession()
        {
            Result<HttpResponseMessage> r = client.PostWithRetriesRaw(GetUri("task/game/start?test=true"), "", "");
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
            Result<string> result = client.PostWithRetries(GetUri("task/move", direction.ToString().ToLower()), "", "");
            if (result.IsFaulted)
                return Result<bool[,]>.Fail(result.Status);

            char[][] array = result.Value.Split('\n').Select(l => l.Trim().ToCharArray()).ToArray();
            bool[,] map = new bool[array.Length, array.First().Length];
            for (int row = 0; row < map.GetLength(0); row++)
            for (int column = 0; column < map.GetLength(1); column++)
                map[row, column] = array[row][column] == '1';
            return map.ToResult();
        }

        public Result<PointsStatistic> SendWords(IEnumerable<string> words)
        {
            return client.PostWithRetries(GetUri("task/words"), JsonConvert.SerializeObject(words.ToArray()), "application/json").Deserialize<PointsStatistic>();
        }

        Uri GetUri(params string[] path)
        {
            return new Uri(baseUri, string.Join("/", path));
        }

        public void Dispose()
        {
            client.Dispose();
        }

        ~GameClient()
        {
            Dispose();
        }

        readonly HttpClientWithRetries client;
        readonly Uri baseUri;
    }
}
