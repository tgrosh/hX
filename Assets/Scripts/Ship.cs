using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using UnityStandardAssets.Cameras;

public class Ship : NetworkBehaviour {
    public float collectionRange = 2.2f;
    public float buildRange = 2.2f;
    public float cargoDropRange = 2.2f;
    public float movementRange = 3.4f;
    public List<GameCell> nearbyCells = new List<GameCell>();
    public List<Resource> nearbyResources = new List<Resource>();
    public List<Depot> nearbyDepots = new List<Depot>();
    public Base nearbyBase;
    public Transform cameraTarget;
    private int boosterCount = 0;
    private int blasterCount = 0;
    private int tractorBeamCount = 0;
    private CapsuleCollider capsuleCollider;

    public delegate void ShipMoveEnd(Ship ship);
    public static event ShipMoveEnd OnShipMoveEnd;

    public delegate void ShipSpawnEnd(Ship ship);
    public static event ShipSpawnEnd OnShipSpawnEnd;

    public delegate void ShipStarted(Ship ship);
    public static event ShipStarted OnShipStarted;

    public delegate void BoostersChanged(int count);
    public static event BoostersChanged OnBoostersChanged;

    public delegate void BlastersChanged(int count);
    public static event BlastersChanged OnBlastersChanged;

    public delegate void TractorBeamsChanged(int count);
    public static event TractorBeamsChanged OnTractorBeamsChanged;

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
        capsuleCollider = GetComponent<CapsuleCollider>();

        GameManager.OnTurnStart += GameManager_OnTurnStart;
        
        if (isServer) { 
            GameManager.singleton.AddEvent(String.Format("Player {0} created a new Trade Ship", GameManager.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
        }

        if (OnShipStarted != null)
        {
            OnShipStarted(this);
        }
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
            TransferResources();
            CollectAvailableResources();            
        }
    }
        	
	// Update is called once per frame
	void Update () {
        capsuleCollider.radius = movementRange + (1.7f * boosterCount);

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
                OnShipSpawnEnd(this);
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
                        OnShipMoveEnd(this);
                    }
                }
            }
        }

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
                    int collectedCount = resource.Collect(cargoHold.AvailableCapacity) + tractorBeamCount;

                    if (collectedCount > 0)
                    {
                        GameManager.singleton.AddEvent(String.Format("Player {0}'s Trade Ship collected " + collectedCount + " {1}",
                            GameManager.singleton.CreateColoredText(owner.seat.ToString(), owner.color), 
                            GameManager.singleton.CreateColoredText(resource.type.ToString(), Resource.GetColor(resource.type))));
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
        if (cargoHold.GetCargo().Count <= 0) return;

        if (nearbyBase != null)
        {
            float distance = Vector3.Distance(nearbyBase.transform.position, transform.position);
            if (distance <= cargoDropRange)
            {
                cargoHold.Transfer(nearbyBase.cargoHold);
                GameManager.singleton.AddEvent(String.Format("Player {0}'s Trade Ship has delivered resources to their Base", GameManager.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
            }
        }

        foreach (Depot depot in nearbyDepots)
        {
            float distance = Vector3.Distance(depot.transform.position, transform.position);
            if (distance <= cargoDropRange)
            {
                cargoHold.Transfer(depot.cargoHold);
                GameManager.singleton.AddEvent(String.Format("Player {0}'s Trade Ship has delivered resources to their Depot", GameManager.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
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

    public int Boosters
    {
        get { return boosterCount; }
        set {
            boosterCount = value;
            Rpc_BoosterChange(boosterCount);
        }
    }

    public int Blasters
    {
        get { return blasterCount; }
        set
        {
            blasterCount = value;
            Rpc_BlasterChange(blasterCount);
        }
    }

    public int TractorBeams
    {
        get { return tractorBeamCount; }
        set
        {
            tractorBeamCount = value;
            Rpc_TractorBeamChange(tractorBeamCount);
        }
    }

    [ClientRpc]
    void Rpc_BoosterChange(int count)
    {
        if (OnBoostersChanged != null)
        {
            OnBoostersChanged(count);
        }
    }

    [ClientRpc]
    void Rpc_BlasterChange(int count)
    {
        if (OnBlastersChanged != null)
        {
            OnBlastersChanged(count);
        }
    }

    [ClientRpc]
    void Rpc_TractorBeamChange(int count)
    {
        if (OnTractorBeamsChanged != null)
        {
            OnTractorBeamsChanged(count);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        GameCell otherGameCell = other.gameObject.GetComponent<GameCell>();
        if (otherGameCell != null && !nearbyCells.Contains(otherGameCell))
        {
            nearbyCells.Add(otherGameCell);
        }

        if (other.transform.parent && other.transform.parent.GetComponent<Resource>() && !nearbyResources.Contains(other.transform.parent.GetComponent<Resource>()))
        {
            nearbyResources.Add(other.transform.parent.GetComponent<Resource>());
        }

        if (other.GetComponent<Depot>() && !nearbyDepots.Contains(other.GetComponent<Depot>()))
        {
            nearbyDepots.Add(other.GetComponent<Depot>());
        }

        if (other.GetComponent<Base>() && nearbyBase == null)
        {
            nearbyBase = other.GetComponent<Base>();
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
