using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

public class Point
{
    public ushort X;
    public ushort Y;

    public Point(ushort x, ushort y)
    {
        X = x;
        Y = y;
    }
}

class GomocupEngine : GomocupInterface
{
    const int MAX_BOARD = 100;
    List<List<char>> board = new List<List<char>>();
    List<List<int>> scoreBoard = new List<List<int>>();
    Random rand = new Random();

    public override string brain_about
    {
        get
        {
            return "name=\"Random\", author=\"Petr Lastovicka\", version=\"1.1\", country=\"Czech Republic\", www=\"http://petr.lastovicka.sweb.cz\"";
        }
    }

    public override void brain_init()
    {
        if (width < 5 || height < 5)
        {
            Console.WriteLine("ERROR size of the board");
            return;
        }
        if (width > MAX_BOARD || height > MAX_BOARD)
        {
            Console.WriteLine("ERROR Maximal board size is " + MAX_BOARD);
            return;
        }
        
        for (int i = 0; i < height; i++)
        {
            board.Add(new List<char>());
            for (int j = 0; j < width; j++)
                board[i].Add('0');
        }

        for (int i = 0; i < height; i++)
        {
            scoreBoard.Add(new List<int>());
            for (int j = 0; j < width; j++)
                scoreBoard[i].Add(0);
        }
        Console.WriteLine("OK");
    }

    public override void brain_restart()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                board[y][x] = '0';

