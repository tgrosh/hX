using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
    
    [SyncVar]
    public PlayerType type = PlayerType.None;
    [SyncVar]
    public Color color;

	// Use this for initialization
    void Start()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().AddPlayer(this);
	}

    [Command]
    public void Cmd_SelectCell(string cellName)
    {
        if (!isLocalPlayer) return;

        Rpc_SelectCell(cellName);
    }

    [ClientRpc]
    public void Rpc_SelectCell(string cellName)
    {
        GameObject.Find(cellName).GetComponent<GameCell>().Select(this.type);
    }

    [Command]
    public void Cmd_EndTurn()
    {
        if (!isLocalPlayer) return;

        Rpc_EndTurn();
    }

    [ClientRpc]
    public void Rpc_EndTurn()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().EndPlayerTurn();
    }
}
