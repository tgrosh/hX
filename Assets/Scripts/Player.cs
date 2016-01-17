using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
    public static Player localPlayer;

    [SyncVar]
    public Color color;

	// Use this for initialization
    void Start()
    {
        if (isLocalPlayer)
        {
            localPlayer = this;
        }
	}
    
    [Client]
    public void SelectCell(string cellName)
    {
        if (isLocalPlayer)
        {
            Cmd_SelectCell(cellName);
        }
    }

    [Client]
    public void EndTurn()
    {
        if (isLocalPlayer)
        {
            Cmd_EndTurn();
        }
    }
    
    [Server]
    public void StartGame()
    {
        Rpc_StartGame();
    }

    [Command]
    private void Cmd_SelectCell(string cellName)
    {
        if (GameManager.singleton.activePlayer == this)
        {
            //rpc call back to all clients, but only the active player object will get the callback
            Rpc_SelectCell(cellName);
        }        
    }

    [Command]
    private void Cmd_EndTurn()
    {
        GameManager.singleton.EndPlayerTurn();
    }

    [ClientRpc]
    private void Rpc_SelectCell(string cellName)
    {
        GameObject.Find(cellName).GetComponent<GameCell>().Select(this);
    }
    
    [ClientRpc]
    private void Rpc_StartGame()
    {
        if (isLocalPlayer) { 
            GameManager.singleton.StartGame();
        }
    }
}
