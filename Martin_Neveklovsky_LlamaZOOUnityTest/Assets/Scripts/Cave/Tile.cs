using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LlamaCave
{
    struct Tile
    {
        public int coordX;
        public int coordY;

        public Tile(int x, int y)
        {
            coordX = x;
            coordY = y;
        }
    }
}
