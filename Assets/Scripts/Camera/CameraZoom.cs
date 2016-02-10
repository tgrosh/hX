using UnityEngine;
using System.Collections;
using UnityStandardAssets.Cameras;

public class CameraZoom : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {        
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                GameObject.Find("DefaultCameraZoomTarget").transform.position = new Vector3(hit.point.x, hit.point.y, GameObject.Find("DefaultCameraZoomTarget").transform.position.z);
                ZoomIn();
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            ZoomOut();
        }        
	}

    void ZoomIn()
    {
        if (GetComponent<AutoCam>().Target == GameObject.Find("DefaultCameraTarget").transform)
        {
            GetComponent<AutoCam>().SetTarget(GameObject.Find("DefaultCameraZoomTarget").transform);
        }
    }

    void ZoomOut()
    {
        if (GetComponent<AutoCam>().Target != GameObject.Find("DefaultCameraTarget").transform)
        {
            GetComponent<AutoCam>().SetTarget(GameObject.Find("DefaultCameraTarget").transform);
        }
    }
}
