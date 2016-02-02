using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Assets.Scripts;
using System;

public class Ship : NetworkBehaviour {
    private float collectionRange = 2.2f;
    public List<GameCell> nearbyCells = new List<GameCell>();
    public List<Resource> nearbyResources = new List<Resource>();

    [SyncVar]
    public Color color;
    public Player owner;

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

    private int capacity = 20;
    private List<ResourceType> cargo;

	// Use this for initialization
	void Start () {
        origPosition = transform.position;
        transform.position = transform.position + new Vector3(0, 0, 1);
        animatingEntrance = true;
        cargo = new List<ResourceType>(capacity);

        GameManager.OnTurnStart += GameManager_OnTurnStart;
	}

    void GameManager_OnTurnStart()
    {
        if (GameManager.singleton.activePlayer == owner)
        {
            CollectAvailableResources();
        }
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
                    transform.position = Vector3.Lerp(transform.position, targetPoint, moveSpeed * Time.deltaTime);
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
    
    private void CollectAvailableResources()
    {
        foreach (Resource resource in nearbyResources)
        {
            float distance = Vector3.Distance(resource.transform.position, transform.position);
            if (distance <= collectionRange)
            {
                if (cargo.Count < capacity)
                {
                    ResourceType collectedResource = resource.type;
                    int collectedCount = resource.Collect(capacity - cargo.Count);

                    for (int x = 0; x < collectedCount; x++)
                    {
                        cargo.Add(collectedResource);
                    }

                    Debug.Log("Player " + owner.seat + "'s ship now loaded with " + cargo.Count + " resources");

                    foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
                    {
                        Debug.Log(t + ": " + GetCargoCount(t));
                    }
                }
                //Debug.Log("Player " + owner.seat + " has resource in Range (" + distance + "m): " + resource.type);
            }
        }
    }

    public int GetCargoCount(ResourceType type)
    {
        int result = 0;

        result = cargo.FindAll((ResourceType t) => { return t == type; }).Count;

        return result;
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
        if (other.gameObject.GetComponent<GameCell>() && !nearbyCells.Contains(other.gameObject.GetComponent<GameCell>()))
        {
            nearbyCells.Add(other.gameObject.GetComponent<GameCell>());
        }

        if (other.transform.parent && other.transform.parent.GetComponent<Resource>() && !nearbyResources.Contains(other.transform.parent.GetComponent<Resource>()))
        {
            nearbyResources.Add(other.transform.parent.GetComponent<Resource>());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (nearbyCells.Contains(other.gameObject.GetComponent<GameCell>()))
        {
            nearbyCells.Remove(other.gameObject.GetComponent<GameCell>());
        }
        if (other.transform.parent && nearbyResources.Contains(other.transform.parent.GetComponent<Resource>()))
        {
            nearbyResources.Remove(other.transform.parent.GetComponent<Resource>());
        }
    }
}
