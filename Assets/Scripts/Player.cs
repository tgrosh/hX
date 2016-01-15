using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
    
    [SyncVar]
    public Color color;

	// Use this for initialization
    void Start()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().AddPlayer(this);
	}

    public void SelectCell(string cellName) {
        if (isLocalPlayer)
        {
            Cmd_SelectCell(cellName);
        }
    }

    [Command]
    public void Cmd_SelectCell(string cellName)
    {
        Rpc_SelectCell(cellName);
    }

    [ClientRpc]
    public void Rpc_SelectCell(string cellName)
    {
        GameObject.Find(cellName).GetComponent<GameCell>().Select(this);
    }

    [Command]
    public void Cmd_EndTurn()
    {
        Rpc_EndTurn();
    }

    [ClientRpc]
    public void Rpc_EndTurn()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().EndPlayerTurn();
    }
    
    private Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }
}
