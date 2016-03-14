using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LlamaCave
{
    class MeshGenerator : MonoBehaviour
    {
        public SquareGrid squareGrid;
        public MeshFilter walls;
        public MeshFilter cave;

        public bool is2D = true;

        List<Vector3> vertices;
        List<int> triangles;
        List<List<int>> outlines = new List<List<int>>();
        HashSet<int> checkedVertices = new HashSet<int>();

        Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();

        public void GenerateMesh(int[,] map, float squareSize)
        {
            triangleDictionary.Clear();
            outlines.Clear();
            checkedVertices.Clear();

            squareGrid = new SquareGrid(map, squareSize);
            vertices = new List<Vector3>();
            triangles = new List<int>();
            for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
            {
                for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
                {
                    TriangulateSquare(squareGrid.squares[x, y]);
                }
            }

            Mesh mesh = new Mesh();
            cave.mesh = mesh;

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            Vector2[] uvs = new Vector2[vertices.Count];

            int tileAmountX = map.GetLength(0) / 2;
            int tileAmountY = map.GetLength(1) / 2;
            for (int i = 0; i < vertices.Count; i++)
            {
                float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmountX;
                float percentY = Mathf.InverseLerp(-map.GetLength(1) / 2 * squareSize, map.GetLength(1) / 2 * squareSize, vertices[i].z) * tileAmountY;

                uvs[i] = new Vector2(percentX, percentY);
            }

            mesh.uv = uvs;

            if (is2D)
            {
                Generate2DColliders();
            }
            else
            {
                CreateWallMesh();
            }
        }

        void Generate2DColliders()
        {
            EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
            for (int i = 0; i < currentColliders.Length; i++)
            {
                Destroy(currentColliders[i]);
            }

            CalculateMeshOutlines();
            foreach (List<int> outline in outlines)
            {
                EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
                Vector2[] edgePoints = new Vector2[outline.Count];

                for (int i = 0; i < outline.Count; i++)
                {
                    edgePoints[i] = new Vector2(vertices[outline[i]].x, vertices[outline[i]].z);
                }
                edgeCollider.points = edgePoints;
            }
        }

        void CreateWallMesh()
        {
            CalculateMeshOutlines();

            List<Vector3> wallVertices = new List<Vector3>();
            List<int> wallTriangles = new List<int>();
            Mesh wallMesh = new Mesh();
            float wallHeight = 5;

            foreach (List<int> outline in outlines)
            {
                for (int i = 0; i < outline.Count - 1; i++)
                {
                    int startIndex = wallVertices.Count;
                    wallVertices.Add(vertices[outline[i]]); //left
                    wallVertices.Add(vertices[outline[i + 1]]); //right
                    wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); //bottom left
                    wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); //bottom right

                    wallTriangles.Add(startIndex);
                    wallTriangles.Add(startIndex + 2);
                    wallTriangles.Add(startIndex + 3);

                    wallTriangles.Add(startIndex + 3);
                    wallTriangles.Add(startIndex + 1);
                    wallTriangles.Add(startIndex);
                }
            }

            wallMesh.vertices = wallVertices.ToArray();
            wallMesh.triangles = wallTriangles.ToArray();
            walls.mesh = wallMesh;

            MeshCollider wallCollider = walls.gameObject.GetComponent<MeshCollider>();
            wallCollider.sharedMesh = wallMesh;
        }

        void TriangulateSquare(Square square)
        {
            switch (square.configuration)
            {
                case 0:
                    break;

                //Only One Point Active
                case 1:
                    MeshFromPoints(square.left, square.bottom, square.bottomLeft);
                    break;
                case 2:
                    MeshFromPoints(square.bottomRight, square.bottom, square.right);
                    break;
                case 4:
                    MeshFromPoints(square.topRight, square.right, square.top);
                    break;
                case 8:
                    MeshFromPoints(square.topLeft, square.top, square.left);
                    break;

                //Two Point Active:
                case 3:
                    MeshFromPoints(square.right, square.bottomRight, square.bottomLeft, square.left);
                    break;
                case 6:
                    MeshFromPoints(square.top, square.topRight, square.bottomRight, square.bottom);
                    break;
                case 9:
                    MeshFromPoints(square.topLeft, square.top, square.bottom, square.bottomLeft);
                    break;
                case 12:
                    MeshFromPoints(square.topLeft, square.topRight, square.right, square.left);
                    break;
                case 10:
                    MeshFromPoints(square.topLeft, square.top, square.right, square.bottomRight, square.bottom, square.left);
                    break;

                //Three Points Active
                case 7:
                    MeshFromPoints(square.top, square.topRight, square.bottomRight, square.bottomLeft, square.left);
                    break;
                case 11:
                    MeshFromPoints(square.topLeft, square.top, square.right, square.bottomRight, square.bottomLeft);
                    break;
                case 13:
                    MeshFromPoints(square.topLeft, square.topRight, square.right, square.bottom, square.bottomLeft);
                    break;
                case 14:
                    MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottom, square.left);
                    break;

                //Four Points Active
                case 15:
                    MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                    checkedVertices.Add(square.topLeft.vertexIndex);
                    checkedVertices.Add(square.topRight.vertexIndex);
                    checkedVertices.Add(square.bottomRight.vertexIndex);
                    checkedVertices.Add(square.bottomLeft.vertexIndex);
                    break;
            }
        }

        void MeshFromPoints(params Node[] points)
        {
            AssignVertices(points);
            if (points.Length >= 3)
            {
                CreateTriangle(points[0], points[1], points[2]);
            }
            if (points.Length >= 4)
            {
                CreateTriangle(points[0], points[2], points[3]);
            }
            if (points.Length >= 5)
            {
                CreateTriangle(points[0], points[3], points[4]);
            }
            if (points.Length >= 6)
            {
                CreateTriangle(points[0], points[4], points[5]);
            }
        }

        void AssignVertices(Node[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].vertexIndex == -1)
                {
                    points[i].vertexIndex = vertices.Count;
                    vertices.Add(points[i].position);
                }
            }
        }

        void CreateTriangle(Node a, Node b, Node c)
        {
            triangles.Add(a.vertexIndex);
            triangles.Add(b.vertexIndex);
            triangles.Add(c.vertexIndex);

            Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
            AddTriangleToDictionary(triangle.vertexIndexA, triangle);
            AddTriangleToDictionary(triangle.vertexIndexB, triangle);
            AddTriangleToDictionary(triangle.vertexIndexC, triangle);
        }

        void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
        {
            if (triangleDictionary.ContainsKey(vertexIndexKey))
            {
                triangleDictionary[vertexIndexKey].Add(triangle);
            }
            else
            {
                List<Triangle> triangleList = new List<Triangle>();
                triangleList.Add(triangle);
                triangleDictionary.Add(vertexIndexKey, triangleList);
            }
        }

        void CalculateMeshOutlines()
        {
            for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
            {
                if (!checkedVertices.Contains(vertexIndex))
                {
                    int newOutlineVertexIndex = GetConnectedOutlineVertex(vertexIndex);
                    if (newOutlineVertexIndex != -1)
                    {
                        checkedVertices.Add(vertexIndex);

                        List<int> newOutline = new List<int>();
                        newOutline.Add(vertexIndex);
                        outlines.Add(newOutline);
                        FollowOutline(newOutlineVertexIndex, outlines.Count - 1);
                        outlines[outlines.Count - 1].Add(vertexIndex);
                    }
                }

            }
        }

        void FollowOutline(int vertexIndex, int outlineIndex)
        {
            outlines[outlineIndex].Add(vertexIndex);
            checkedVertices.Add(vertexIndex);

            int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
            if (nextVertexIndex != -1)
            {
                FollowOutline(nextVertexIndex, outlineIndex);
            }
        }

        int GetConnectedOutlineVertex(int vertexIndex)
        {
            List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];
            for (int i = 0; i < trianglesContainingVertex.Count; i++)
            {
                Triangle triangle = trianglesContainingVertex[i];
                for (int j = 0; j < 3; j++)
                {
                    int vertexB = triangle[j];
                    if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                    {
                        if (IsOutlineEdge(vertexIndex, vertexB))
                        {
                            return vertexB;
                        }
                    }
                }
            }
            return -1;
        }

        bool IsOutlineEdge(int vertexA, int vertexB)
        {
            List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
            int sharedTriangleCount = 0;
            for (int i = 0; i < trianglesContainingVertexA.Count; i++)
            {
                if (trianglesContainingVertexA[i].Contains(vertexB))
                {
                    sharedTriangleCount++;
                    if (sharedTriangleCount > 1)
                    {
                        break;
                    }
                }
            }
            return sharedTriangleCount == 1;
        }
    }
}
