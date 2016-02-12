using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine.Networking;

[ExecuteInEditMode]
public class GameCell : NetworkBehaviour
{
    public Material Empty;
    public Material BaseMaterial;
    public Material BaseAreaMaterial;
    public Material ShipMaterial;
    public Material MovementAreaMaterial;
    public Material ResourceMaterial;
    public GameObject prefabHex;
    public GameObject prefabShip;
    public GameObject prefabResourceLocationTrillium;
    public GameObject prefabResourceLocationHydrazine;
    public GameObject prefabResourceLocationSupplies;
    public GameObject prefabResourceLocationWorkers;
    public GameObject prefabBase;
    public ParticleSystem selectedParticles;
    public ParticleSystem shipParticles;
    
    [SyncVar]
    public GameCellState state = GameCellState.Empty;
    [SyncVar]
    public PlayerSeat ownerSeat = PlayerSeat.Empty;
    [SyncVar]
    public NetworkInstanceId owner = NetworkInstanceId.Invalid;
    [SyncVar]
    public bool selected = false;
    [SyncVar]
    public ResourceType resourceType = ResourceType.None;
    [SyncVar]
    public bool hasShip = false;

    private float cellAnimationSpeed = 3f;
    private float cellAnimationTime = 0f;
    private bool animatingColor;
    private Color cellColorTarget = Color.clear;
    private List<GameObject> adjacent = new List<GameObject>();
    [SyncVar]
    public NetworkInstanceId associatedShip;
    private Resource associatedResourceLocation;
    public Base associatedBase;
    [SyncVar]
    private GameCellState prevState;
    [SyncVar]
    private PlayerSeat prevSeat;
    [SyncVar]
    private NetworkInstanceId prevOwner;
    private float hoverTime;
    private float tooltipDelay = 1f;

    // Use this for initialization
    void Start()
    {
        if (isServer)
        {
            if (ownerSeat != PlayerSeat.Empty)
            {
                SetOwner(GameManager.singleton.PlayerAtSeat(ownerSeat));
            }

            if (state == GameCellState.Resource)
            {
                GameObject objResourceLocation;

                if (resourceType == ResourceType.Corium)
                {
                    objResourceLocation = (GameObject)Instantiate(prefabResourceLocationTrillium, transform.position, Quaternion.identity);
                }
                else if (resourceType == ResourceType.Hydrazine)
                {
                    objResourceLocation = (GameObject)Instantiate(prefabResourceLocationHydrazine, transform.position, Quaternion.identity);
                }
                else if (resourceType == ResourceType.Supplies)
                {
                    objResourceLocation = (GameObject)Instantiate(prefabResourceLocationSupplies, transform.position, Quaternion.identity);
                }
                else
                {
                    objResourceLocation = (GameObject)Instantiate(prefabResourceLocationWorkers, transform.position, Quaternion.identity);
                }
             
                associatedResourceLocation = objResourceLocation.GetComponent<Resource>();
                associatedResourceLocation.type = resourceType;
                NetworkServer.Spawn(objResourceLocation);
            }

            if (state == GameCellState.Base)
            {
                GameObject objBase = (GameObject)Instantiate(prefabBase, transform.position, Quaternion.identity);
                associatedBase = objBase.GetComponent<Base>();
                associatedBase.color = GameManager.singleton.PlayerAtSeat(ownerSeat).color;
                associatedBase.owner = owner;
                NetworkServer.Spawn(objBase);
            }  
        }
            
    } 

    // Update is called once per frame
    void Update()
    {
        //Live
        if (Application.isPlaying)
        {
            Material cellMaterial = prefabHex.GetComponent<Renderer>().material;

            if (selected && !selectedParticles.gameObject.activeInHierarchy)
            {
                selectedParticles.gameObject.SetActive(true);
            }
            else if (!selected && selectedParticles.gameObject.activeInHierarchy)
            {
                selectedParticles.gameObject.SetActive(false);
            }

            if (hasShip && !shipParticles.gameObject.activeInHierarchy)
            {
                shipParticles.gameObject.SetActive(true);
            }
            else if (!hasShip && shipParticles.gameObject.activeInHierarchy)
            {
                shipParticles.gameObject.SetActive(false);
            }

            if (owner != NetworkInstanceId.Invalid)
            {
                if (state == GameCellState.Base && !cellMaterial.name.Contains(BaseMaterial.name))
                {
                    SetCellMaterial(ClientScene.FindLocalObject(owner).GetComponent<Player>().color, BaseMaterial);
                }
                else if (state == GameCellState.BaseArea && !cellMaterial.name.Contains(BaseAreaMaterial.name))
                {
                    SetCellMaterial(ClientScene.FindLocalObject(owner).GetComponent<Player>().color, BaseAreaMaterial);
                }
                else if (state == GameCellState.MovementArea && !cellMaterial.name.Contains(MovementAreaMaterial.name))
                {
                    SetCellMaterial(MovementAreaMaterial.color, MovementAreaMaterial);
                }
            }

            if (state == GameCellState.Empty && !cellMaterial.name.Contains(Empty.name))
            {
                SetCellMaterial(Color.clear, Empty);
            }
            else if (state == GameCellState.Resource && !cellMaterial.name.Contains(ResourceMaterial.name))
            {
                SetCellMaterial(Color.clear, ResourceMaterial);
            }

            if (cellColorTarget != Color.clear && animatingColor && cellAnimationTime / cellAnimationSpeed < .9f)
            {
                prefabHex.GetComponent<Renderer>().material.color = Color.Lerp(cellMaterial.color, cellColorTarget, cellAnimationSpeed * Time.deltaTime);

                cellAnimationTime += Time.deltaTime;
            }
            else if (cellColorTarget != Color.clear && cellAnimationTime < cellAnimationSpeed)
            {
                prefabHex.GetComponent<Renderer>().material.color = cellColorTarget;

                cellAnimationTime = 0;
                animatingColor = false;
            }
        }
        else
        {
            //Design Time
            if (state == GameCellState.Base)
            {
                cellColorTarget = Color.blue;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(BaseMaterial);
            }
            else if (state == GameCellState.BaseArea)
            {
                cellColorTarget = Color.blue;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(BaseAreaMaterial);
            }
            else if (state == GameCellState.Resource)
            {
                cellColorTarget = Resource.GetColor(resourceType);
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(ResourceMaterial);
            }
            else if (state == GameCellState.Empty)
            {
                //cellColorTarget = Color.clear;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(Empty);
            }
            prefabHex.GetComponent<Renderer>().sharedMaterial.color = cellColorTarget;
        }
    }
    
