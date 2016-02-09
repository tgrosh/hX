using UnityEngine;
using System.Collections;
using UnityStandardAssets.Cameras;

public class CameraWatcher : MonoBehaviour {

    public delegate void CameraDestinationReached();
    public event CameraDestinationReached OnCameraReachedDestination;

    private bool atDestination = false;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance(GetComponent<AutoCam>().Target.position, transform.position) < .1f)
        {
            if (!atDestination)
            {
                if (OnCameraReachedDestination != null)
                {
                    OnCameraReachedDestination();
                }
                atDestination = true;
            }            
        }
        else
        {
            atDestination = false;
        }
	}
}
