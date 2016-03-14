using UnityEngine;
using System.Collections;

namespace LlamaCave
{
    public class Treasure : MonoBehaviour
    {
        bool wasOpened;

        Treasure()
        {
            DelegatesAndEvents.onCaveCreated += this.CaveCreated;
            wasOpened = false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                wasOpened = true;
            }
        }

        void FixedUpdate()
        {
            gameObject.GetComponent<Animator>().SetBool("WasOpened", wasOpened);
        }

        void CaveCreated()
        {
            wasOpened = false;
        }

    }
}
