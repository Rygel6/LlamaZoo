using UnityEngine;
using System;
using System.Collections.Generic;

namespace LlamaCave
{
    class CaveGenerator : MonoBehaviour
    {
        public Cave cave;

        public int width = 150;
        public int height = 75;
        public int borderSize = 40;

        public int generationCount = 5;         //number of times we run our initial grid through the cellular automata function.

        public int underpopulationValue = 2;   //if number of live neighbours is below this amount, Live tiles become Dead
        public int overpopulationValue = 4;    //if number of live neighbours is above this amount, Live tiles become Dead
        public int birthValue = 3;        //if number of live neighbours is exactly this amount, Dead tiles become Live

        public string seed;
        public bool generateNewSeed = true;

        public bool isFullyEnclosed = true; //bool that controls if tiles on the edges are always have "dead" (walls) or if it can be "live" (walkable tiles)

        [Range(0, 100)]
        public int chanceToStartAlive = 33;

        int[,] map;
        System.Random random;

        void Start()
        {
            DelegatesAndEvents.onPlayerReachedExit += this.PlayerReachedExit;
            GenerateCave();
        }

        public void PlayerReachedExit()
        {
            GenerateCave();
        }

        public void OnCaveInitialized()
        {
            GenerateCave();
        }

        void GenerateCave()
        {
            map = new int[width, height];
            RandomlyFillCave();

            for (int i = 0; i < generationCount; i++)
            {
                map = CalculateNextTileGeneration(map);
            }

            List<Room> rooms = CleanUpCave();

            PopulateCaveWithProps(rooms);
            cave.mapTiles = map;

            int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

            for (int x = 0; x < borderedMap.GetLength(0); x++)
            {
                for (int y = 0; y < borderedMap.GetLength(1); y++)
                {
                    if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                    {
                        borderedMap[x, y] = map[x - borderSize, y - borderSize];
                    }
                    else
                    {
                        borderedMap[x, y] = 0;
                    }
                }
            }

            MeshGenerator navMeshGen = GetComponent<MeshGenerator>();
            navMeshGen.GenerateMesh(borderedMap, 1f);
            
            cave.OnNewCaveGenerated(borderedMap);
        }

