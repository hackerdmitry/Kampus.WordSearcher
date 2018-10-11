using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kampus.WordSearcher
{
    public static class Letter
    {
        const int hCheck = 2, wCheck = 3;
        static readonly int[] rowCheck = {0, AI.height - 1};

        static readonly int[,] helpCheckpoints =
        {
            {0, AI.width / 2, AI.width - 1},
            {0, AI.width / 2, AI.width - 1}
        };

        public const char wrongLetter = '#';

        static readonly HashSet<char>[,] checkpoints = new HashSet<char>[hCheck, wCheck];
        static readonly HashSet<char> allLetters = new HashSet<char>();
        static readonly Dictionary<char, bool[][]> parseLetter = new Dictionary<char, bool[][]>();

        static readonly Tuple<int, int>[] basicPoints =
        {
            Tuple.Create(0, 0),
            Tuple.Create(2, 1),
            Tuple.Create(5, 6)
        };

        static Letter()
        {
            DirectoryInfo dir = new DirectoryInfo(@".\Kampus_letters");
            for (int i = 0; i < hCheck; i++)
                for (int j = 0; j < wCheck; j++)
                    checkpoints[i, j] = new HashSet<char>();
            foreach (FileInfo letterFile in dir.GetFiles())
            {
                bool[][] parseLetterFile = letterFile.OpenText().ReadToEnd().ToValue();
                char letter = letterFile.Name[0];
                allLetters.Add(letter);
                for (int i = 0; i < hCheck; i++)
                    for (int j = 0; j < wCheck; j++)
                        if (parseLetterFile[rowCheck[i]][helpCheckpoints[i, j]])
                            checkpoints[i, j].Add(letter);
                parseLetter.Add(letter, parseLetterFile);
            }
        }

        public static char GetLetter(this List<List<bool>> listMap, int x, int y, int w, int h)
        {
            List<char> possibleLetters = new List<char>(allLetters);
            for (int i = 0; i < hCheck; i++)
                for (int j = 0; j < wCheck; j++)
                    possibleLetters = listMap[(rowCheck[i] + y) % h][(helpCheckpoints[i, j] + x) % w]
                        ? possibleLetters.Where(m => checkpoints[i, j].Any(n => n == m)).ToList()
                        : possibleLetters.Where(m => checkpoints[i, j].All(n => n != m)).ToList();

            foreach (char letter in possibleLetters)
                if (ClarifyLetter(listMap, letter, x, y, w, h))
                    return letter;
            return wrongLetter;
        }

        static bool ClarifyLetter(IReadOnlyList<List<bool>> listMap, char letter, int x, int y, int w, int h)
        {
            bool[][] parseLetter = Letter.parseLetter[letter];
            for (int i = 0; i < AI.height; i++)
                for (int j = 0; j < AI.width; j++)
                    if (listMap[(i + y) % h][(j + x) % w] ^ parseLetter[i][j])
                        return false;
            return true;
        }

        public static bool SomethingLikeALetter(this List<List<bool>> listMap, int x, int y, int w, int h)
        {
            return basicPoints.Any(point => listMap[(point.Item1 + y) % h][(point.Item2 + x) % w]);
        }

        static bool[][] ToValue(this string notValue)
        {
            return notValue.Split('\n').Select(x => Array.ConvertAll(x.ToCharArray(), y => y == '1')).ToArray();
        }
    }
}