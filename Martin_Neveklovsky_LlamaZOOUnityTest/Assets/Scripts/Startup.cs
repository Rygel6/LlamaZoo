using UnityEngine;
using System.Collections;

namespace LlamaCave
{

    public class Startup : MonoBehaviour
    {
        GameObject cave;
        // Use this for initialization
        void Start()
        {
            cave = Instantiate(Resources.Load("Cave")) as GameObject;
        }
    }
}
