using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameCell : NetworkBehaviour {

    public Material Empty;
    public Material Core;
    public Material Area;

    public GameCellState state = GameCellState.Empty;
    public Player owner = null;

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
        else if (state == GameCellState.Core)
        {
            GetComponent<Renderer>().material = Core;
        }
        else if (state == GameCellState.Area)
        {
            GetComponent<Renderer>().material = Area;
        }

        if (owner)
        {
            GetComponent<Renderer>().material.color = new Color(owner.color.r, owner.color.g, owner.color.b, GetComponent<Renderer>().material.color.a);
        }
	}

    public void Select(Player player)
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

    public void SelectCore(Player player)
    {
        if (state == GameCellState.Empty || (owner == player && state == GameCellState.Area))
        {
            SetCore(player, true, true);            
        }
    }

    public void SetArea(Player player)
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

    private void SetCore(Player player, bool cascadeAreas, bool cascadeCores)
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
