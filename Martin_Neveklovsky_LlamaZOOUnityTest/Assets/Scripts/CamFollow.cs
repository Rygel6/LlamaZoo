using UnityEngine;
using System.Collections;

public class CamFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 followOffset = new Vector3(0, 0, -15);
	
	// Update is called once per frame
	void LateUpdate ()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position + followOffset;
        transform.LookAt(target);
	}
}
