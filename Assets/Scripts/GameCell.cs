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
    public GameObject hex;
    public GameObject shipPrefab;
    public GameObject resourceLocationPrefab;
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

    private float cellAnimationSpeed = 3f;
    private float cellAnimationTime = 0f;
    private bool animatingColor;
    private Color cellColorTarget = Color.clear;
    private List<GameObject> adjacent = new List<GameObject>();
    private Ship associatedShip;
    private Resource associatedResourceLocation;
    [SyncVar]
    private GameCellState prevState;
    [SyncVar]
    private PlayerSeat prevSeat;
    [SyncVar]
    private NetworkInstanceId prevOwner;

    // Use this for initialization
    void Start()
    {
        if (isServer && ownerSeat != PlayerSeat.Empty) { 
            SetOwner(GameManager.singleton.PlayerAtSeat(ownerSeat));
        }

        if (isServer && state == GameCellState.Resource)
        {
            GameObject objResourceLocation = (GameObject)Instantiate(resourceLocationPrefab, transform.position, Quaternion.identity);
            associatedResourceLocation = objResourceLocation.GetComponent<Resource>();
            associatedResourceLocation.type = resourceType;
            NetworkServer.Spawn(objResourceLocation);
        }        
    } 

    // Update is called once per frame
    void Update()
    {
        Material cellMaterial = hex.GetComponent<Renderer>().sharedMaterial;

        if (selected && !selectedParticles.gameObject.activeInHierarchy)
        {
            selectedParticles.gameObject.SetActive(true);
        }
        else if (!selected && selectedParticles.gameObject.activeInHierarchy)
        {
            selectedParticles.gameObject.SetActive(false);
        }

        if (state == GameCellState.Ship && !shipParticles.gameObject.activeInHierarchy)
        {
            shipParticles.gameObject.SetActive(true);
        }
        else if (state != GameCellState.Ship && shipParticles.gameObject.activeInHierarchy)
        {
            shipParticles.gameObject.SetActive(false);
        }

        //Live
        if (Application.isPlaying)
        {
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
                else if (state == GameCellState.Ship && !cellMaterial.name.Contains(ShipMaterial.name))
                {
                    if (prevState != GameCellState.BaseArea)
                    {
                        SetCellMaterial(ShipMaterial.color, ShipMaterial);
                    }
                }
            }
            else
            {
                if (state == GameCellState.Empty && !cellMaterial.name.Contains(Empty.name))
                {
                    SetCellMaterial(Color.clear, Empty);
                }
            }
        }
        else
        {
            //Design Time
            if (state == GameCellState.Base)
            {
                SetCellMaterial(Color.blue, BaseMaterial);
            }
            else if (state == GameCellState.BaseArea)
            {
                SetCellMaterial(Color.blue, BaseAreaMaterial);
            }
            else if (state == GameCellState.Resource)
            {
                SetCellMaterial(Resource.GetColor(resourceType), ResourceMaterial);
            }
            else if (state == GameCellState.Empty)
            {
                SetCellMaterial(Color.clear, Empty);
            }
        }
        

        if (cellColorTarget != Color.clear && animatingColor && cellAnimationTime / cellAnimationSpeed < .9f)
        {
            hex.GetComponent<Renderer>().sharedMaterial.color = Color.Lerp(cellMaterial.color, cellColorTarget, cellAnimationSpeed * Time.deltaTime);

            cellAnimationTime += Time.deltaTime;
        }
        else if (cellColorTarget != Color.clear && cellAnimationTime < cellAnimationSpeed)
        {
            hex.GetComponent<Renderer>().sharedMaterial.color = cellColorTarget;

            cellAnimationTime = 0;
            animatingColor = false;
        }
    }
    
    private void SetCellMaterial(Color color, Material material)
    {
        //Debug.Log("setting " + state + " cell to " + material.name + " with color " + color);
        cellColorTarget = color;
        hex.GetComponent<Renderer>().sharedMaterial = new Material(material);
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
        if (state == GameCellState.BaseArea && owner == player.netId)
        {
            //clicking on my own base area, place ship
            SetCell(player, GameCellState.Ship);
            GameObject objShip = (GameObject)Instantiate(shipPrefab, transform.position, Quaternion.identity);            
            associatedShip = objShip.GetComponent<Ship>();
            associatedShip.color = player.color;
            associatedShip.owner = player;
            NetworkServer.Spawn(objShip);
            return true;
        }

        if (state == GameCellState.Ship && owner == player.netId)
        {
            //clicking on my ship space, select it
            selected = !selected;
            GameManager.singleton.selectedCell = selected ? this : null;

            if (selected)
            {
                foreach (GameCell cell in associatedShip.nearbyCells)
                {
                    if (cell.state == GameCellState.Empty)
                    {
                        cell.SetCell(player, GameCellState.MovementArea);
                    }                    
                }
            }
            else
            {
                foreach (GameCell cell in associatedShip.nearbyCells)
                {
                    if (cell.state == GameCellState.MovementArea)
                    {
                        cell.Revert();
                    }
                }
            }
        }

        if (state == GameCellState.MovementArea && owner == player.netId)
        {
            //move the ship
            state = GameCellState.Ship;
            associatedShip = GameManager.singleton.selectedCell.associatedShip;
            GameManager.singleton.selectedCell.associatedShip = null;
            GameManager.singleton.selectedCell.Revert();
            GameManager.singleton.selectedCell = null;
            associatedShip.MoveTo(this.netId);
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

}
