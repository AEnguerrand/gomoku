using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gomoku
{
    class CommandHandler
    {
        /** parse coordinates x,y */
        private bool parse_coord2(string param, out int x, out int y)
        {
            string[] p = param.Split(',');
            if (p.Length == 2 && int.TryParse(p[0], out x) && int.TryParse(p[1], out y) && x >= 0 && y >= 0)
                return true;
            x = y = 0;
            return false;
        }

        /** parse coordinates x,y */
        private bool parse_coord(string param, out int x, out int y)
        {
            return parse_coord2(param, out x, out y) && x < GomocupEngine.Instance.width && y < GomocupEngine.Instance.height;
        }

        /** parse coordinates x,y and player number z */
        private void parse_3int_chk(string param, out int x, out int y, out int z)
        {
            string[] p = param.Split(',');
            if (!(p.Length == 3 && int.TryParse(p[0], out x) && int.TryParse(p[1], out y) && int.TryParse(p[2], out z)
                && x >= 0 && y >= 0 && x < GomocupEngine.Instance.width && y < GomocupEngine.Instance.height))
                x = y = z = 0;
        }

        private static string get_cmd_param(string command, out string param)
        {
            Console.WriteLine("DEBUG a");
            param = "";
            Console.WriteLine("DEBUG b");
            int pos = command.IndexOf(' ');
            Console.WriteLine("DEBUG c");
            if (pos >= 0)
            {
                param = command.Substring(pos + 1).TrimStart(' ');
                command = command.Substring(0, pos);
            }
            Console.WriteLine("DEBUG d");
            return command.ToLower();
        }

        private void ParseInfo(string param)
        {
            switch (get_cmd_param(param, out string info))
            {
                case "max_memory":
                    int.TryParse(info, out GomocupEngine.Instance.info_max_memory);
                    break;
                case "timeout_match":
                    int.TryParse(info, out GomocupEngine.Instance.info_timeout_match);
                    break;
                case "timeout_turn":
                    int.TryParse(info, out GomocupEngine.Instance.info_timeout_turn);
                    break;
                case "time_left":
                    int.TryParse(info, out GomocupEngine.Instance.info_time_left);
                    break;
                case "game_type":
                    int.TryParse(info, out GomocupEngine.Instance.info_game_type);
                    break;
                case "rule":
                    if (int.TryParse(info, out int e))
                    {
                        GomocupEngine.Instance.info_exact5 = (e & 1) != 0;
                        GomocupEngine.Instance.info_continuous = (e & 2) != 0;
                        GomocupEngine.Instance.info_renju = (e & 4) != 0;
                    }
                    break;
                case "folder":
                    GomocupEngine.Instance.dataFolder = info;
                    break;
                case "evaluate":
                    if (parse_coord(info, out int x, out int y))
                        GomocupEngine.Instance.brain_eval(x, y);
                    break;
                    /* unknown info is ignored */
            }
        }

        private void ParseStart(string param)
        {
            if (!int.TryParse(param, out GomocupEngine.Instance.width) || GomocupEngine.Instance.width < 5)
            {
                GomocupEngine.Instance.width = 0;
                Console.WriteLine("ERROR bad START parameter");
            }
            else
            {
                GomocupEngine.Instance.height = GomocupEngine.Instance.width;
                GomocupEngine.Instance.start();
                GomocupEngine.Instance.brain_init();
            }
        }

        private void ParseRectstart(string param)
        {
            if (!parse_coord2(param, out GomocupEngine.Instance.width, out GomocupEngine.Instance.height) || GomocupEngine.Instance.width < 5 || GomocupEngine.Instance.height < 5)
            {
                GomocupEngine.Instance.width = GomocupEngine.Instance.height = 0;
                Console.WriteLine("ERROR bad RECTSTART parameters");
            }
            else
            {
                GomocupEngine.Instance.start();
                GomocupEngine.Instance.brain_init();
            }
        }

        private void ParseRestart(string param)
        {
            GomocupEngine.Instance.start();
            GomocupEngine.Instance.brain_restart();
        }

        private void ParseTurn(string param)
        {
            GomocupEngine.Instance.start();
            if (!parse_coord(param, out int x, out int y))
            {
                Console.WriteLine("ERROR bad coordinates");
            }
            else
            {
                GomocupEngine.Instance.brain_opponents(x, y);
                GomocupEngine.Instance.turn();
            }
        }

        private void ParsePlay(string param)
        {
            GomocupEngine.Instance.start();
            if (!parse_coord(param, out int x, out int y))
            {
                Console.WriteLine("ERROR bad coordinates");
            }
            else
            {
                GomocupEngine.Instance.do_mymove(x, y);
            }
        }

        private void ParseBegin(string param)
        {
            GomocupEngine.Instance.start();
            GomocupEngine.Instance.turn();
        }

        private void ParseAbout(string param)
        {
            Console.WriteLine(GomocupEngine.Instance.brain_about);
        }

        private void ParseEnd(string param)
        {
            GomocupEngine.Instance.stop();
            GomocupEngine.Instance.brain_end();
            Environment.Exit(0);
        }

        private void ParseBoard(string param)
        {
            GomocupEngine.Instance.start();
            for (; ; ) /* fill the whole board */
            {
                GomocupEngine.Instance.get_line();
                parse_3int_chk(GomocupEngine.Instance.cmd, out int x, out int y, out int who);
                Console.WriteLine("DEBUG x: {0} y: {1} who: {2}", x, y, who);
                if (who == 1)
                    GomocupEngine.Instance.brain_my(x, y);
                else if (who == 2)
                    GomocupEngine.Instance.brain_opponents(x, y);
                else if (who == 3)
                    GomocupEngine.Instance.brain_block(x, y);
                else
                {
                    if (!GomocupEngine.Instance.cmd.Equals("done", StringComparison.InvariantCultureIgnoreCase))
                        Console.WriteLine("ERROR x,y,who or DONE expected after BOARD");
                    break;
                }
            }
            GomocupEngine.Instance.turn();
        }

        private void ParseTakeback(string param)
        {
            GomocupEngine.Instance.start();
            string t = "ERROR bad coordinates";
            if (parse_coord(param, out int x, out int y))
            {
                int e = GomocupEngine.Instance.brain_takeback(x, y);
                if (e == 0)
                    t = "OK";
                else if (e == 1)
                    t = "UNKNOWN";
            }
            Console.WriteLine(t);
        }

        /** do command cmd */
        public void DoCommand()
        {

            Console.WriteLine("DEBUG In");
            var CommandFunc = new Dictionary<string, ParamFunc>
            {
                { "info", ParseInfo },
                { "start", ParseStart },
                { "rectstart", ParseRectstart },
                { "restart", ParseRestart },
                { "turn", ParseTurn },
                { "play", ParsePlay },
                { "begin", ParseBegin },
                { "about", ParseAbout },
                { "end", ParseEnd },
                { "board", ParseBoard },
                { "takeback", ParseTakeback }
            };
            Console.WriteLine("DEBUG 1");

            string command = get_cmd_param(GomocupEngine.Instance.cmd, out string param);
            Console.WriteLine("DEBUG 2");

            foreach (var it in CommandFunc)
            {
                Console.WriteLine("DEBUG - " + it.Key);

                if (it.Key == command)
                {
                    Console.WriteLine("DEBUG key = " + it.Key);
                    it.Value(param);
                    return;
                }
            }
            Console.WriteLine("DEBUG 3");

            Console.WriteLine("UNKNOWN command");
        }
    }
}
