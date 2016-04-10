using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Player : NetworkBehaviour
{
    public static Player localPlayer;
    public static Player opponent;
    public List<FleetVessel> fleetVessels = new List<FleetVessel>();

    [SyncVar]
    public Color color;
    [SyncVar]
    public bool playerActive = false;
    [SyncVar]
    public int score;
    [SyncVar]
    public PlayerSeat seat;
    [SyncVar]
    public NetworkInstanceId playerBase = NetworkInstanceId.Invalid;
    [SyncVar]
    public bool isBuyingColonyShip;
    [SyncVar]
    public bool isBuyingFleetVessel;
    [SyncVar]
    public bool isBuyingDepot;
    [SyncVar]
    public bool isBuyingBoosterUpgrade;
    [SyncVar]
    public bool isBuyingTractorBeamUpgrade;
    [SyncVar]
    public bool isBuyingBlasterUpgrade;
    [SyncVar]
    public int reputation;
    [SyncVar]
    public bool tempusAccess;
    [SyncVar]
    public bool isBuyingStarport;    

    // Use this for initialization
    void Start()
    {
        if (isLocalPlayer)
        {
            localPlayer = this;
        }
        else
        {
            opponent = this;
        }

        if (isServer)
        {
            GameManager.OnTurnStart += GameManager_OnTurnStart;
        }

        Depot.OnDepotStarted += Depot_OnDepotStarted;
        Starport.OnStarportStarted += Starport_OnStarportStarted;
        FleetVessel.OnShipStarted += Ship_OnShipStarted;
        FleetVessel.OnBoostersChanged += Ship_OnBoostersChanged;
    }

    public List<Starport> Starports
    {
        get
        {
            return new List<Starport>(GameObject.FindObjectsOfType<Starport>()).FindAll((Starport starport) => { return starport.owner = this; });
        }
    }

    public List<Depot> Depots
    {
        get
        {
            return new List<Depot>(GameObject.FindObjectsOfType<Depot>()).FindAll((Depot depot) => { return depot.owner = this; });
        }
    }

    void Starport_OnStarportStarted(Starport starport)
    {
        isBuyingStarport = false;
    }

    private void Ship_OnBoostersChanged(int count)
    {
        isBuyingBoosterUpgrade = false;
    }

    private void Ship_OnShipStarted(Ship ship)
    {
        isBuyingFleetVessel = false;
        isBuyingColonyShip = false;
    }

    private void Depot_OnDepotStarted(Depot depot)
    {
        isBuyingDepot = false;
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
    
    [Command]
    private void Cmd_SelectCell(NetworkInstanceId cellId)
    {
        if (GameManager.singleton.activePlayer == this)
        {
            NetworkServer.FindLocalObject(cellId).GetComponent<GameCell>().Select(this);
        }
    }
    
    [Command]
    private void Cmd_EndTurn()
    {
        GameManager.singleton.EndPlayerTurn();
    }

    [Command]
    public void Cmd_SetIsBuyingFleetVessel(bool isBuying)
    {
        NetworkServer.FindLocalObject(playerBase).GetComponent<Base>().ToggleArea(isBuying);
        foreach (Starport starport in Starports)
        {
            starport.ToggleArea(isBuying);
        }
        this.isBuyingFleetVessel = isBuying;
    }

    [Command]
    public void Cmd_SetIsBuyingColonyShip(bool isBuying)
    {
        NetworkServer.FindLocalObject(playerBase).GetComponent<Base>().ToggleArea(isBuying);        
        this.isBuyingColonyShip = isBuying;
    }

    [Command]
    public void Cmd_SetIsBuyingDepot(bool isBuying)
    {
        if (isBuying)
        {
            foreach (FleetVessel ship in fleetVessels)
            {
                List<GameCell> nearbyBuildableCells = ship.nearbyCells.FindAll((GameCell objCell) => { return !objCell.hasLocalPlayerFleetVessel && objCell.state == GameCellState.Empty; });
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
        this.isBuyingDepot = isBuying;
    }

    [Command]
    public void Cmd_SetIsBuyingStarport(bool isBuying)
    {
        this.isBuyingStarport = isBuying;
    }

    [Command]
    public void Cmd_SetIsBuyingBlasterUpgrade(bool isBuying)
    {
        this.isBuyingBlasterUpgrade = isBuying;
    }

    [Command]
    public void Cmd_SetIsBuyingTractorBeamUpgrade(bool isBuying)
    {
        this.isBuyingTractorBeamUpgrade = isBuying;
    }

    [Command]
    public void Cmd_SetIsBuyingBoosterUpgrade(bool isBuying)
    {
        this.isBuyingBoosterUpgrade = isBuying;
    }

    [Command]
    public void Cmd_UpdateReputation(int rep)
    {
        reputation += rep;
    }

    [Command]
    public void Cmd_SetShipDisabled(NetworkInstanceId shipId, bool isDisabled)
    {
        NetworkServer.FindLocalObject(shipId).GetComponent<FleetVessel>().IsDisabled = true;
    }

    [Command]
    public void Cmd_AddShipCargo(NetworkInstanceId shipId, ResourceType resource, int quantity)
    {
        NetworkServer.FindLocalObject(shipId).GetComponent<FleetVessel>().cargoHold.Add(resource, quantity);
    }

    [Command]
    public void Cmd_DumpShipCargo(NetworkInstanceId shipId, ResourceType resource, int quantity)
    {
        NetworkServer.FindLocalObject(shipId).GetComponent<FleetVessel>().cargoHold.Dump(resource, quantity);
    }

    [Command]
    public void Cmd_PurgeShipCargo(NetworkInstanceId shipId)
    {
        NetworkServer.FindLocalObject(shipId).GetComponent<FleetVessel>().cargoHold.Purge();
    }

    [Command]
    public void Cmd_PurgeHalfShipCargo(NetworkInstanceId shipId)
    {
        NetworkServer.FindLocalObject(shipId).GetComponent<FleetVessel>().cargoHold.PurgeHalf();
    }
    
    [Command]
    public void Cmd_ShipWormholeTo(NetworkInstanceId shipId, NetworkInstanceId cellId)
    {
        NetworkServer.FindLocalObject(shipId).GetComponent<FleetVessel>().WormholeTo(cellId);
    }

    [Command]
    public void Cmd_SetTempusAccess(bool granted)
    {
        tempusAccess = granted;
    }

    [ClientRpc]
    public void Rpc_AddResource(ResourceType resource)
    {
        if (isLocalPlayer)
        {
            GameObject.Find("ResourceTracker").GetComponent<ResourceTracker>().IncrementResource(resource);
        }
    }

    [ClientRpc]
    public void Rpc_DumpResource(ResourceType resource)
    {
        if (isLocalPlayer)
        {
            GameObject.Find("ResourceTracker").GetComponent<ResourceTracker>().DecrementResource(resource);
        }
    }
    
    [ClientRpc]
    public void Rpc_StartGame()
    {
        if (isLocalPlayer)
        {
            GameObject.Find("Waiting").SetActive(false);
            GameObject.Find("ResourceTracker").GetComponent<ResourceTracker>().Show();
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

    [ClientRpc]
    public void Rpc_ShowEncounter(NetworkInstanceId ownerShip, int currentEncounterIndex)
    {
        if (isLocalPlayer)
        {
            GameObject.Find("EncounterPanel").GetComponent<EncounterManager>().ShowEncounter(ownerShip, currentEncounterIndex);
        }
    }
}
