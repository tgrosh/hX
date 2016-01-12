using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;

public class GameCell : MonoBehaviour {

    public Material PlayerOneCore;
    public Material PlayerOneArea;
    public Material PlayerTwoCore;
    public Material PlayerTwoArea;

    private GameCellState state = GameCellState.Empty;
    private List<GameObject> adjacent = new List<GameObject>();
    private GameManager gameManager;

	// Use this for initialization
	void Start () {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
        if (state == GameCellState.PlayerOneCore)
        {
            GetComponent<Renderer>().material = PlayerOneCore;
        }
        else if (state == GameCellState.PlayerOneArea)
        {
            GetComponent<Renderer>().material = PlayerOneArea;
        }
        else if (state == GameCellState.PlayerTwoCore)
        {
            GetComponent<Renderer>().material = PlayerTwoCore;
        }
        else if (state == GameCellState.PlayerTwoArea)
        {
            GetComponent<Renderer>().material = PlayerTwoArea;
        }
	}

    public void SelectCore(PlayerType player)
    {
        if (state == GameCellState.Empty)
        {
            MakeCore(player, true);
            gameManager.EndPlayerTurn();
        }
    }

    public void SelectArea(PlayerType player)
    {
        if (state == GameCellState.Empty)
        {
            if (player == PlayerType.One)
            {
                state = GameCellState.PlayerOneArea;
            }
            else
            {
                state = GameCellState.PlayerTwoArea;
            }            
        }
        else if (state == GameCellState.PlayerOneArea)
        {
            if (player == PlayerType.One)
            {
                //already an area, make it a core         
                MakeCore(PlayerType.One, false);
            }
            else
            {
                state = GameCellState.PlayerTwoArea;
            }
        }
        else if (state == GameCellState.PlayerTwoArea)
        {
            if (player == PlayerType.One)
            {
                state = GameCellState.PlayerOneArea;
            }
            else
            { 
                //already an area, make it a core         
                MakeCore(PlayerType.Two, false);
            }
        }
    }

    private void MakeCore(PlayerType player, bool cascade)
    {
        if (player == PlayerType.One)
        {
            state = GameCellState.PlayerOneCore;
        }
        else
        {
            state = GameCellState.PlayerTwoCore;
        }        

        if (cascade)
        {
            foreach (GameObject obj in adjacent)
            {
                obj.GetComponent<GameCell>().SelectArea(player);
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
