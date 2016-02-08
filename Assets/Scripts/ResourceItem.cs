using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ResourceItem : NetworkBehaviour {
    [SyncVar]
    public bool rotate;
    public float rotateSpeed = 7f;
    private float moveSpeed = 3f;
    private float currentMovementTime = 0f;
    [SyncVar]
    private NetworkInstanceId destination = NetworkInstanceId.Invalid;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (rotate)
        {
            transform.Rotate(Vector3.one * Time.deltaTime * rotateSpeed);
        }

        if (destination != NetworkInstanceId.Invalid)
        {
            transform.position = Vector3.MoveTowards(transform.position, ClientScene.FindLocalObject(destination).transform.position, moveSpeed * Time.deltaTime);
            currentMovementTime += Time.deltaTime;

            if (Vector3.Distance(transform.position, ClientScene.FindLocalObject(destination).transform.position) < .1f)
            {
                Destroy(gameObject);
            }
        }
	}

    public void ToggleRotate(bool rotate)
    {
        this.rotate = rotate;
    }

    public void FlyTo(NetworkInstanceId destination)
    {
        this.destination = destination;        
    }
}
