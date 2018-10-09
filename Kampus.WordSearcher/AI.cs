using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            MakeMove(Direction.Up);
            GoToRound();
        }

        void MakeMove(Direction direction)
        {
            field = client.MakeMove(direction);
        }

        void GoToRound()
        {
            FindNotNullCells();
            AlignPosition();
            bool[,] screenshot = Screenshot();
            int x = GetWidthMap(screenshot), y = GetHeightMap(screenshot);
            listMap.RemoveRange(listMap.Count - height - 1, height);
            Console.WriteLine("{0} {1}", x, y);
            Console.WriteLine(client.GetStatistics().Value.Points);
            Console.WriteLine(field.Value.ToString('-', '#'));
            WriteListMapInFile(@"C:\Users\User\Desktop\listMapBegin.txt");
            Next(x, y);
        }

        void Next(int w, int h)
        {
            int curX = 0, curY = 0;
            for (int i = 0; i < height; i++, curY++)
                MakeMove(Direction.Down);
            for (int i = 0; i < width; i++, curX++)
                MakeMove(Direction.Right);
            if (w % width == 0)
                MoveParallelY(curX, curY, w, h);
            else
                MoveParallelX(curX, curY, w, h);
            WriteListMapInFile(@"C:\Users\User\Desktop\listMap.txt");
        }

        void WriteListMapInFile(string uri)
        {
            StreamWriter writeList = new StreamWriter(uri, false);
            foreach (List<bool> listI in listMap)
            {
                foreach (bool listJ in listI)
                    writeList.Write(listJ ? '#' : '-');
                writeList.Write('\n');
            }

            writeList.Close();
        }

        void MoveParallelY(int curX, int curY, int w, int h)
        {
            bool dir = true;
            for (int i = 0; i < (w - width) / (double) width; i++)
            {
                for (int j = 0; j < h - 2 * height; j++, curY += dir ? 1 : -1)
                {
                    MakeMove(dir ? Direction.Down : Direction.Up);
                    AddRowInListMap(curX, curY, dir ? height - 1 : 0, true, w);
                }

                for (int j = 0; j < width; j++, curX++)
                {
                    MakeMove(Direction.Right);
                    AddColumnInListMap(curX, curY, width - 1, true);
                }

                dir = !dir;
            }
        }

        void MoveParallelX(int curX, int curY, int w, int h)
        {
            bool dir = true;
            for (int i = 0; i < (h - height) / (double) height; i++)
            {
                for (int j = 0; j < w - 2 * width; j++, curX += dir ? 1 : -1)
                {
                    MakeMove(dir ? Direction.Right : Direction.Left);
                    AddColumnInListMap(curX, curY, dir ? width - 1 : 0, true);
                }

                for (int j = 0; j < height; j++, curY++)
                {
                    MakeMove(Direction.Down);
                    AddRowInListMap(curX, curY, height - 1, true, w);
                }

                dir = !dir;
            }
        }


        void AddColumnInListMap(int x, int y, int numColumn, bool @lock)
        {
            if (!@lock) AddNewRows(y + height);
            for (int i = 0; i < height; i++)
                if (y + i >= listMap.Count)
                    break;
                else if (listMap[y + i].Count <= x)
                {
                    if (@lock) continue;
                    AddNewColumns(x, y + i);
                    listMap[y + i].Add(field.Value[i, numColumn]);
                }
                else
                    listMap[y + i][x] = field.Value[i, numColumn];
        }

        void AddRowInListMap(int x, int y, int numRow, bool @lock, int w = int.MaxValue)
        {
            if (!@lock) AddNewRows(y + height);
            for (int i = x; i < x + width; i++)
            {
                if (i + 1 >= w || y + height - 1 >= listMap.Count) break;
                AddNewColumns(i + 1, y + height - 1);
                listMap[y + height - 1][i] = field.Value[numRow, i - x];
            }
        }

        void AddNewColumns(int x, int y)
        {
            while (listMap[y].Count < x)
                listMap[y].Add(false);
        }

        void AddNewRows(int y)
        {
            while (listMap.Count < y)
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
                MakeMove(trigger ? Direction.Down : Direction.Right);
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
                MakeMove(dir);
        }

        int GetWidthMap(bool[,] screenshot)
        {
            int x = 0;
            do
            {
                AddColumnInListMap(x, 0, 0, false);
                x++;
                MakeMove(Direction.Right);
            } while (!EqualsForScreenshot(screenshot));

            return x;
        }

        int GetHeightMap(bool[,] screenshot)
        {
            int y = 0;
            do
            {
                y++;
                MakeMove(Direction.Down);
                AddRowInListMap(0, y, height - 1, false);
            } while (!EqualsForScreenshot(screenshot));

            return y;
        }
    }
}