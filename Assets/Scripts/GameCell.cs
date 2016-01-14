using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameCell : NetworkBehaviour {

    public Material Empty;
    public Material PlayerOneCore;
    public Material PlayerOneArea;
    public Material PlayerTwoCore;
    public Material PlayerTwoArea;

    [SyncVar]
    public GameCellState state = GameCellState.Empty;
    [SyncVar]
    public PlayerType owner = PlayerType.None;

    private List<GameObject> adjacent = new List<GameObject>();
    private GameManager gameManager;
    
	// Use this for initialization
	void Start () {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
        if (state == GameCellState.Empty)
        {
            GetComponent<Renderer>().material = Empty;
        }
        else if (state == GameCellState.Core && owner == PlayerType.One)
        {
            GetComponent<Renderer>().material = PlayerOneCore;
        }
        else if (state == GameCellState.Area && owner == PlayerType.One)
        {
            GetComponent<Renderer>().material = PlayerOneArea;
        }
        else if (state == GameCellState.Core && owner == PlayerType.Two)
        {
            GetComponent<Renderer>().material = PlayerTwoCore;
        }
        else if (state == GameCellState.Area && owner == PlayerType.Two)
        {
            GetComponent<Renderer>().material = PlayerTwoArea;
        }
	}

    public void Select(PlayerType player)
    {
        if (state == GameCellState.Empty)
        {
            SetArea(player);
            gameManager.EndPlayerTurn();
        }
        else if (owner == player && state == GameCellState.Area)
        {
            SetCore(player, true, true);
            gameManager.EndPlayerTurn();
        }
    }

    public void SelectCore(PlayerType player)
    {
        if (state == GameCellState.Empty || (owner == player && state == GameCellState.Area))
        {
            SetCore(player, true, true);            
        }
    }

    public void SetArea(PlayerType player)
    {
        if (state == GameCellState.Empty)
        {
            state = GameCellState.Area;
            owner = player;
        }
        else if (state == GameCellState.Area)
        {
            if (player == owner)
            {
                //already an area, make it a core         
                SetCore(player, false, false);
            }
            else
            {
                state = GameCellState.Area;
                owner = player;
            }
        }
    }

    private void SetCore(PlayerType player, bool cascadeAreas, bool cascadeCores)
    {
        state = GameCellState.Core;
        owner = player;

        if (cascadeAreas)
        {
            foreach (GameObject obj in adjacent)
            {
                GameCell cell = obj.GetComponent<GameCell>();

                if (cell.state != GameCellState.Core)
                {
                    cell.SetArea(player);
                }
            }
        }

        if (cascadeCores)
        {
            foreach (GameObject obj in adjacent)
            {
                GameCell cell = obj.GetComponent<GameCell>();

                if (cell.state == GameCellState.Core && cell.owner != player)
                {
                    cell.SetCore(player, true, true);
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
