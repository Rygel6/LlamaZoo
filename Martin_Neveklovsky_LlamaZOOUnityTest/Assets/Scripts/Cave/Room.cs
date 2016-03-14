using UnityEngine;
using System;
using System.Collections.Generic;

namespace LlamaCave
{
    class Room : IComparable<Room>
    {
        public List<Tile> tiles;
        public List<Tile> edgeTiles;
        public List<Tile> innerTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public int distanceFromMainRoom;

        public Room()
        {
            distanceFromMainRoom = -1;
        }

        public Room(List<Tile> roomTiles, int[,] cave)
        {
            distanceFromMainRoom = -1;
            tiles = roomTiles;
            roomSize = tiles.Count;

            connectedRooms = new List<Room>();

            edgeTiles = new List<Tile>();
            innerTiles = new List<Tile>();
            foreach (Tile tile in tiles)
            {
                bool isEdgeTile = false;
                if (tile.coordX == 0 || tile.coordX == cave.GetLength(0) - 1 || tile.coordY == 0 || tile.coordY == cave.GetLength(1) - 1)
                {
                    isEdgeTile = true;
                }
                else
                {
                    for (int x = tile.coordX - 1; x <= tile.coordX + 1; x++)
                    {
                        for (int y = tile.coordY - 1; y <= tile.coordY + 1; y++)
                        {
                            if (x == tile.coordX || y == tile.coordY)
                            {
                                if (cave[x, y] == 0)
                                {
                                    isEdgeTile = true;
                                    break;
                                }
                            }
                        }

                        if (isEdgeTile)
                        {
                            break;
                        }
                    }
                }

                if (isEdgeTile)
                {
                    edgeTiles.Add(tile);
                }
                else
                {
                    innerTiles.Add(tile);
                }
            }
        }

        public void SetAccessibleFromMainRoom(int connectedRoomDistFromMainRoom)
        {
            if (!IsAccessibleFromMainRoom() && connectedRoomDistFromMainRoom >= 0)
            {
                distanceFromMainRoom = connectedRoomDistFromMainRoom + 1;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom(distanceFromMainRoom);
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.IsAccessibleFromMainRoom())
            {
                roomB.SetAccessibleFromMainRoom(roomA.distanceFromMainRoom);
            }
            else if (roomB.IsAccessibleFromMainRoom())
            {
                roomA.SetAccessibleFromMainRoom(roomB.distanceFromMainRoom);
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }

        public bool IsAccessibleFromMainRoom()
        {
            return distanceFromMainRoom >= 0;
        }

        public bool IsMainRoom()
        {
            return distanceFromMainRoom == 0;
        }
    }
}
