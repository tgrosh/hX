﻿using UnityEngine;
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

    public Base playerBase;

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
    }
    
    public bool CanAfford(Purchase purchase)
    {
        bool result = true;

        if (playerBase == null) return false;

        foreach (PurchaseCost cost in purchase.cost)
        {
            result = playerBase.cargoHold.GetCargo(cost.resource).Count >= cost.quantity && result;
        }

        return result;
    }

    public bool Purchase(Purchase purchase)
    {
        bool result = true;

        if (playerBase == null) return false;

        if (CanAfford(purchase))
        {
            foreach (PurchaseCost cost in purchase.cost)
            {
                playerBase.cargoHold.Dump(cost.resource, cost.quantity);
            }
            result = true;
        }
        else
        {
            result = false;
        }        

        return result;
    }

    [Client]
    public void EndTurn()
    {
        if (isLocalPlayer)
        {
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
            if (NetworkServer.FindLocalObject(cellId).GetComponent<GameCell>().Select(this))
            {
                GameManager.singleton.EndPlayerTurn();
            }
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
            //Camera.main.transform.LookAt(gameBoard);
        }        
    }

    [Command]
    public void Cmd_SetIsBuyingShip(bool isBuyingShip)
    {
        this.isBuyingShip = isBuyingShip;
    }
}
