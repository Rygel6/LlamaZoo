using UnityEngine;
using System.Collections;

public class MiniMapIcon : MonoBehaviour {

    public Transform target;
    
    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
