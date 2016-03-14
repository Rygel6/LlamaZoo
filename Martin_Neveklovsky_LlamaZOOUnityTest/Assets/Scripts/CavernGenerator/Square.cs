using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LlamaCave
{
    class Square
    {
        public ControlNode topLeft, topRight, bottomLeft, bottomRight;
        public Node top, left, bottom, right;
        public int configuration;

        public Square(ControlNode _TL, ControlNode _TR, ControlNode _BR, ControlNode _BL)
        {
            topLeft = _TL;
            topRight = _TR;
            bottomLeft = _BL;
            bottomRight = _BR;

            top = topLeft.right;
            left = bottomLeft.above;
            bottom = bottomLeft.right;
            right = bottomRight.above;

            if (topLeft.active)
            {
                configuration += 8;
            }

            if (topRight.active)
            {
                configuration += 4;
            }

            if (bottomRight.active)
            {
                configuration += 2;
            }

            if (bottomLeft.active)
            {
                configuration += 1;
            }
        }
    }
}
