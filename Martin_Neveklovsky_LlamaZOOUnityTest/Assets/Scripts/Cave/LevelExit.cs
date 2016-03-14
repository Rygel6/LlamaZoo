using UnityEngine;
using System.Collections;


namespace LlamaCave
{
    public class LevelExit : MonoBehaviour
    {
        bool isActivated;

        LevelExit()
        {
            DelegatesAndEvents.onCaveCreated += this.CaveCreated;
            isActivated = false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (isActivated)
            {
                if (other.gameObject.CompareTag("Player"))
                {
                    DelegatesAndEvents.PlayerReachedExit();
                }
            }
        }
        
        void CaveCreated()
        {
            isActivated = true;
        }
    }
}