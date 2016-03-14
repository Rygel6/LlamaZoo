using UnityEngine;
using System.Collections.Generic;

namespace LlamaCave
{
    class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }
}