    private void SetCellMaterial(Color color, Material material)
    {
        cellColorTarget = color;
        prefabHex.GetComponent<Renderer>().material = material;
    }

    [Server]
    private void SetOwner(Player player)
    {
        prevOwner = owner;
        prevSeat = ownerSeat;

        if (player == null)
        {   
            owner = NetworkInstanceId.Invalid;
            ownerSeat = PlayerSeat.Empty;
        }
        else
        {
            owner = player.netId;
            ownerSeat = player.seat;
        }
    }

    
    [Server]
    public bool SetCell(Player player, GameCellState state)
    {
        bool result = (this.state != state);

        SetOwner(player);
        prevState = this.state;
        this.state = state;

        return result;
    }
    
    [Server]
    public void Revert()
    {
        state = prevState;
        ownerSeat = prevSeat;
        owner = prevOwner;
    }

    [Server]
    public bool Select(Player player)
    {
        if (hasShip && owner == player.netId)
        {
            //clicking on my ship space, select it
            selected = !selected;
            GameManager.singleton.selectedCell = selected ? this : null;

            if (selected)
            {
                foreach (GameCell cell in NetworkServer.FindLocalObject(associatedShip).GetComponent<Ship>().nearbyCells)
                {
                    if (cell.state == GameCellState.Empty || cell.state == GameCellState.BaseArea)
                    {
                        cell.SetCell(player, GameCellState.MovementArea);
                    }
                }
            }
            else
            {
                foreach (GameCell cell in NetworkServer.FindLocalObject(associatedShip).GetComponent<Ship>().nearbyCells)
                {
                    if (cell.state == GameCellState.MovementArea)
                    {
                        cell.Revert();
                    }
                }
            }
        }
        else if (state == GameCellState.BaseArea && owner == player.netId)
        {
            if (player.isBuyingShip && player.Purchase(PurchaseManager.Ship))
            {
                SetCell(player, GameCellState.Empty);
                player.isBuyingShip = false;
                hasShip = true;
                GameObject objShip = (GameObject)Instantiate(prefabShip, transform.position, Quaternion.identity);
                Ship ship = objShip.GetComponent<Ship>();
                ship.color = player.color;
                ship.owner = player;
                NetworkServer.Spawn(objShip);
                associatedShip = ship.netId;
                return true;
            }
        }
        else if (state == GameCellState.MovementArea && owner == player.netId)
        {
            //move the ship
            Revert();
            SetOwner(player);
            hasShip = true;
            associatedShip = GameManager.singleton.selectedCell.associatedShip;
            GameManager.singleton.selectedCell.associatedShip = NetworkInstanceId.Invalid;
            GameManager.singleton.selectedCell.hasShip = false;
            GameManager.singleton.selectedCell.Revert();
            GameManager.singleton.selectedCell = null;
            NetworkServer.FindLocalObject(associatedShip).GetComponent<Ship>().MoveTo(this.netId);
            return true;
        }

        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!adjacent.Contains(other.gameObject))
        {
            adjacent.Add(other.gameObject);
        }
    }

    void OnMouseOver()
    {
        if (hasShip && owner == Player.localPlayer.netId && associatedShip != NetworkInstanceId.Invalid)
        {
            hoverTime += Time.deltaTime;
            if (hoverTime > tooltipDelay)
            {
                MenuManager.singleton.ShowShipTooltip(ClientScene.FindLocalObject(associatedShip).GetComponent<Ship>());
            }
        }
    }

    void OnMouseExit()
    {
        hoverTime = 0;
        MenuManager.singleton.HideShipTooltip();
    }

}