        void RandomlyFillCave()
        {
            if (generateNewSeed)
            {
                seed = System.DateTime.Now.Ticks.ToString();
            }

            random = new System.Random(seed.GetHashCode());

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool isEdgeTile = IsEdgeTileCoord(x, y);
                    if (isFullyEnclosed && isEdgeTile)
                    {
                        map[x, y] = 0;
                    }
                    else
                    {
                        map[x, y] = (random.Next(0, 100) < chanceToStartAlive) ? 1 : 0;
                    }
                }
            }
        }

        List<Room> CleanUpCave()
        {

            int minWallSectionSize = 6;
            RemoveSmallWallSections(minWallSectionSize); //Remove any small "pillars" or wall sections that are smaller than what we'd like, to minimize the occurence of tiny pillars in rooms.

            int minRoomSize = 70;
            List<Room> rooms = RemoveSmallRooms(minRoomSize); //Remove any small rooms that are smaller than our smallest desired room.

            rooms.Sort();
            rooms[0].distanceFromMainRoom = 0;
 
            ConnectClosestRooms(rooms, false);

            //currently my "find outlines" algorithm has a small bug in it where if you have a diagonal wall that is 1 grid square wide
            //where two wall tiles are touching corners, but don't share a common neighbour that is in contact with their edges, then it 
            //won't calculate the outlines properly in that section and you get some invisible colliders that don't match the visible 
            //geometry. So for now the quick fix is to adjust these walls to be slightly thicker at this area.
            AdjustThinDiagonalWalls();

            return rooms;
        }

        void RemoveSmallWallSections(int minWallSectionSize)
        {
            List<List<Tile>> wallSections = GetRooms(0);
            foreach (List<Tile> section in wallSections)
            {
                if (section.Count < minWallSectionSize)
                {
                    foreach (Tile tile in section)
                    {
                        map[tile.coordX, tile.coordY] = 1;
                    }
                }
            }
        }

        List<Room> RemoveSmallRooms(int minRoomSize)
        {
            List<List<Tile>> rooms = GetRooms(1);
            List<Room> remainingRooms = new List<Room>();
            foreach (List<Tile> room in rooms)
            {
                if (room.Count < minRoomSize)
                {
                    foreach (Tile tile in room)
                    {
                        map[tile.coordX, tile.coordY] = 0;
                    }
                }
                else
                {
                    remainingRooms.Add(new Room(room, map));
                }
            }

            return remainingRooms;
        }

        void AdjustThinDiagonalWalls()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!IsEdgeTileCoord(x, y))
                    {
                        if (map[x, y] == 1)
                        {
                            if (map[x - 1, y] == 0 && map[x, y + 1] == 0 && map[x - 1, y + 1] == 1)
                            {
                                map[x - 1, y + 1] = 0;
                            }

                            if (map[x + 1, y] == 0 && map[x, y + 1] == 0 && map[x + 1, y + 1] == 1)
                            {
                                map[x + 1, y + 1] = 0;
                            }

                            if (map[x + 1, y] == 0 && map[x, y - 1] == 0 && map[x + 1, y - 1] == 1)
                            {
                                map[x + 1, y - 1] = 0;
                            }

                            if (map[x - 1, y] == 0 && map[x, y - 1] == 0 && map[x - 1, y - 1] == 1)
                            {
                                map[x - 1, y - 1] = 0;
                            }
                        }
                    }
                }
            }
        }

        void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom)
        {
            List<Room> roomListA = new List<Room>();
            List<Room> roomListB = new List<Room>();

            if (forceAccessibilityFromMainRoom)
            {
                foreach (Room room in allRooms)
                {
                    if (room.IsAccessibleFromMainRoom())
                    {
                        roomListB.Add(room);
                    }
                    else
                    {
                        roomListA.Add(room);
                    }
                }
            }
            else
            {
                roomListA = allRooms;
                roomListB = allRooms;
            }

            int bestDistance = 0;
            Tile bestTileA = new Tile();
            Tile bestTileB = new Tile();
            Room bestRoomA = new Room();
            Room bestRoomB = new Room();
            bool possibleConnectionFound = false;

            foreach (Room roomA in roomListA)
            {
                if (!forceAccessibilityFromMainRoom)
                {
                    possibleConnectionFound = false;
                    if (roomA.connectedRooms.Count > 0)
                    {
                        continue;
                    }
                }

                foreach (Room roomB in roomListB)
                {
                    if (roomA == roomB || roomA.IsConnected(roomB))
                    {
                        continue;
                    }

                    for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                    {
                        for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                        {
                            Tile tileA = roomA.edgeTiles[tileIndexA];
                            Tile tileB = roomB.edgeTiles[tileIndexB];
                            int distanceBetweenRooms = (int)(Mathf.Pow(tileA.coordX - tileB.coordX, 2) + Mathf.Pow(tileA.coordY - tileB.coordY, 2));

                            if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                            {
                                bestDistance = distanceBetweenRooms;
                                possibleConnectionFound = true;
                                bestTileA = tileA;
                                bestTileB = tileB;
                                bestRoomA = roomA;
                                bestRoomB = roomB;
                            }
                        }
                    }
                }

                if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
                {
                    CreateHallway(bestRoomA, bestRoomB, bestTileA, bestTileB);
                }
            }

            if (possibleConnectionFound && forceAccessibilityFromMainRoom)
            {
                CreateHallway(bestRoomA, bestRoomB, bestTileA, bestTileB);
                ConnectClosestRooms(allRooms, true);
            }

            if (!forceAccessibilityFromMainRoom)
            {
                ConnectClosestRooms(allRooms, true);
            }
        }

        void CreateHallway(Room roomA, Room roomB, Tile tileA, Tile tileB)
        {
            Room.ConnectRooms(roomA, roomB);

            List<Tile> line = GetLine(tileA, tileB);
            foreach (Tile tile in line)
            {
                ExcavateCircle(tile, 2);
            }
        }

        void ExcavateCircle(Tile tile, int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        int drawX = tile.coordX + x;
                        int drawY = tile.coordY + y;
                        if (IsValidTileCoord(drawX, drawY))
                        {
                            map[drawX, drawY] = 1;
                        }
                    }
                }
            }
        }


        void PopulateCaveWithProps(List<Room> rooms)
        {
            bool wasEntrancePlaced = false;
            bool wasExitPlaced = false;

            int greatestDistanceFromMainRoom = 0;
            foreach (Room room in rooms)
            {
                if(room.distanceFromMainRoom > greatestDistanceFromMainRoom)
                {
                    greatestDistanceFromMainRoom = room.distanceFromMainRoom;
                }
            }

            foreach (Room room in rooms)
            {

                if (room.IsMainRoom() && !wasEntrancePlaced)
                {
                    int entranceTileIndex = random.Next(0, room.innerTiles.Count-1);
                    map[room.innerTiles[entranceTileIndex].coordX, room.innerTiles[entranceTileIndex].coordY] = 2;
                    wasEntrancePlaced = true;
                }
                else
                {
                    if(room.distanceFromMainRoom == greatestDistanceFromMainRoom && !wasExitPlaced)
                    {
                        int exitTileIndex = random.Next(0, room.innerTiles.Count - 1);
                        map[room.innerTiles[exitTileIndex].coordX, room.innerTiles[exitTileIndex].coordY] = 3;
                        wasExitPlaced = true;
                    }
                }

                int validLiveTileCount = 3; //place a treasure on any tyle that only has 3 open spaces around it (this is to keep the treasures only appearing in small alcoves)
                int treasurePlacementValue = 0;//controls how frequently we place a treasure in the appropriately "secluded" tile
                int treasurePlacementThrottle = 3;
                foreach (Tile tile in room.edgeTiles)
                {
                    int adjacentLiveTileCount = GetAdjacentLiveTileCount(map, tile.coordX, tile.coordY);
                    if (adjacentLiveTileCount == validLiveTileCount) 
                    {
                        if (treasurePlacementValue == 0)
                        {
                            map[tile.coordX, tile.coordY] = 4;
                        }

                        treasurePlacementValue++;
                        if(treasurePlacementValue >= treasurePlacementThrottle)
                        {
                            treasurePlacementValue = 0;
                        }
                    }
                }
            }
        }

        List<Tile> GetLine(Tile from, Tile to)
        {
            List<Tile> line = new List<Tile>();

            int x = from.coordX;
            int y = from.coordY;

            int dx = to.coordX - from.coordX;
            int dy = to.coordY - from.coordY;

            bool inverted = false;
            int step = Math.Sign(dx);
            int gradientStep = Math.Sign(dy);

            int longest = Mathf.Abs(dx);
            int shortest = Mathf.Abs(dy);
            if (longest < shortest)
            {
                inverted = true;
                longest = Mathf.Abs(dy);
                shortest = Mathf.Abs(dx);

                step = Math.Sign(dy);
                gradientStep = Math.Sign(dx);
            }

            int gradientAccumulation = longest / 2;
            for (int i = 0; i < longest; i++)
            {
                line.Add(new Tile(x, y));
                if (inverted)
                {
                    y += step;
                }
                else
                {
                    x += step;
                }

                gradientAccumulation += shortest;
                if (gradientAccumulation >= longest)
                {
                    if (inverted)
                    {
                        x += gradientStep;
                    }
                    else
                    {
                        y += gradientStep;
                    }
                    gradientAccumulation -= longest;
                }
            }

            return line;
        }

        public Vector3 TileTo2DWorldPoint(Tile tile, float desiredZPosition)
        {
            return TileTo2DWorldPoint(tile, width, height, desiredZPosition);
        }

        public Vector3 TileTo2DWorldPoint(Tile tile, int gridWidth, int gridHeight, float desiredZPosition)
        {
            Vector3 worldPoint = TileToWorldPoint(tile, gridWidth, gridHeight);
            return new Vector3(worldPoint.x, worldPoint.z, desiredZPosition);
        }

        public Vector3 TileToWorldPoint(Tile tile)
        {
            return TileToWorldPoint(tile, width, height);
        }

        public Vector3 TileToWorldPoint(Tile tile, int gridWidth, int gridHeight)
        {
            return new Vector3(-gridWidth / 2 + .5f + tile.coordX, 0.05f, -gridHeight / 2 + .5f + tile.coordY);
        }

        List<List<Tile>> GetRooms(int tileType)
        {
            List<List<Tile>> rooms = new List<List<Tile>>();
            int[,] caveFlags = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (caveFlags[x, y] == 0 && map[x, y] == tileType)
                    {
                        List<Tile> newRoom = GetRoomTiles(x, y);
                        rooms.Add(newRoom);

                        foreach (Tile tile in newRoom)
                        {
                            caveFlags[tile.coordX, tile.coordY] = 1;
                        }
                    }
                }
            }

            return rooms;
        }

        List<Tile> GetRoomTiles(int startCoordX, int startCoordY)
        {
            List<Tile> roomTiles = new List<Tile>();
            int[,] caveFlags = new int[width, height];
            int tileType = map[startCoordX, startCoordY];

            Queue<Tile> queue = new Queue<Tile>();
            queue.Enqueue(new Tile(startCoordX, startCoordY));
            caveFlags[startCoordX, startCoordY] = 1;

            while (queue.Count > 0)
            {
                Tile tile = queue.Dequeue();
                roomTiles.Add(tile);

                for (int x = tile.coordX - 1; x <= tile.coordX + 1; x++)
                {
                    for (int y = tile.coordY - 1; y <= tile.coordY + 1; y++)
                    {
                        if (IsValidTileCoord(x, y) && (x == tile.coordX || y == tile.coordY))
                        {
                            if (caveFlags[x, y] == 0 && map[x, y] == tileType)
                            {
                                caveFlags[x, y] = 1;
                                queue.Enqueue(new Tile(x, y));
                            }
                        }
                    }
                }
            }

            return roomTiles;
        }

        bool IsValidTileCoord(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        bool IsEdgeTileCoord(int x, int y)
        {
            return (x == 0 || x == (width - 1) || y == 0 || y == (height - 1));
        }

        int[,] CalculateNextTileGeneration(int[,] oldCave)
        {
            int[,] newCave = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int adjacentLiveTileCount = GetAdjacentLiveTileCount(oldCave, x, y);
                    bool shouldBeAlive = GetShouldTileBeAlive(oldCave[x, y], adjacentLiveTileCount);

                    if (shouldBeAlive)
                    {
                        newCave[x, y] = 1;
                    }
                    else
                    {
                        newCave[x, y] = 0;
                    }
                }
            }

            return newCave;
        }

        int GetAdjacentLiveTileCount(int[,] oldCave, int x, int y)
        {
            int liveCount = 0;
            for (int offsetX = -1; offsetX < 2; offsetX++)
            {
                for (int offsetY = -1; offsetY < 2; offsetY++)
                {
                    int finalCoordX = x + offsetX;
                    int finalCoordY = y + offsetY;

                    if (offsetX == 0 && offsetY == 0)
                    {
                        //If we're looking at the input coordinate, do nothing, as we don't want to count the input coord
                    }
                    else if (!IsValidTileCoord(finalCoordX, finalCoordY))
                    {
                        //If we're currently looking at a coordinate that's off the grid, count it as a dead tile
                        //liveCount++;
                    }
                    else if (oldCave[finalCoordX, finalCoordY] >= 1)
                    {
                        //If the coord we're looking at is valid and alive, increment the liveCount
                        liveCount++;
                    }
                }
            }
            return liveCount;
        }

        bool GetShouldTileBeAlive(int tileState, int adjacentLiveCount)
        {
            bool shouldBeAlive = false;

            if (tileState == 1)
            {
                if (adjacentLiveCount <= underpopulationValue)
                {
                    shouldBeAlive = false;
                }
                else
                {
                    shouldBeAlive = true;
                }
            }
            else if (tileState == 0)
            {
                if (adjacentLiveCount > birthValue)
                {
                    shouldBeAlive = true;
                }
                else
                {
                    shouldBeAlive = false;
                }

            }

            return shouldBeAlive;
        }
    }
}