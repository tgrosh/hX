using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Wormhole : NetworkBehaviour {
    public float rotateSpeed;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.forward * Time.deltaTime * rotateSpeed);
	}

    public void Exit()
    {
        gameObject.SetActive(false);
    }
}
