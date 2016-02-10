using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.Networking;

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
            GameObject.Find("Hotbar").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
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
            MenuManager.singleton.ShowResourceTracker();
        }        
    }

    [ClientRpc]
    public void Rpc_StartTurn()
    {
        if (isLocalPlayer)
        {
            GameManager.singleton.ResetCamera();
            GameObject.Find("Hotbar").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 45f);
            MenuManager.singleton.ShowYourTurn();
        }
    }

    [Command]
    public void Cmd_SetIsBuyingShip(bool isBuyingShip)
    {
        this.isBuyingShip = isBuyingShip;
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
}
