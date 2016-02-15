using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Player : NetworkBehaviour
{
    public static Player localPlayer;
    public static Player opponent;
    
    [SyncVar]
    public Color color;
    [SyncVar(hook = "OnPlayerSetName")]
    public string playerName;
    [SyncVar]
    public bool playerActive = false;
    [SyncVar]
    public int score;
    [SyncVar]
    public PlayerSeat seat;
    [SyncVar]
    public NetworkInstanceId playerBase = NetworkInstanceId.Invalid;
    [SyncVar]
    public bool isBuyingShip;
    public List<Ship> ships = new List<Ship>();
    [SyncVar]
    public bool isBuyingDepot;
    [SyncVar]
    public bool isBuyingBoosterUpgrade;

    // Use this for initialization
    void Start()
    {
        if (isLocalPlayer)
        {
            localPlayer = this;
            this.playerName = LocalPlayerInfo.singleton.name;
            Cmd_SetName(this.playerName);
        }
        else
        {
            opponent = this;
        }

        if (isServer)
        {
            GameManager.OnTurnStart += GameManager_OnTurnStart;
        }
    }

    void GameManager_OnTurnStart()
    {
        if (GameManager.singleton.activePlayer == this)
        {
            Rpc_StartTurn();
        }        
    }
    
    public bool CanAfford(Purchase purchase)
    {
        bool result = true;

        if (playerBase == NetworkInstanceId.Invalid) return false;

        foreach (PurchaseCost cost in purchase.cost)
        {
            result = NetworkServer.FindLocalObject(playerBase).GetComponent<Base>().cargoHold.GetCargo(cost.resource).Count >= cost.quantity && result;
        }

        return result;
    }

    public bool Purchase(Purchase purchase)
    {
        bool result = true;

        if (playerBase == NetworkInstanceId.Invalid) return false;

        if (CanAfford(purchase))
        {
            foreach (PurchaseCost cost in purchase.cost)
            {
                NetworkServer.FindLocalObject(playerBase).GetComponent<Base>().cargoHold.Dump(cost.resource, cost.quantity);
            }
            result = true;
        }
        else
        {
            result = false;
        }        

        return result;
    }

    public Base GetBase()
    {
        if (playerBase == NetworkInstanceId.Invalid) return null;

        return ClientScene.FindLocalObject(playerBase).GetComponent<Base>();
    }

    [Client]
    public void EndTurn()
    {
        if (isLocalPlayer)
        {
            UIManager.singleton.hotbar.Toggle(false);
            GameManager.singleton.ResetCamera();
            Cmd_EndTurn();
        }
    }

    [Client]
    public void SelectCell(NetworkInstanceId cellId)
    {
        Cmd_SelectCell(cellId);
    }

    [Client]
    private void OnPlayerSetName(string value)
    {
        this.playerName = value;
    }

    [Command]
    private void Cmd_SelectCell(NetworkInstanceId cellId)
    {
        if (GameManager.singleton.activePlayer == this)
        {
            NetworkServer.FindLocalObject(cellId).GetComponent<GameCell>().Select(this);
        }
    }

    [Command]
    private void Cmd_SetName(string name)
    {
        this.playerName = name;
    }

    [Command]
    private void Cmd_EndTurn()
    {
        GameManager.singleton.EndPlayerTurn();
    }

    [ClientRpc]
    public void Rpc_StartGame()
    {
        if (isLocalPlayer)
        {
            GameObject.Find("Waiting").SetActive(false);
            UIManager.singleton.ShowResourceTracker();
        }        
    }

    [ClientRpc]
    public void Rpc_StartTurn()
    {
        if (isLocalPlayer)
        {
            GameManager.singleton.ResetCamera();
            UIManager.singleton.hotbar.Toggle(true);
            UIManager.singleton.ShowYourTurn();
        }
    }

    [Command]
    public void Cmd_SetIsBuyingShip(bool isBuyingShip)
    {
        NetworkServer.FindLocalObject(playerBase).GetComponent<Base>().ToggleArea(isBuyingShip);
        this.isBuyingShip = isBuyingShip;
    }

    [Command]
    public void Cmd_SetIsBuyingDepot(bool isBuyingDepot)
    {
        if (isBuyingDepot)
        {
            foreach (Ship ship in ships)
            {
                List<GameCell> nearbyBuildableCells = ship.nearbyCells.FindAll((GameCell objCell) => { return !objCell.hasShip && objCell.state == GameCellState.Empty; });
                foreach (GameCell cell in nearbyBuildableCells)
                {
                    if (Vector3.Distance(ship.transform.position, cell.transform.position) <= ship.buildRange)
                    {
                        List<GameCell> adjacentResources = cell.adjacentCells.FindAll((GameCell objCell) => { return objCell.state == GameCellState.Resource; });
                        if (adjacentResources.Count > 0)
                        {
                            cell.SetCell(ship.owner, GameCellState.DepotBuildArea);
                        }
                    }
                }
            }
        }
        else
        {
            foreach (GameCell cell in GameManager.singleton.cells)
            {
                if (cell.state == GameCellState.DepotBuildArea)
                {
                    cell.Revert();
                }
            }
        }
        this.isBuyingDepot = isBuyingDepot;
    }

    [ClientRpc]
    public void Rpc_AddResource(ResourceType resource)
    {
        if (isLocalPlayer)
        {
            GameManager.singleton.IncrementResource(resource);
        }
    }

    [ClientRpc]
    public void Rpc_DumpResource(ResourceType resource)
    {
        if (isLocalPlayer)
        {
            GameManager.singleton.DecrementResource(resource);
        }
    }

    [Server]
    public void Cmd_SetIsBuyingBoosterUpgrade(bool isBuyingBoosterUpgrade)
    {
        this.isBuyingBoosterUpgrade = isBuyingBoosterUpgrade;
    }
}
