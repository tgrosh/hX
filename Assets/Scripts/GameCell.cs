using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameCell : NetworkBehaviour {

    public Material Empty;
    public Material Core;
    public Material Area;

    [SyncVar]
    public GameCellState state = GameCellState.Empty;
    [SyncVar]
    public NetworkInstanceId owner;

    private List<GameObject> adjacent = new List<GameObject>();
    
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (state == GameCellState.Empty)
        {
            GetComponent<Renderer>().material = Empty;
        }
        else if (state == GameCellState.Core)
        {
            GetComponent<Renderer>().material = Core;
        }
        else if (state == GameCellState.Area)
        {
            GetComponent<Renderer>().material = Area;
        }

        if (owner.Value != uint.MinValue)
        {
            Player ownerPlayer = ClientScene.FindLocalObject(owner).GetComponent<Player>();
            GetComponent<Renderer>().material.color = new Color(ownerPlayer.color.r, ownerPlayer.color.g, ownerPlayer.color.b, GetComponent<Renderer>().material.color.a);            
        }
	}

    [Server]
    public bool Select(NetworkInstanceId playerId)
    {
        if (state == GameCellState.Empty)
        {
            SetArea(playerId);
            return true;
        }
        else if (owner == playerId && state == GameCellState.Area)
        {
            SetCore(playerId, true, true);
            return true;
        }

        return false;
    }
    
    void SetArea(NetworkInstanceId playerId)
    {
        if (state == GameCellState.Empty)
        {
            state = GameCellState.Area;
            owner = playerId;
        }
        else if (state == GameCellState.Area)
        {
            if (playerId == owner)
            {
                //already an area, make it a core         
                SetCore(playerId, false, false);
            }
            else
            {
                state = GameCellState.Area;
                owner = playerId;
            }
        }
    }

    void SetCore(NetworkInstanceId playerId, bool cascadeAreas, bool cascadeCores)
    {
        state = GameCellState.Core;
        owner = playerId;

        if (cascadeAreas)
        {
            foreach (GameObject obj in adjacent)
            {
                GameCell cell = obj.GetComponent<GameCell>();

                if (cell.state != GameCellState.Core)
                {
                    cell.SetArea(playerId);
                }
            }
        }

        if (cascadeCores)
        {
            foreach (GameObject obj in adjacent)
            {
                GameCell cell = obj.GetComponent<GameCell>();

                if (cell.state == GameCellState.Core && cell.owner != playerId)
                {
                    cell.SetCore(playerId, true, true);
                }
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!adjacent.Contains(other.gameObject))
        {
            adjacent.Add(other.gameObject);
        }
    }

}
