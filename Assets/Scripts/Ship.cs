using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public abstract class Ship : NetworkBehaviour {
    public float baseMovementRange;
    [HideInInspector]
    public List<GameCell> nearbyCells = new List<GameCell>();
    [HideInInspector]
    public Base nearbyBase;
    public Transform cameraTarget;
    public Wormhole prefabWormhole;

    protected float movementRange;

    private Transform colliderTransform;
    private float moveSpeed = 2f;
    private Vector3 targetPoint;
    private NetworkInstanceId wormholeCellId;
    private Wormhole whSource;
    private Wormhole whDest;

    [SyncVar]
    [HideInInspector]
    public NetworkInstanceId ownerId;
    [SyncVar]
    [HideInInspector]
    public NetworkInstanceId associatedCell;
    [SyncVar]
    private int disabledRounds;
    [SyncVar]
    private bool isDisabled;
    [SyncVar]
    private NetworkInstanceId travelDestination = NetworkInstanceId.Invalid;

    public delegate void ShipMoveStart(Ship ship);
    public static event ShipMoveStart OnShipMoveStart;
    public delegate void ShipMoveEnd(Ship ship);
    public static event ShipMoveEnd OnShipMoveEnd;

    public delegate void ShipSpawnStart(Ship ship);
    public static event ShipSpawnStart OnShipSpawnStart;
    public delegate void ShipSpawnEnd(Ship ship);
    public static event ShipSpawnEnd OnShipSpawnEnd;

    public delegate void ShipStarted(Ship ship);
    public static event ShipStarted OnShipStarted;

	// Use this for initialization
	protected void Start () {
        colliderTransform = transform.FindChild("collider");

        movementRange = baseMovementRange;

        GameManager.OnRoundStart += GameManager_OnRoundStart;
        
        if (OnShipStarted != null)
        {
            OnShipStarted(this);
        }

        GetComponentInChildren<AnimationHandler>().OnAnimationComplete += AnimationHandler_OnAnimationComplete;
        if (OnShipSpawnStart != null)
        {
            OnShipSpawnStart(this);
        }
	}
    	
	// Update is called once per frame
	protected void Update () {
        if (colliderTransform != null)
        {
            colliderTransform.localScale = new Vector3(movementRange, movementRange, 2);
        }

        if (travelDestination != NetworkInstanceId.Invalid)
        {
            targetPoint = ClientScene.FindLocalObject(travelDestination).transform.position;

            if (transform.position != targetPoint)
            {
                Quaternion look = Quaternion.LookRotation(-(targetPoint - transform.position), Vector3.forward);
                look.x = look.y = 0;
                transform.rotation = look;

                if (Vector3.Distance(transform.position, targetPoint) > .01f)
                {
                    transform.position = Vector3.Lerp(transform.position, targetPoint, moveSpeed * Time.deltaTime);
                }
                else
                {
                    travelDestination = NetworkInstanceId.Invalid;
                    transform.position = targetPoint;
                    GameManager.singleton.ResetCamera();
                }

                if (Vector3.Distance(transform.position, targetPoint) <= .01f)
                {
                    if (OnShipMoveEnd != null)
                    {
                        OnShipMoveEnd(this);
                    }
                }
            }
        }        
	}

    void AnimationHandler_OnAnimationComplete(AnimationType animationType)
    {
        if (animationType == AnimationType.FleetVesselEnter || animationType == AnimationType.ColonyShipEnter)
        {
            ShipEnterComplete();
        }
    }
    
    [Server]
    void GameManager_OnRoundStart()
    {
        disabledRounds--;
        if (IsDisabled && disabledRounds <= 0)
        {
            IsDisabled = false;
        }
    }
        
    [Server]
    public void MoveTo(NetworkInstanceId cellId)
    {
        if (IsDisabled) return;

        this.associatedCell = cellId;
        this.travelDestination = cellId;

        if (OnShipMoveStart != null)
        {
            OnShipMoveStart(this);
        }
        Rpc_ShipMoveStart();
    }

    [Server]
    public void WormholeTo(NetworkInstanceId cellId)
    {
        NetworkServer.FindLocalObject(associatedCell).GetComponent<GameCell>().SetCell(false, NetworkInstanceId.Invalid);
        NetworkServer.FindLocalObject(cellId).GetComponent<GameCell>().SetCell(this.owner, true, this.netId);
        Rpc_WormholeTo(cellId);
    }


    public Color Color
    {
        set
        {
            Rpc_SetColor(value);
        }
    }   	

    public Player owner
    {
        get
        {
            return NetworkServer.FindLocalObject(ownerId).GetComponent<Player>();
        }
    }
    
    public bool IsDisabled
    {
        get { return isDisabled; }
        set
        {
            isDisabled = value;

            if (isDisabled)
            {
                disabledRounds = 2;
                Rpc_SetColor(Color.red);
            }
            else
            {
                disabledRounds = 0;
                Rpc_SetColor(owner.color);
            }
        }
    }

    [Client]
    protected abstract void SetColor(Color color);
    
    [ClientRpc]
    void Rpc_SetColor(Color color)
    {
        SetColor(color);
    }

    [ClientRpc]
    void Rpc_ShipMoveStart()
    {
        if (OnShipMoveStart != null)
        {
            OnShipMoveStart(this);
        }
    }

    [ClientRpc]
    void Rpc_WormholeTo(NetworkInstanceId cellId)
    {
        whSource = (Wormhole)Instantiate(prefabWormhole, ClientScene.FindLocalObject(associatedCell).transform.position, Quaternion.identity);
        whDest = (Wormhole)Instantiate(prefabWormhole, ClientScene.FindLocalObject(cellId).transform.position, Quaternion.identity);

        associatedCell = cellId;
        wormholeCellId = cellId;

        CameraWatcher.OnCameraReachedDestination += StartWormholeJump;
        GameManager.singleton.cam.SetTarget(this.cameraTarget.transform);
    }

    void StartWormholeJump()
    {
        GetComponentInChildren<AnimationHandler>().OnAnimationComplete += FleetVessel_EnterWormhole;
        GetComponentInChildren<AnimationHandler>().animator.SetTrigger("EnterWormhole");

        //unregister the event
        CameraWatcher.OnCameraReachedDestination -= StartWormholeJump;
    }

    void FleetVessel_EnterWormhole(AnimationType tEnter)
    {
        if (tEnter == AnimationType.FleetVesselWormholeEnter)
        {
            //ship has entered wormhole
            Vector3 newCellPosition = ClientScene.FindLocalObject(wormholeCellId).GetComponent<GameCell>().transform.position;
            transform.position = new Vector3(newCellPosition.x, newCellPosition.y, transform.position.z);

            GetComponentInChildren<AnimationHandler>().OnAnimationComplete += FleetVessel_ExitWormhole;
            GetComponentInChildren<AnimationHandler>().animator.SetTrigger("ExitWormhole");
        }
    }

    void FleetVessel_ExitWormhole(AnimationType tExit)
    {
        if (tExit == AnimationType.FleetVesselWormholeExit)
        {
            //ship has exited wormhole
            whSource.Exit();
            whDest.Exit();
            GameManager.singleton.ResetCamera();
        }
    }
    
    protected void OnTriggerEnter(Collider other)
    {
        GameCell otherGameCell = other.gameObject.GetComponent<GameCell>();
        if (otherGameCell != null && !nearbyCells.Contains(otherGameCell))
        {
            nearbyCells.Add(otherGameCell);
        }        
        if (other.GetComponent<Base>() && nearbyBase == null)
        {
            nearbyBase = other.GetComponent<Base>();
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (nearbyCells.Contains(other.gameObject.GetComponent<GameCell>()))
        {
            nearbyCells.Remove(other.gameObject.GetComponent<GameCell>());
        }
        if (nearbyBase == other.transform.GetComponent<Base>())
        {
            nearbyBase = null;
        }
    }

    void ShipEnterComplete()
    {
        if (OnShipSpawnEnd != null)
        {
            OnShipSpawnEnd(this);
        }
    }        
}
