using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gomoku
{
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
}
