using System;
using System.Collections.Generic;

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
    List<List<char>> scoreBoard = new List<List<char>>();
    Random rand = new Random();

    private static GomocupEngine instance;

    public static GomocupEngine Instance
    {
        get
        {
            if (instance == null)
                instance = new GomocupEngine();
            return (instance);
        }
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
            scoreBoard.Add(new List<char>());
            for (int j = 0; j < width; j++)
                scoreBoard[i].Add('0');
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
                scoreBoard[i][j] = '0';
        }
    }

    int checkDirection(int posX, int posY, int xDir, int yDir)
    {
        if (posY < 0 && posY >= board.Count)
            return (0);
        int scoreMe = 0;
        int scoreOp = 0;
        int loop = 0;
        for (int i = posY, j = posX; loop < 5 && i >= 0 && i < board.Count && j >= 0 && j < board[i].Count; i += yDir, j += xDir)
        {
            if (board[i][j] == '1')
                scoreMe += 10;
            else if (board[i][j] == '2')
                scoreOp += 10;
            loop += 1;
        }
        return ((scoreMe >= scoreOp) ? (scoreMe) : (scoreOp));
    }

    List<int> checkAllDirections(int x, int y)
    {
        List<int> list = new List<int>();

        // E, W, S, N, SE, NW, SW, NE
        list.Add(checkDirection(x, y, 1, 0));
        list.Add(checkDirection(x, y, -1, 0));
        list.Add(checkDirection(x, y, 0, 1));
        list.Add(checkDirection(x, y, 0, -1));
        list.Add(checkDirection(x, y, 1, 1));
        list.Add(checkDirection(x, y, -1, -1));
        list.Add(checkDirection(x, y, -1, 1));
        list.Add(checkDirection(x, y, 1, -1));
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
                    scoreBoard[i][j] = (char)max;
                    if ((char)highScore < scoreBoard[i][j])
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