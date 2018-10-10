using System;
using System.Linq;
using System.Text;

namespace Kampus.WordSearcher
{
    static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Expected 2 console line arguments!");
            string url = args[0];
            string apiKey = args[1];
            using (GameClient client = new GameClient(url, apiKey))
            {
                Result<SessionInfo> info = client.InitSession();
                if (info.Status == Status.Conflict)
                    client.InitSession();
                Console.WriteLine(info.Value.Expires.TotalMinutes);
                FindAndSendWords(client);
            }
        }

        static void FindAndSendWords(GameClient client)
        {
            tryAgain:
//            try
//            {
                new AI(client);
//            }
//            catch
//            {
//                goto tryAgain;
//            }
        }
    }
}