        Console.WriteLine("OK");
    }

    private bool isFree(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height && board[y][x] == '0';
    }

    public override void brain_my(int x, int y)
    {
        if (isFree(x, y))
        {
            board[y][x] = '1';
        }
        else
        {
            Console.WriteLine("ERROR my move [{0},{1}]", x, y);
        }
    }

    public override void brain_opponents(int x, int y)
    {
        if (isFree(x, y))
        {
            board[y][x] = '2';
        }
        else
        {
            Console.WriteLine("ERROR opponents's move [{0},{1}]", x, y);
        }
    }

    public override void brain_block(int x, int y)
    {
        if (isFree(x, y))
        {
            board[y][x] = '3';
        }
        else
        {
            Console.WriteLine("ERROR winning move [{0},{1}]", x, y);
        }
    }

    public override int brain_takeback(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height && board[x][y] != '0')
        {
            board[y][x] = '0';
            return 0;
        }
        return 2;
    }

    void resetScoreBoard()
    {
        for (int i = 0; i < scoreBoard.Count; ++i)
        {
            for (int j = 0; j < scoreBoard[i].Count; ++j)
                scoreBoard[i][j] = 0;
        }
    }

    int checkDirection(int posX, int posY, int xDir, int yDir, int baseScoreMe, int baseScoreOp)
    {
        if (posY < 0 && posY >= board.Count)
            return (0);
        int scoreMe = baseScoreMe;
        int scoreOp = baseScoreOp;
        bool stopMe = false;
        bool stopOp = false;
        int loop = 0;
        short foundMe = 0;
        short foundOp = 0;
        for (int i = posY, j = posX; loop < 5 && (!stopMe || !stopOp) && i >= 0 && i < board.Count && j >= 0 && j < board[i].Count; i += yDir, j += xDir)
        {
            if (board[i][j] == '0' && loop == 4)
            {
                if (!stopMe && foundMe == 3)
                    scoreMe += 20;
                if (!stopOp && foundOp == 3)
                    scoreOp += 20;
            }
            else if (board[i][j] == '1')
            {
                if (!stopMe)
                {
                    foundMe += 1;
                    scoreMe += 10 * foundMe;
                    stopOp = true;
                }
                scoreOp -= 20;
            }
            else if (board[i][j] == '2')
            {
                if (!stopOp)
                {
                    foundOp += 1;
                    scoreOp += 10 * foundOp;
                    stopMe = true;
                }
                scoreMe -= 20;
            }
            loop += 1;
            if (i + yDir < 0 || i + yDir >= board.Count || j + xDir < 0 && j + xDir >= board[i].Count)
                return (0);
        }
        if (foundMe == 4)
            return (scoreMe * 200);
        else if (foundOp == 4)
            return (scoreOp * 100);
        return ((scoreMe >= scoreOp) ? (scoreMe) : (scoreOp));
    }

    void CheckWeightAroundCell(int x, int y, out int baseScoreMe, out int baseScoreOp)
    {
        baseScoreMe = 0;
        baseScoreOp = 0;
        
        //E
        if (x + 1 < board[y].Count && board[y][x + 1] == '1')
            baseScoreMe += 1;
        else if (x + 1 < board[y].Count && board[y][x + 1] == '2')
            baseScoreOp += 1;

        //W
        if (x - 1 >= 0 && board[y][x - 1] == '1')
            baseScoreMe += 1;
        else if (x - 1 >= 0 && board[y][x - 1] == '2')
            baseScoreOp += 1;

        //S
        if (y + 1 < board.Count && board[y + 1][x] == '1')
            baseScoreMe += 1;
        else if (y + 1 < board.Count && board[y + 1][x] == '2')
            baseScoreOp += 1;

        //N
        if (y - 1 >= 0 && board[y - 1][x] == '1')
            baseScoreMe += 1;
        else if (y - 1 >= 0 && board[y - 1][x] == '2')
            baseScoreOp += 1;

        //SE
        if (y + 1 < board.Count && x + 1 < board[y].Count && board[y + 1][x + 1] == '1')
            baseScoreMe += 1;
        else if (y + 1 < board.Count && x + 1 < board[y].Count && board[y + 1][x + 1] == '2')
            baseScoreOp += 1;

        //NW
        if (y - 1 >= 0 && x - 1 >= 0 && board[y - 1][x - 1] == '1')
            baseScoreMe += 1;
        else if (y - 1 >= 0 && x - 1 >= 0 && board[y - 1][x - 1] == '2')
            baseScoreOp += 1;

        //NE
        if (y - 1 >= 0 && x + 1 < board[y].Count && board[y - 1][x + 1] == '1')
            baseScoreMe += 1;
        else if (y - 1 >= 0 && x + 1 < board[y].Count && board[y - 1][x + 1] == '2')
            baseScoreOp += 1;

        //SW
        if (y + 1 < board.Count && x - 1 >= 0 && board[y + 1][x - 1] == '1')
            baseScoreMe += 1;
        else if (y + 1 < board.Count && x - 1 >= 0 && board[y + 1][x - 1] == '2')
            baseScoreOp += 1;
    }

    List<int> checkAllDirections(int x, int y)
    {
        List<int> list = new List<int>();
        CheckWeightAroundCell(x, y, out int baseScoreMe, out int baseScoreOp);

        // E
        list.Add(checkDirection(x - 1, y, 1, 0, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, 1, 0, baseScoreMe, baseScoreOp));
        // W
        list.Add(checkDirection(x + 2, y, -1, 0, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x + 1, y, -1, 0, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, -1, 0, baseScoreMe, baseScoreOp));
        // S
        list.Add(checkDirection(x, y - 1, 0, 1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, 0, 1, baseScoreMe, baseScoreOp));
        // N
        list.Add(checkDirection(x, y + 2, 0, -1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y + 1, 0, -1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, 0, -1, baseScoreMe, baseScoreOp));
        // SE
        list.Add(checkDirection(x - 1, y - 1, 1, 1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, 1, 1, baseScoreMe, baseScoreOp));
        // NW
        list.Add(checkDirection(x + 2, y + 2, -1, -1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x + 1, y + 1, -1, -1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, -1, -1, baseScoreMe, baseScoreOp));
        // SW
        list.Add(checkDirection(x + 1, y - 1, -1, 1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, -1, 1, baseScoreMe, baseScoreOp));
        // NE
        list.Add(checkDirection(x - 2, y + 2, 1, -1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x - 1, y + 1, 1, -1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, 1, -1, baseScoreMe, baseScoreOp));
        Console.WriteLine("DEBUG [{0}, {1}] => E-1:{2}, E:{3}, W+2:{4}, W+1:{5}, W:{6}, S-1:{7}, S:{8}, N+2:{9}, N-1:{10}, N:{11}, SE1:{12}, SE:{13}, NW2:{14}, NW1:{15}, NW:{16}, SW1:{17}, SW:{18}, NE2:{19}, NE1:{20}, NE:{21}",
                          x, y, list[0], list[1], list[2], list[3], list[4], list[5], list[6], list[0], list[7], list[8], list[9], list[10], list[11], list[12], list[13], list[14], list[15], list[16], list[17], list[18]);
        return (list);
    }

    Point findBestMove()
    {
        Console.WriteLine("DEBUG debut findbestmove");
        int highScore = 0;
        int max;
        Point bestPos = new Point((ushort)(height / 2), (ushort)(width / 2));

        for (int i = 0; i < board.Count; ++i)
        {
            for (int j = 0; j < board[i].Count; ++j)
            {
                if (board[i][j] == '0')
                {
                    List<int> weights = checkAllDirections(j, i);
                    max = 0;
                    for (int k = 0; k < weights.Count; k++)
                    {
                        if (max < weights[k])
                            max = weights[k];
                    }
                    Console.WriteLine("DEBUG [{0}, {1}] => {2}", j, i, max);
                    scoreBoard[i][j] = max;
                    if (highScore < scoreBoard[i][j])
                    {
                        highScore = scoreBoard[i][j];
                        bestPos = new Point((ushort)j, (ushort)i);
                    }
                }
            }
        }
        return (bestPos);
    }

    public override void brain_turn()
    {
        Console.WriteLine("DEBUG TURN");
        Console.WriteLine("DEBUG PRINT MAP BEFORE");
        for (int i = 0; i < board.Count; i++)
        {
            string s = "";
            for (int j = 0; j < board[i].Count; j++)
            {
                s += board[i][j];
            }
            Console.WriteLine("DEBUG {0}", s);
        }
        resetScoreBoard();
        Console.WriteLine("DEBUG avant findbestmove");
        Point pos = findBestMove();
        Console.WriteLine("DEBUG apres findbestmove");
        Console.WriteLine("DEBUG [{0}, {1}]", pos.X, pos.Y);
        do_mymove(pos.X, pos.Y);
        Console.WriteLine("DEBUG PRINT MAP AFTER");
        for (int i = 0; i < board.Count; i++)
        {
            string s = "";
            for (int j = 0; j < board[i].Count; j++)
            {
                s += board[i][j];
            }
            Console.WriteLine("DEBUG {0}", s);
        }
    }

    public override void brain_end()
    {
    }

    public override void brain_eval(int x, int y)
    {
    }
}