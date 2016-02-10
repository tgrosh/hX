using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Assets.Scripts;
using System;
using UnityStandardAssets.Cameras;

public class Ship : NetworkBehaviour {
    private float collectionRange = 2.2f;
    private float cargoDropRange = 2.2f;
    public List<GameCell> nearbyCells = new List<GameCell>();
    public List<Resource> nearbyResources = new List<Resource>();
    public Base nearbyBase;
    public Transform cameraTarget;

    public delegate void ShipMoveEnd();
    public static event ShipMoveEnd OnShipMoveEnd;

    public delegate void ShipSpawnEnd();
    public static event ShipSpawnEnd OnShipSpawnEnd;
        
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

    public CargoHold cargoHold = new CargoHold();

	// Use this for initialization
    void Start()
    {
        origPosition = transform.position;
        transform.position = transform.position + new Vector3(0, 0, 1);
        animatingEntrance = true;        

        GameManager.OnTurnStart += GameManager_OnTurnStart;

        MenuManager.singleton.ToggleShip(false);
	}

    public override void OnStartClient()
    {
        cameraTarget = transform.FindChild("CameraTarget");
    }

    [Server]
    void GameManager_OnTurnStart()
    {
        if (GameManager.singleton.activePlayer == owner)
        {
            CollectAvailableResources();
            TransferResources();
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

            GameManager.singleton.ResetCamera();
            if (OnShipSpawnEnd != null)
            {
                OnShipSpawnEnd();
            }
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
                    destination = NetworkInstanceId.Invalid;
                    transform.position = targetPoint;
                    moveTime = 0;
                    GameManager.singleton.ResetCamera();

                    if (OnShipMoveEnd != null)
                    {
                        OnShipMoveEnd();
                    }
                }
            }
        }

        //if (isClient)
        //{
        //    Camera.main.GetComponent<AutoCam>().SetTarget(this.transform.FindChild("CameraTarget"));
        //}

        if (!isColorSet)
        {
            SetColor(this.color);
            isColorSet = true;
        }
	}
    
    [Server]
    private void CollectAvailableResources()
    {
        foreach (Resource resource in nearbyResources)
        {
            float distance = Vector3.Distance(resource.transform.position, transform.position);
            if (distance <= collectionRange)
            {
                if (!cargoHold.IsFull)
                {
                    ResourceType collectedResource = resource.type;
                    int collectedCount = resource.Collect(cargoHold.AvailableCapacity);

                    if (collectedCount > 0)
                    {
                        GameManager.singleton.AddEvent("Player " + GameManager.singleton.CreateColoredText(owner.seat.ToString(), owner.color) + " collected " + collectedCount + " " + GameManager.singleton.CreateColoredText(resource.type.ToString(), Resource.GetColor(resource.type)));
                        GameObject item = (GameObject)Instantiate(resource.resourceItemPrefab, resource.sphere.transform.position, resource.sphere.transform.rotation);
                        item.GetComponent<ResourceItem>().FlyTo(netId);
                        NetworkServer.Spawn(item);
                    } 

                    cargoHold.Add(collectedResource, collectedCount);
                }
            }
        }
    }

    [Server]
    private void TransferResources()
    {
        if (nearbyBase != null)
        {
            float distance = Vector3.Distance(nearbyBase.transform.position, transform.position);
            if (distance <= cargoDropRange)
            {
                cargoHold.Transfer(nearbyBase.cargoHold);
            }
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

    [Server]
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

        if (other.transform.GetComponent<Base>() && nearbyBase == null)
        {
            nearbyBase = other.transform.GetComponent<Base>();
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
        if (nearbyBase == other.transform.GetComponent<Base>())
        {
            nearbyBase = null;
        }
    }

    
}
