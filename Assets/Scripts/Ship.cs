using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Ship : NetworkBehaviour {
    public float range = 10f; //not used yet

    private bool animatingEntrance;
    private float animationCurrentTime;
    private float animationSpeed = 3f;
    private Vector3 origPosition;
    public List<GameObject> movementCells = new List<GameObject>();

	// Use this for initialization
	void Start () {
        origPosition = transform.position;
        transform.position = transform.position + new Vector3(0, 0, 1);
        animatingEntrance = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (animatingEntrance && animationCurrentTime / animationSpeed < .9f)
        {
            transform.position = Vector3.Lerp(transform.position, origPosition, animationSpeed * Time.deltaTime);
            animationCurrentTime += Time.deltaTime;
        }
        else if (animationCurrentTime < animationSpeed)
        {
            transform.localPosition = origPosition;

            animationCurrentTime = 0;
            animatingEntrance = false;
        }
	}
    
    void OnTriggerEnter(Collider other)
    {
        if (!movementCells.Contains(other.gameObject))
        {
            movementCells.Add(other.gameObject);
        }
    }
}
