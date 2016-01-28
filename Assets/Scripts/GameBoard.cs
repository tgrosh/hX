using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameBoard : NetworkBehaviour {

    public GameObject emptyBoardSpace;
            
    [Server]
    public void SpawnBoard()
    {
        foreach (GameCell cell in transform.GetComponentsInChildren<GameCell>())
        {
            NetworkServer.Spawn(cell.gameObject);
        }
    }
}
