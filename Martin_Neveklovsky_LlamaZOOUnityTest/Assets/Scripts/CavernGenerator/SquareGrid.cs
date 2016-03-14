using UnityEngine;
using System.Collections.Generic;

namespace LlamaCave
{
    class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] tiles, float squareSize)
        {
            int tileCountX = tiles.GetLength(0);
            int tileCountY = tiles.GetLength(1);
            float caveWidth = tileCountX * squareSize;
            float caveHeight = tileCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[tileCountX, tileCountY];
            for (int x = 0; x < tileCountX; x++)
            {
                for (int y = 0; y < tileCountY; y++)
                {
                    Vector3 pos = new Vector3(-caveWidth / 2 + x * squareSize + squareSize / 2, 0, -caveHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, tiles[x, y] == 0, squareSize);
                }
            }
            squares = new Square[tileCountX - 1, tileCountY - 1];
            for (int x = 0; x < tileCountX - 1; x++)
            {
                for (int y = 0; y < tileCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }

    }
}
