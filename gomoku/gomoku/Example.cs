using System.Collections.Generic;
using gomoku;
using System;

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
    List<List<char>> board;
    List<List<int>> scoreBoard;
    Random rand = new Random();

    private static GomocupEngine instance = null;

    public static GomocupEngine Instance
    {
        get
        {
            if (instance == null)
                instance = new GomocupEngine();
            return (instance);
        }
    }

    public GomocupEngine()
    {
        this.board = new List<List<char>>();
        this.scoreBoard = new List<List<int>>();
        this.cmdHandler = new CommandHandler();
        this.Cmd = "";
    }

    public override string brain_about
    {
        get
        {
            return "name=\"Random\", author=\"Maxime Cauv1\", version=\"1.1\", country=\"OzuLand\", www=\"http://petr.lastovicka.sweb.cz\"";
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
        Console.WriteLine("DEBUG is free x = " + x + " | y = " + y);
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
        for (int i = posY, j = posX; loop < 5 && (!stopMe || !stopOp) && i >= 0 && i < board.Count && j >= 0 && j < board[i].Count; i += yDir, j += xDir)
        {
            if (board[i][j] == '1' && !stopMe)
            {
                scoreMe += 10;
                stopOp = true;
            }
            else if (board[i][j] == '2' && !stopOp)
            {
                scoreOp += 10;
                stopMe = true;
            }
            loop += 1;
        }
        return ((scoreMe > scoreOp) ? (scoreMe) : (scoreOp));
    }

    int CheckWeightAroundCell(int x, int y, out int baseScoreMe, out int baseScoreOp)
    {
        baseScoreMe = 0;
        baseScoreOp = 0;
        
        //E
        if (x + 1 < board[y].Count && board[y][x + 1] == '1')
            baseScoreMe += 5;
        else if (x + 1 < board[y].Count && board[y][x + 1] == '2')
            baseScoreOp += 5;

        //W
        if (x - 1 >= 0 && board[y][x - 1] == '1')
            baseScoreMe += 5;
        else if (x - 1 >= 0 && board[y][x - 1] == '2')
            baseScoreOp += 5;

        //S
        if (y + 1 < board.Count && board[y + 1][x] == '1')
            baseScoreMe += 5;
        else if (y + 1 < board.Count && board[y + 1][x] == '2')
            baseScoreOp += 5;

        //N
        if (y - 1 >= 0 && board[y - 1][x] == '1')
            baseScoreMe += 5;
        else if (y - 1 >= 0 && board[y - 1][x] == '2')
            baseScoreOp += 5;

        //SE
        if (y + 1 < board.Count && x + 1 < board[y].Count && board[y + 1][x + 1] == '1')
            baseScoreMe += 5;
        else if (y + 1 < board.Count && x + 1 < board[y].Count && board[y + 1][x + 1] == '2')
            baseScoreOp += 5;

        //NW
        if (y - 1 >= 0 && x - 1 >= 0 && board[y - 1][x - 1] == '1')
            baseScoreMe += 5;
        else if (y - 1 >= 0 && x - 1 >= 0 && board[y - 1][x - 1] == '2')
            baseScoreOp += 5;

        //NE
        if (y - 1 >= 0 && x + 1 < board[y].Count && board[y - 1][x + 1] == '1')
            baseScoreMe += 5;
        else if (y - 1 >= 0 && x + 1 < board[y].Count && board[y - 1][x + 1] == '2')
            baseScoreOp += 5;

        //SW
        if (y + 1 < board.Count && x - 1 >= 0 && board[y + 1][x - 1] == '1')
            baseScoreMe += 5;
        else if (y + 1 < board.Count && x - 1 >= 0 && board[y + 1][x - 1] == '2')
            baseScoreOp += 5;

        return ((baseScoreMe > baseScoreOp) ? (baseScoreMe) : (baseScoreOp));
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
        list.Add(checkDirection(x + 2, y + 1, -1, -1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, -1, -1, baseScoreMe, baseScoreOp));
        // SW
        list.Add(checkDirection(x + 1, y - 1, -1, 1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, -1, 1, baseScoreMe, baseScoreOp));
        // NE
        list.Add(checkDirection(x - 2, y + 2, 1, -1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x - 1, y + 1, 1, -1, baseScoreMe, baseScoreOp));
        list.Add(checkDirection(x, y, 1, -1, baseScoreMe, baseScoreOp));
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