﻿using System;
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

    void ResetScoreBoard()
    {
        for (int i = 0; i < scoreBoard.Count; ++i)
        {
            for (int j = 0; j < scoreBoard[i].Count; ++j)
                scoreBoard[i][j] = 0;
        }
    }

    int CheckDirection(int posX, int posY, int xDir, int yDir, char player)
    {
        string s = "X";
        bool blockCursor1 = false;
        bool blockCursor2 = false;
        for (int loop = 0, x1 = posX + xDir, y1 = posY + yDir, x2 = posX + xDir * -1, y2 = posY + yDir * -1;
             loop < 5;
             loop++, x1 += xDir, x2 += xDir * -1, y1 += yDir, y2 += yDir * -1)
        {
            if (!blockCursor1)
            {
                if (y1 < 0 || y1 >= board.Count || x1 < 0 || x1 >= board[y1].Count)
                    blockCursor1 = true;
                else if (board[y1][x1] == player)
                    s = "X" + s;
                else if (board[y1][x1] == '0')
                    s = "H" + s;
                else
                    blockCursor1 = true;
            }
            if (!blockCursor2)
            {
                if (y2 < 0 || y2 >= board.Count || x2 < 0 || x2 >= board[y2].Count)
                    blockCursor2 = true;
                else if (board[y2][x2] == player)
                    s += "X";
                else if (board[y2][x2] == '0')
                    s += "H";
                else
                    blockCursor2 = true;
            }
        }
        //Console.WriteLine("DEBUG DIR: [{0}, {1}] | PLAYER: [{2}] | STRING: [{3}]", xDir, yDir, player, s);
        if (s.Contains("XXXXX"))
            return (41000000);
        else if (s.Contains("HXXXXH"))
            return (10100000);
        else if (s.Contains("XXXXH") || s.Contains("HXXXX") || s.Contains("HXXXHH") || s.Contains("HHXXXH") || s.Contains("HXHXXH") ||
            s.Contains("HXXHXH") || s.Contains("HXXXHX") || s.Contains("XHXXXH") || s.Contains("XXXHX") || s.Contains("XHXXX"))
            return (2510000);
        else if (s.Contains("XXXHH") || s.Contains("HHXXX") || s.Contains("XXHXH") || s.Contains("HXHXX") || s.Contains("XHXXH") ||
                 s.Contains("HXXHX") || s.Contains("XXHHX") || s.Contains("XHHXX") || s.Contains("XHXHX"))
            return (625100);
        else if (s.Contains("XX"))
            return (156260);
        return (0);
    }

    int CheckAllDirections(int x, int y)
    {
        List<int> weightsMe = new List<int>
        {
            CheckDirection(x, y, 0, 1, '1'),
            CheckDirection(x, y, 1, 0, '1'),
            CheckDirection(x, y, 1, 1, '1'),
            CheckDirection(x, y, -1, 1, '1')
        };
        List<int> weightsOp = new List<int>
        {
            CheckDirection(x, y, 0, 1, '2'),
            CheckDirection(x, y, 1, 0, '2'),
            CheckDirection(x, y, 1, 1, '2'),
            CheckDirection(x, y, -1, 1, '2')
        };
        //Console.WriteLine("DEBUG [{0}, {1}]", x, y);
        //Console.WriteLine("DEBUG weightsMe: {0}", JsonConvert.SerializeObject(weightsMe));
        //Console.WriteLine("DEBUG weightsOp: {0}", JsonConvert.SerializeObject(weightsOp));
        int sumMe = weightsMe.Sum();
        int sumOp = weightsOp.Sum();
        //Console.WriteLine("DEBUG sumMe: {0} | sumOp {1}", sumMe, sumOp);
        //Console.WriteLine("DEBUG ------------------------------");
        return ((sumMe >= sumOp) ? (sumMe) : (sumOp));
    }

    Point FindBestMove()
    {
        int highScore = 0;
        Point bestPos = new Point((ushort)(height / 2), (ushort)(width / 2));

        for (int i = 0; i < board.Count; ++i)
        {
            for (int j = 0; j < board[i].Count; ++j)
            {
                if (board[i][j] == '0')
                {
                    scoreBoard[i][j] = CheckAllDirections(j, i);
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
        //Console.WriteLine("DEBUG TURN");
        //Console.WriteLine("DEBUG PRINT MAP BEFORE");
        for (int i = 0; i < board.Count; i++)
        {
            string s = "";
            for (int j = 0; j < board[i].Count; j++)
            {
                s += board[i][j];
            }
            //Console.WriteLine("DEBUG {0}", s);
        }
        ResetScoreBoard();
        //Console.WriteLine("DEBUG avant findbestmove");
        Point pos = FindBestMove();
        //Console.WriteLine("DEBUG apres findbestmove");
        //Console.WriteLine("DEBUG [{0}, {1}]", pos.X, pos.Y);
        do_mymove(pos.X, pos.Y);
        //Console.WriteLine("DEBUG PRINT MAP AFTER");
        for (int i = 0; i < board.Count; i++)
        {
            string s = "";
            for (int j = 0; j < board[i].Count; j++)
            {
                s += board[i][j];
            }
            //Console.WriteLine("DEBUG {0}", s);
        }
    }

    public override void brain_end()
    {
    }

    public override void brain_eval(int x, int y)
    {
    }
}