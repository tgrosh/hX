using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameCell : NetworkBehaviour
{
    public Material Empty;
    public Material BaseMaterial;
    public Material BaseAreaMaterial;
    public Material ShipMaterial;
    public Material MovementAreaMaterial;
    public GameObject hex;
    public GameObject ship;
    public ParticleSystem selectedParticles;
    
    [SyncVar]
    public GameCellState state = GameCellState.Empty;
    [SyncVar]
    public PlayerSeat ownerSeat = PlayerSeat.Empty;
    [SyncVar]
    public NetworkInstanceId owner = NetworkInstanceId.Invalid;
    [SyncVar]
    public bool selected = false;
    
    private float cellAnimationSpeed = 3f;
    private float cellAnimationTime = 0f;
    private bool animatingColor;
    private Color cellColorTarget = Color.white;
    private Player ownerPlayer;
    private List<GameObject> adjacent = new List<GameObject>();
    private Ship associatedShip;

    // Use this for initialization
    void Start()
    {
        if (isServer && ownerSeat != PlayerSeat.Empty) { 
            SetOwner(GameManager.singleton.PlayerAtSeat(ownerSeat));
        }
    }

    // Update is called once per frame
    void Update()
    {
        Material cellMaterial = hex.GetComponent<Renderer>().material;

        if (selected && !selectedParticles.gameObject.activeInHierarchy)
        {
            selectedParticles.gameObject.SetActive(true);
        }
        else if (!selected && selectedParticles.gameObject.activeInHierarchy)
        {
            selectedParticles.gameObject.SetActive(false);
        }

        if (owner != NetworkInstanceId.Invalid)
        {
            if (state == GameCellState.Base && !cellMaterial.name.Contains(BaseMaterial.name))
            {
                SetCellColor(BaseMaterial);
                animatingColor = true;
            }
            else if (state == GameCellState.BaseArea && !cellMaterial.name.Contains(BaseAreaMaterial.name))
            {
                SetCellColor(BaseAreaMaterial);
                animatingColor = true;
            }
            else if (state == GameCellState.MovementArea && !cellMaterial.name.Contains(MovementAreaMaterial.name))
            {
                hex.GetComponent<Renderer>().material = MovementAreaMaterial;
            }

            if (animatingColor && cellAnimationTime / cellAnimationSpeed < .9f)
            {
                hex.GetComponent<Renderer>().material.color = Color.Lerp(cellMaterial.color, cellColorTarget, cellAnimationSpeed * Time.deltaTime);

                cellAnimationTime += Time.deltaTime;
            }
            else if (cellAnimationTime < cellAnimationSpeed)
            {
                hex.GetComponent<Renderer>().material.color = cellColorTarget;

                cellAnimationTime = 0;
                animatingColor = false;
            }
        }
        else
        {
            if (state == GameCellState.Empty && !cellMaterial.name.Contains(Empty.name))
            {
                hex.GetComponent<Renderer>().material = Empty;
            }
        }
    }
    
    private void SetCellColor(Material material)
    {
        ownerPlayer = ClientScene.FindLocalObject(owner).GetComponent<Player>();
        cellColorTarget = ownerPlayer.color;
        hex.GetComponent<Renderer>().material = material;
    }

    [Server]
    private void SetOwner(Player player)
    {
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
        this.state = state;

        return result;
    }

    //[Server]
    //bool SetAdjacentCells(GameCell cell, NetworkInstanceId adjacentPlayerId, GameCellState adjacentCellState, NetworkInstanceId newPlayerId, GameCellState newCellState, bool continueCascade)
    //{
    //    bool result = false;

    //    foreach (GameObject obj in cell.adjacent)
    //    {
    //        GameCell adjacentCell = obj.GetComponent<GameCell>();

    //        if (adjacentCell.state == adjacentCellState && adjacentCell.owner == adjacentPlayerId)
    //        {
    //            result = SetCell(adjacentCell, newPlayerId, newCellState) || result;

    //            if (continueCascade)
    //            {
    //                SetAdjacentCells(adjacentCell, adjacentPlayerId, adjacentCellState, newPlayerId, newCellState, continueCascade);
    //            }
    //        }
    //    }

    //    return result;
    //}
    
    [Server]
    public bool Select(Player player)
    {
        if (state == GameCellState.BaseArea && owner == player.netId)
        {
            //clicking on my own base area, place ship
            SetCell(player, GameCellState.Ship);
            GameObject objShip = (GameObject)Instantiate(ship, transform.position, Quaternion.identity);
            associatedShip = objShip.GetComponent<Ship>();
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
                foreach (GameObject obj in associatedShip.movementCells)
                {
                    GameCell cell = obj.GetComponent<GameCell>();
                    if (cell.state == GameCellState.Empty)
                    {
                        cell.SetCell(player, GameCellState.MovementArea);
                    }                    
                }
            }
            else
            {
                foreach (GameObject obj in associatedShip.movementCells)
                {
                    GameCell cell = obj.GetComponent<GameCell>();
                    if (cell.state == GameCellState.MovementArea)
                    {
                        cell.SetCell(null, GameCellState.Empty);
                    }
                }
            }
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
