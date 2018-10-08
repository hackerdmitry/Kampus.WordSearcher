using System;
using System.Collections.Generic;
using System.Linq;

namespace Kampus.WordSearcher
{
    public class AI
    {
        readonly GameClient client;
        Result<bool[,]> field;
        const int width = 7, height = 7;
        readonly List<List<bool>> listMap = new List<List<bool>>();

        public AI(GameClient client)
        {
            this.client = client;
            field = this.client.MakeMove(Direction.Up);
            GoToRound();
        }

        void GoToRound()
        {
            FindNotNullCells();
            AlignPosition();
            bool[,] screenshot = Screenshot();
            int x = GetWidthMap(screenshot), y = GetHeightMap(screenshot);
            listMap.RemoveRange(listMap.Count - height, height);
            Console.WriteLine("{0} {1}", x, y);
            Console.WriteLine(client.GetStatistics().Value.Points);
            Console.WriteLine(field.Value.ToString('-', '#'));
        }

        void AddColumnInListMap(int x, int y, int numColumn)
        {
            AddNewRows(y + height);
            for (int i = 0; i < height; i++)
                if (listMap[y + i].Count <= x)
                {
                    AddNewColumns(x, y + i);
                    listMap[y + i].Add(field.Value[i, numColumn]);
                }
                else
                    listMap[y + i][x] = field.Value[i, numColumn];
        }

        void AddRowInListMap(int x, int y, int numRow)
        {
            AddNewRows(y + height);
            for (int i = x; i < x + width; i++)
            {
                AddNewColumns(i + 1, y + height - 1);
                listMap[y + height - 1][i] = field.Value[numRow, i];
            }
        }

        void AddNewColumns(int x, int y)
        {
            while (listMap[y].Count < x)
                listMap[y].Add(false);
        }

        void AddNewRows(int y)
        {
            while(listMap.Count < y)
                listMap.Add(new List<bool>());
        }

        int CheckFullCells()
        {
            int count = 0;
            bool[,] map = field.Value;
            for (int column = 0; column < height; column++)
                for (int row = 0; row < width; row++)
                    if (map[row, column])
                        count++;
            return count;
        }

        int CheckClearLine(out bool findFull)
        {
            bool[,] map = field.Value;
            findFull = false;
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                    if (map[row, column])
                    {
                        if (!findFull && row != 0)
                            return row - 1;
                        findFull = true;
                        goto cont;
                    }

                if (findFull) return row;
                cont: ;
            }

            return -1;
        }

        bool[,] Screenshot()
        {
            bool[,] map = new bool[width, height];
            for (int column = 0; column < height; column++)
                for (int row = 0; row < width; row++)
                    map[row, column] = field.Value[row, column];
            return map;
        }

        bool EqualsForScreenshot(bool[,] screenshot)
        {
            bool[,] map = field.Value;
            for (int column = 0; column < height; column++)
                for (int row = 0; row < width; row++)
                    if (screenshot[row, column] ^ map[row, column])
                        return false;
            return true;
        }

        void FindNotNullCells()
        {
            bool trigger = false;
            while (CheckFullCells() == 0)
            {
                field = client.MakeMove(trigger ? Direction.Down : Direction.Right);
                trigger = !trigger;
            }
        }

        void AlignPosition()
        {
            bool findFull;
            int clearLine = CheckClearLine(out findFull);
            Direction dir = findFull ? Direction.Up : Direction.Down;
            if (findFull)
                clearLine = height - clearLine - 1;
            for (int i = 0; i < clearLine + 1; i++)
                field = client.MakeMove(dir);
        }

        int GetWidthMap(bool[,] screenshot)
        {
            int x = 0;
            do
            {
                AddColumnInListMap(x, 0, width - 1);
                x++;
                field = client.MakeMove(Direction.Right);
            } while (!EqualsForScreenshot(screenshot));

            return x;
        }

        int GetHeightMap(bool[,] screenshot)
        {
            int y = 0;
            do
            {
                y++;
                AddRowInListMap(0, y, height - 1);
                field = client.MakeMove(Direction.Down);
            } while (!EqualsForScreenshot(screenshot));

            return y;
        }
    }
}