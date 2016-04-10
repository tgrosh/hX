using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TempusStar : NetworkBehaviour {
    private float rotateSpeedMin = 5f;
    private float rotateSpeedMax = 15f;
    private float rotateSpeed;
    public Transform sphere;

	// Use this for initialization
	void Start () {
        rotateSpeed = Random.Range(rotateSpeedMin, rotateSpeedMax);
	}
	
	// Update is called once per frame
	void Update () {
        sphere.Rotate(Vector3.forward * Time.deltaTime * rotateSpeed);
	}
}
