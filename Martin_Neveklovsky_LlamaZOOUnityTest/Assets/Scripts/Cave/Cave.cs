using UnityEngine;
using System.Collections.Generic;

namespace LlamaCave
{
    class Cave : MonoBehaviour
    {
        public int[,] mapTiles;
        public GameObject player;

        public GameObject followCamera;
        public GameObject miniMapCamera;
        public GameObject entrance;
        public GameObject exit;

        public List<GameObject> treasures;
        public List<GameObject> traps;

        bool wasSaved;

        void Start()
        {
            //Instantiate prefabs needed for the cave
            player = Instantiate(Resources.Load("Player")) as GameObject;
            entrance = Instantiate(Resources.Load("Entrance")) as GameObject;
            exit = Instantiate(Resources.Load("Exit")) as GameObject;

            followCamera = Instantiate(Resources.Load("FollowCamera")) as GameObject;
            followCamera.GetComponent<CamFollow>().target = player.transform;
            miniMapCamera = Instantiate(Resources.Load("MiniMapCamera")) as GameObject;

            CameraSwitcher camSwitcherScript = GetComponent<CameraSwitcher>();
            if (camSwitcherScript)
            {
                GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                camSwitcherScript.levelCam = mainCamera.GetComponent<Camera>();
                camSwitcherScript.followCam = followCamera.GetComponent<Camera>();
            }
            CaveGenerator caveGenerator = GetComponent<CaveGenerator>();
            caveGenerator.OnCaveInitialized();
        }
        
        public void OnNewCaveGenerated(int[,] newMap)
        {
            wasSaved = false;
            mapTiles = newMap;

            foreach(GameObject treasureGO in treasures)
            {
                treasureGO.SetActive(false);
            }

            CaveGenerator caveGenerator = GetComponent<CaveGenerator>();
            float desiredZPosition = -0.15f;
            int initializedTreasures = 0;

            for (int x = 0; x < mapTiles.GetLength(0); x++)
            {
                for(int y = 0; y < mapTiles.GetLength(1); y++)
                {
                    int tyleState = mapTiles[x, y];

                    if(tyleState == 2) // this is this level's entrance tile
                    {
                        Tile entranceTile = new Tile(x, y);
                        Vector3 entrancePositionIn2DSpace = caveGenerator.TileTo2DWorldPoint(entranceTile, mapTiles.GetLength(0), mapTiles.GetLength(1), desiredZPosition);

                        entrance.transform.position = entrancePositionIn2DSpace;
                        entrance.transform.rotation = Quaternion.identity;
                        
                        player.transform.position = entrancePositionIn2DSpace;
                        player.transform.rotation = Quaternion.identity;
                    }
                    else if(tyleState == 3) //this is this level's exit Tile
                    {
                        Tile exitTile = new Tile(x, y);
                        Vector3 exitPositionIn2DSpace = caveGenerator.TileTo2DWorldPoint(exitTile, mapTiles.GetLength(0), mapTiles.GetLength(1), desiredZPosition);
                        
                        exit.transform.position = exitPositionIn2DSpace;
                        exit.transform.rotation = Quaternion.identity;

                    }
                    else if(tyleState == 4) // this is a treasure tile
                    {
                        Tile treasureTile = new Tile(x, y);
                        Vector3 treasurePositionIn2DSpace = caveGenerator.TileTo2DWorldPoint(treasureTile, mapTiles.GetLength(0), mapTiles.GetLength(1), desiredZPosition);
                        if (initializedTreasures >= treasures.Count)
                        {
                            GameObject treasure = Instantiate(Resources.Load("Treasure")) as GameObject;
                            treasures.Add(treasure);
                        }

                        treasures[initializedTreasures].SetActive(true);
                        treasures[initializedTreasures].transform.position = treasurePositionIn2DSpace;
                        treasures[initializedTreasures].transform.rotation = Quaternion.identity;

                        initializedTreasures++;
                    }
                }
            }

            DelegatesAndEvents.NewCaveCreated();
        }

    }
}
