/** functions that communicate with Piskvork manager through pipes */
/** don't modify this file */
using System.Threading;
using System; 
using gomoku;

delegate void ParamFunc(string param);

abstract class GomocupInterface
{
    /* information about a game - you should use these variables */
    public int width, height; /* the board size */
    public int info_timeout_turn = 30000; /* time for one turn in milliseconds */
    public int info_timeout_match = 1000000000; /* total time for a game */
    public int info_time_left = 1000000000; /* left time for a game */
    public int info_max_memory = 0; /* maximum memory in bytes, zero if unlimited */
    public int info_game_type = 1; /* 0:human opponent, 1:AI opponent, 2:tournament, 3:network tournament */
    public bool info_exact5 = false; /* false:five or more stones win, true:exactly five stones win */
    public bool info_renju = false; /* false:gomoku, true:renju */
    public bool info_continuous = false; /* false:single game, true:continuous */
    public int terminate; /* return from brain_turn when terminate>0 */
    public int start_time; /* tick count at the beginning of turn */
    public string dataFolder; /* folder for persistent files, can be null */

    /* you have to implement these functions */
    abstract public string brain_about { get; } /* copyright, version, homepage */
    abstract public void brain_init(); /* create the board and call Console.WriteLine("OK"); or Console.WriteLine("ERROR Maximal board size is .."); */
    abstract public void brain_restart(); /* delete old board, create new board, call Console.WriteLine("OK"); */
    abstract public void brain_turn(); /* choose your move and call do_mymove(x,y); 0<=x<width, 0<=y<height */
    abstract public void brain_my(int x, int y); /* put your move to the board */
    abstract public void brain_opponents(int x, int y); /* put opponent's move to the board */
    abstract public void brain_block(int x, int y); /* square [x,y] belongs to a winning line (when info_continuous is 1) */
    abstract public int brain_takeback(int x, int y); /* clear one square; return value: 0:success, 1:not supported, 2:error */
    abstract public void brain_end();  /* delete temporary files, free resources */
    virtual public void brain_eval(int x, int y) { } /* display evaluation of square [x,y] */

    protected CommandHandler cmdHandler;
    private string cmd;

    public string Cmd { get => cmd; set => cmd = value; }
    private AutoResetEvent event1;
    private ManualResetEvent event2;

    /** read a line from STDIN */
    public void get_line()
    {
        Console.WriteLine("DEBUG getline = " + (GomocupEngine.Instance.Cmd = Console.ReadLine()));
        if (GomocupEngine.Instance.Cmd == null)
            Environment.Exit(0);
    }

    /** send suggest */
    protected void suggest(int x, int y)
    {
        Console.WriteLine("SUGGEST {0},{1}", x, y);
    }

    /** write move to the pipe and update internal data structures */
    public void do_mymove(int x, int y)
    {
        brain_my(x, y);
        Console.WriteLine("{0},{1}", x, y);
    }

    /** main function for the working thread */
    private void threadLoop()
    {
        for (; ; )
        {
            GomocupEngine.Instance.event1.WaitOne();
            brain_turn();
            GomocupEngine.Instance.event2.Set();
        }
    }

    /** start thinking */
    public void turn()
    {
        GomocupEngine.Instance.terminate = 0;
        GomocupEngine.Instance.event2.Reset();
        GomocupEngine.Instance.event1.Set();
    }

    /** stop thinking */
    public void stop()
    {
        GomocupEngine.Instance.terminate = 1;
        GomocupEngine.Instance.event2.WaitOne();
    }

    public void start()
    {
        GomocupEngine.Instance.start_time = Environment.TickCount;
        stop();
        if (GomocupEngine.Instance.width == 0)
        {
            GomocupEngine.Instance.width = GomocupEngine.Instance.height = 20;
            brain_init();
        }
    }

    /** main function for AI console application  */
    public void main()
    {
        try
        {
            int dummy = Console.WindowHeight;
            //ERROR, process started from the Explorer or command line
            Console.WriteLine("MESSAGE Gomoku AI should not be started directly. Please install gomoku manager (http://sourceforge.net/projects/piskvork). Then enter path to this exe file in players settings.");
        }
        catch (System.IO.IOException)
        {
            //OK, process started from the Piskvork manager
        }

        GomocupEngine.Instance.event1 = new AutoResetEvent(false);
        new Thread(threadLoop).Start();
        GomocupEngine.Instance.event2 = new ManualResetEvent(true);
        for (; ; )
        {
            get_line();
            cmdHandler.DoCommand();
        }
    }

    static void Main(string[] args)
    {
        GomocupEngine.Instance.main();
    }
}