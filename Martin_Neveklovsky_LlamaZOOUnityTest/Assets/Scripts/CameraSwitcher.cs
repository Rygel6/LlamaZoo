using UnityEngine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour {

    public bool isFollowEnabled = false;
    public Camera levelCam;
    public Camera followCam;

	// Use this for initialization
	void Start ()
    {
        SyncCameraStates();    
	}
	
	// Update is called once per frame
	void Update () {
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isFollowEnabled = !isFollowEnabled;
            SyncCameraStates();
        }

    }

    void SyncCameraStates()
    {
        if (levelCam && followCam)
        {
            levelCam.enabled = !isFollowEnabled;
            followCam.enabled = isFollowEnabled;
        }
    }
}
