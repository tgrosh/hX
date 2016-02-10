using UnityEngine;
using System.Collections;
using UnityStandardAssets.Cameras;

public class CameraPan : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1)) //right mouse button
        {
            if (GetComponent<AutoCam>().Target == GameObject.Find("DefaultCameraZoomTarget").transform)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {
                    GameObject.Find("DefaultCameraZoomTarget").transform.position = new Vector3(hit.point.x, hit.point.y, GameObject.Find("DefaultCameraZoomTarget").transform.position.z);
                }
            }
        }
    }
}
