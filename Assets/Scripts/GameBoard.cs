using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameBoard : NetworkBehaviour {

    public GameCell emptyBoardSpace;
            
    [Server]
    public void SpawnBoard()
    {
        foreach (GameCell cell in transform.GetComponentsInChildren<GameCell>())
        {
            GameCell obj = (GameCell)Instantiate(emptyBoardSpace, cell.transform.position, cell.transform.localRotation);
            obj.state = cell.state;
            obj.ownerSeat = cell.ownerSeat;
            GameManager.singleton.cells.Add(obj);

            NetworkServer.Spawn(obj.gameObject);
        }
    }
}
