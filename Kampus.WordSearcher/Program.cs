using System;
using System.Text;

namespace Kampus.WordSearcher
{
    static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Expected 2 console line arguments!");
            var url = args[0];
            var apiKey = args[1];

            using (var client = new GameClient(url, apiKey))
            {
                var info = client.InitSession();
                if (info.Status == Status.Conflict)
                    client.InitSession();
                var direction = Direction.Up;
                while (true)
                {
                    var field = client.MakeMove(direction);
                    Console.Clear();
                    Console.WriteLine(field.Value.ToString(' ', '#'));
                    direction = Console.ReadKey(true).ToDirection();
                }
                var stat = client.GetStatistics();
                var pass = client.SendWords(new[] {"ШПОРА"});
                var end = client.FinishSession();
            }
        }

        private static Direction ToDirection(this ConsoleKeyInfo k)
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

        private static string ToString(this bool[,] map, char empty, char full)
        {
            var sb = new StringBuilder();
            for (var row = 0; row < map.GetLength(0); row++)
            {
                for (var column = 0; column < map.GetLength(1); column++)
                {
                    sb.Append(map[row, column] ? full : empty);
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
