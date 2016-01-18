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
    
    [Command]
    private void Cmd_SelectCell(NetworkInstanceId cellId)
    {
        if (GameManager.singleton.activePlayer == this)
        {
            if (NetworkServer.FindLocalObject(cellId).GetComponent<GameCell>().Select(this.netId))
            {
                GameManager.singleton.EndPlayerTurn();
            }
        }        
    }

    [Command]
    private void Cmd_EndTurn()
    {
        GameManager.singleton.EndPlayerTurn();
    }
}
