using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LlamaCave
{
    class DelegatesAndEvents
    {

        public delegate void CaveEventHandler();

        public static event CaveEventHandler onPlayerReachedExit;
        public static event CaveEventHandler onCaveCreated;


        public static void PlayerReachedExit()
        {
            if (onPlayerReachedExit != null)
                onPlayerReachedExit();
        }

        public static void NewCaveCreated()
        {
            if (onCaveCreated != null)
                onCaveCreated();
        }
    }
}
