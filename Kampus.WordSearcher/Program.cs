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
            Console.WriteLine("{0} {1}", url, apiKey);
            using (GameClient client = new GameClient(url, apiKey))
            {
                Result<SessionInfo> info = client.InitSession();
                if (info.Status == Status.Conflict)
                    client.InitSession();
                Direction direction = Direction.Down;
                new AI(client);
            }
        }

        static Direction ToDirection(this ConsoleKeyInfo k)
        {
            switch (k.Key)
            {
                case ConsoleKey.W: return Direction.Up;
                case ConsoleKey.A: return Direction.Left;
                case ConsoleKey.S: return Direction.Down;
                case ConsoleKey.D: return Direction.Right;
            }

            throw new InvalidOperationException();
        }

        public static string ToString(this bool[,] map, char empty, char full)
        {
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < map.GetLength(0); row++)
            {
                for (int column = 0; column < map.GetLength(1); column++)
                {
                    sb.Append(map[row, column] ? full : empty);
                }

                sb.Append("\n");
            }

            return sb.ToString();
        }
    }
}