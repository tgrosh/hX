using UnityEngine;
using System.Collections;
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
    public Material DepotBuildAreaMaterial;
    public Material TempusSpaceMaterial;
    public Material NoMovementMaterial;
    public GameObject prefabHex;
    public GameObject prefabFleetVessel;
    public GameObject prefabColonyShip;
    public GameObject prefabResourceLocationTrillium;
    public GameObject prefabResourceLocationHydrazine;
    public GameObject prefabResourceLocationSupplies;
    public GameObject prefabResourceLocationWorkers;
    public GameObject prefabBase;
    public GameObject prefabDepot;
    public GameObject prefabStarport;
    public GameObject prefabTempus;
    public GameObject prefabTempus1;
    public GameObject prefabTempus2;
    public GameObject prefabTempus3;
    public GameObject prefabTempus4;
    public GameObject prefabTempus5;
    public GameObject prefabTempus6;
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
    public List<GameCell> adjacentCells = new List<GameCell>();
    [SyncVar]
    public NetworkInstanceId associatedShip;
    [SyncVar]
    public NetworkInstanceId associatedStation;
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

            if (state == GameCellState.Tempus)
            {
                GameObject obj = (GameObject)Instantiate(prefabTempus, transform.position, Quaternion.identity);
                NetworkServer.Spawn(obj);
            }

            if (state == GameCellState.Tempus1)
            {
                GameObject obj = (GameObject)Instantiate(prefabTempus1, transform.position, Quaternion.identity);
                NetworkServer.Spawn(obj);
            }
            if (state == GameCellState.Tempus2)
            {
                GameObject obj = (GameObject)Instantiate(prefabTempus2, transform.position, Quaternion.identity);
                NetworkServer.Spawn(obj);
            }
            if (state == GameCellState.Tempus3)
            {
                GameObject obj = (GameObject)Instantiate(prefabTempus3, transform.position, Quaternion.identity);
                NetworkServer.Spawn(obj);
            }
            if (state == GameCellState.Tempus4)
            {
                GameObject obj = (GameObject)Instantiate(prefabTempus4, transform.position, Quaternion.identity);
                NetworkServer.Spawn(obj);
            }
            if (state == GameCellState.Tempus5)
            {
                GameObject obj = (GameObject)Instantiate(prefabTempus5, transform.position, Quaternion.identity);
                NetworkServer.Spawn(obj);
            }
            if (state == GameCellState.Tempus6)
            {
                GameObject obj = (GameObject)Instantiate(prefabTempus6, transform.position, Quaternion.identity);
                NetworkServer.Spawn(obj);
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

            if (owner == Player.localPlayer.netId && selected && !selectedParticles.gameObject.activeInHierarchy)
            {
                selectedParticles.gameObject.SetActive(true);
            }
            else if (!selected && selectedParticles.gameObject.activeInHierarchy)
            {
                selectedParticles.gameObject.SetActive(false);
            }

            if (owner == Player.localPlayer.netId && hasShip && !shipParticles.gameObject.activeInHierarchy)
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
                else if (owner == Player.localPlayer.netId && state == GameCellState.ShipBuildArea && !cellMaterial.name.Contains(BaseAreaMaterial.name))
                {
                    SetCellMaterial(ClientScene.FindLocalObject(owner).GetComponent<Player>().color, BaseAreaMaterial);
                }
                else if (owner == Player.localPlayer.netId && state == GameCellState.MovementArea && !cellMaterial.name.Contains(MovementAreaMaterial.name))
                {
                    SetCellMaterial(MovementAreaMaterial.color, MovementAreaMaterial);
                }
                else if (owner == Player.localPlayer.netId && state == GameCellState.DepotBuildArea && !cellMaterial.name.Contains(DepotBuildAreaMaterial.name))
                {
                    SetCellMaterial(DepotBuildAreaMaterial.color, DepotBuildAreaMaterial);
                }
                else if (state == GameCellState.Depot && !cellMaterial.name.Contains(Empty.name))
                {
                    SetCellMaterial(Empty.color, Empty);
                }
            }

            if (state == GameCellState.Empty && !cellMaterial.name.Contains(Empty.name))
            {
                SetCellMaterial(Color.clear, Empty);
            }
            else if (state == GameCellState.NoMovement && !cellMaterial.name.Contains(Empty.name))
            {
                SetCellMaterial(Color.clear, Empty);
            }
            else if (state == GameCellState.TempusSpace && !cellMaterial.name.Contains(TempusSpaceMaterial.name))
            {
                SetCellMaterial(Color.clear, TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus1 && !cellMaterial.name.Contains(TempusSpaceMaterial.name))
            {
                SetCellMaterial(Color.clear, TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus2 && !cellMaterial.name.Contains(TempusSpaceMaterial.name))
            {
                SetCellMaterial(Color.clear, TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus3 && !cellMaterial.name.Contains(TempusSpaceMaterial.name))
            {
                SetCellMaterial(Color.clear, TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus4 && !cellMaterial.name.Contains(TempusSpaceMaterial.name))
            {
                SetCellMaterial(Color.clear, TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus5 && !cellMaterial.name.Contains(TempusSpaceMaterial.name))
            {
                SetCellMaterial(Color.clear, TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus6 && !cellMaterial.name.Contains(TempusSpaceMaterial.name))
            {
                SetCellMaterial(Color.clear, TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus && !cellMaterial.name.Contains(TempusSpaceMaterial.name))
            {
                SetCellMaterial(Color.clear, TempusSpaceMaterial);
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
            if (resourceType != ResourceType.None)
            {
                state = GameCellState.Resource;
            }
            else if (state == GameCellState.Resource)
            {
                state = GameCellState.Empty;
            }
            if (state == GameCellState.Empty)
            {
                resourceType = ResourceType.None;
            }

            if (state == GameCellState.Base)
            {
                cellColorTarget = Color.blue;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(BaseMaterial);
            }
            else if (state == GameCellState.ShipBuildArea)
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
                cellColorTarget = Color.black;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(Empty);
            }
            else if (state == GameCellState.NoMovement)
            {
                cellColorTarget = Color.clear;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(NoMovementMaterial);
            }
            else if (state == GameCellState.TempusSpace)
            {
                cellColorTarget = Color.gray;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus1)
            {
                cellColorTarget = Color.cyan;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus2)
            {
                cellColorTarget = Color.cyan;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus3)
            {
                cellColorTarget = Color.cyan;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus4)
            {
                cellColorTarget = Color.cyan;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus5)
            {
                cellColorTarget = Color.cyan;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus6)
            {
                cellColorTarget = Color.cyan;
                prefabHex.GetComponent<Renderer>().sharedMaterial = new Material(TempusSpaceMaterial);
            }
            else if (state == GameCellState.Tempus)
            {
                cellColorTarget = new Color(1, .5f, 0);
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
    public bool SetCell(Player owner, GameCellState state)
    {
        bool result = (this.state != state);

        SetOwner(owner);
        prevState = this.state;
        this.state = state;

        return result;
    }

    [Server]
    public bool SetCell(Player owner, bool hasShip, NetworkInstanceId associatedShip)
    {
        this.hasShip = hasShip;
        this.associatedShip = associatedShip;
        SetOwner(owner);

        return true;
    }

    [Server]
    public bool SetCell(Player owner, GameCellState state, bool hasShip, NetworkInstanceId associatedShip)
    {
        this.hasShip = hasShip;
        this.associatedShip = associatedShip;
        return SetCell(owner, state);
    }

    [Server]
    public bool SetCell(bool hasShip, NetworkInstanceId associatedShip)
    {
        this.hasShip = hasShip;
        this.associatedShip = associatedShip;
        return true;
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
            if (player.isBuyingBoosterUpgrade && hasLocalPlayerFleetVessel)
            {
                if (player.Purchase(PurchaseManager.UpgradeBooster))
                {
                    NetworkServer.FindLocalObject(associatedShip).GetComponent<FleetVessel>().Boosters++;
                }
            }
            else if (player.isBuyingBlasterUpgrade && hasLocalPlayerFleetVessel)
            {
                if (player.Purchase(PurchaseManager.UpgradeBlaster))
                {
                    NetworkServer.FindLocalObject(associatedShip).GetComponent<FleetVessel>().Blasters++;
                }
            }
            else if (player.isBuyingTractorBeamUpgrade && hasLocalPlayerFleetVessel)
            {
                if (player.Purchase(PurchaseManager.UpgradeTractorBeam))
                {
                    NetworkServer.FindLocalObject(associatedShip).GetComponent<FleetVessel>().TractorBeams++;
                }
            }
            else
            {
                if (NetworkServer.FindLocalObject(associatedShip).GetComponent<Ship>().IsDisabled) return false;

                //clicking on my ship space, select it
                selected = !selected;
                GameManager.singleton.selectedCell = selected ? this : null;

                if (selected)
                {
                    foreach (GameCell cell in NetworkServer.FindLocalObject(associatedShip).GetComponent<Ship>().nearbyCells)
                    {
                        if (cell.state == GameCellState.Empty || cell.state == GameCellState.ShipBuildArea ||
                            (cell.state == GameCellState.TempusSpace && player.reputation >= GameManager.singleton.tempusReputation))
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
        }
        else if (state == GameCellState.ShipBuildArea && owner == player.netId)
        {
            if (player.isBuyingFleetVessel && player.Purchase(PurchaseManager.FleetVessel))
            {
                GameObject objShip = (GameObject)Instantiate(prefabFleetVessel, transform.position, Quaternion.identity);
                NetworkServer.Spawn(objShip);
                FleetVessel ship = objShip.GetComponent<FleetVessel>();
                ship.Color = player.color;
                ship.ownerId = player.netId;
                ship.associatedCell = this.netId;
                player.fleetVessels.Add(ship);

                SetCell(player, GameCellState.Empty, true, ship.netId);
                return true;
            }
            else if (player.isBuyingColonyShip && player.Purchase(PurchaseManager.ColonyShip))
            {
                GameObject objShip = (GameObject)Instantiate(prefabColonyShip, transform.position, Quaternion.identity);
                NetworkServer.Spawn(objShip);
                ColonyShip ship = objShip.GetComponent<ColonyShip>();
                ship.Color = player.color;
                ship.ownerId = player.netId;
                ship.associatedCell = this.netId;

                SetCell(player, GameCellState.Empty, true, ship.netId);
                return true;
            }
        }
        else if (state == GameCellState.MovementArea && owner == player.netId)
        {
            foreach (GameCell cell in NetworkServer.FindLocalObject(GameManager.singleton.selectedCell.associatedShip).GetComponent<Ship>().nearbyCells)
            {
                if (cell.state == GameCellState.MovementArea)
                {
                    cell.Revert();
                }
            }
            //move the ship
            SetCell(player, true, GameManager.singleton.selectedCell.associatedShip);
            GameManager.singleton.selectedCell.SetCell(false, NetworkInstanceId.Invalid);            
            GameManager.singleton.selectedCell = null;
            NetworkServer.FindLocalObject(associatedShip).GetComponent<Ship>().MoveTo(this.netId);
            return true;
        }
        else if (state == GameCellState.Depot && owner == player.netId)
        {
            if (player.isBuyingStarport && player.Purchase(PurchaseManager.Starport))
            {
                SetCell(player, GameCellState.Starport);
                NetworkServer.Destroy(NetworkServer.FindLocalObject(associatedStation));
                GameObject obj = (GameObject)Instantiate(prefabStarport, transform.position, Quaternion.identity);
                Starport starport = obj.GetComponent<Starport>();
                starport.color = player.color;
                starport.owner = player;
                NetworkServer.Spawn(obj);
                associatedStation = starport.netId;
                //player.depots.Add(depot);
                return true;
            }
        }
        else if (state == GameCellState.DepotBuildArea && owner == player.netId)
        {
            if (player.isBuyingDepot && player.Purchase(PurchaseManager.Depot))
            {
                SetCell(player, GameCellState.Depot);
                GameObject objDepot = (GameObject)Instantiate(prefabDepot, transform.position, Quaternion.identity);
                Depot depot = objDepot.GetComponent<Depot>();
                depot.color = player.color;
                depot.owner = player;
                NetworkServer.Spawn(objDepot);
                associatedStation = depot.netId;
                //player.depots.Add(depot);
                return true;
            }
        }

        return false;
    }

    public bool hasLocalPlayerFleetVessel
    {
        get
        {
            return hasShip && owner == Player.localPlayer.netId && associatedShip != NetworkInstanceId.Invalid && ClientScene.FindLocalObject(associatedShip).GetComponent<FleetVessel>() != null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        GameCell cell = other.gameObject.GetComponent<GameCell>();

        if (cell != null && !adjacentCells.Contains(cell))
        {
            adjacentCells.Add(cell);
        }
    }

    void OnMouseOver()
    {
        if (hasLocalPlayerFleetVessel)
        {
            hoverTime += Time.deltaTime;
            if (hoverTime > tooltipDelay)
            {
                UIManager.singleton.ShowShipTooltip(ClientScene.FindLocalObject(associatedShip).GetComponent<FleetVessel>());
            }
        }
    }

    void OnMouseExit()
    {
        hoverTime = 0;
        UIManager.singleton.HideShipTooltip();
    }

}
