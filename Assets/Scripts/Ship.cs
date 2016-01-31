using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Ship : NetworkBehaviour {
    public float range = 10f; //not used yet
    public List<GameObject> movementCells = new List<GameObject>();
    [SyncVar]
    public Color color;

    [SyncVar]
    private NetworkInstanceId destination = NetworkInstanceId.Invalid;
    private bool animatingEntrance;
    private float animationCurrentTime;
    private float animationSpeed = 3f;
    private Vector3 origPosition;
    private float moveSpeed = 2f;
    private float moveTime = 0f;
    private Vector3 targetPoint;
    private bool isColorSet;

	// Use this for initialization
	void Start () {
        origPosition = transform.position;
        transform.position = transform.position + new Vector3(0, 0, 1);
        animatingEntrance = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (animatingEntrance && animationCurrentTime / animationSpeed < .95f)
        {
            transform.position = Vector3.Lerp(transform.position, origPosition, animationSpeed * Time.deltaTime);
            animationCurrentTime += Time.deltaTime;
        }
        else if (animatingEntrance)
        {
            transform.localPosition = origPosition;

            animationCurrentTime = 0;
            animatingEntrance = false;
        }
        
        if (destination != NetworkInstanceId.Invalid)
        {
            targetPoint = ClientScene.FindLocalObject(destination).transform.position;

            if (transform.position != targetPoint)
            {
                Quaternion look = Quaternion.LookRotation(-(targetPoint - transform.position), Vector3.forward);
                look.x = look.y = 0;                
                transform.rotation = look;
                
                if (transform.position != targetPoint && moveTime / moveSpeed < .95f)
                {
                    transform.position = Vector3.Slerp(transform.position, targetPoint, moveSpeed * Time.deltaTime);
                    transform.rotation = look;
                    moveTime += Time.deltaTime;
                }
                else if (transform.position != targetPoint)
                {
                    transform.position = targetPoint;
                    moveTime = 0;
                }
            }
        }

        if (!isColorSet)
        {
            SetColor(this.color);
            isColorSet = true;
        }
	}

    [Client]
    private void SetColor(Color color)
    {        
        foreach (Renderer rend in transform.FindChild("SSO-2").FindChild("SSO-2").gameObject.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in rend.materials)
            {
                if (mat.name.Contains("Default"))
                {
                    mat.color = color + (Color.white * .5f);
                }
            }
        }
    }

    public void MoveTo(NetworkInstanceId cellId)
    {
        this.destination = cellId;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!movementCells.Contains(other.gameObject))
        {
            movementCells.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (movementCells.Contains(other.gameObject))
        {
            movementCells.Remove(other.gameObject);
        }
    }
}
