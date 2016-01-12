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

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SelectCore(PlayerType player)
    {
        if (state == GameCellState.Empty)
        {
            MakeCore(player, true);
        }
    }

    public void SelectArea(PlayerType player)
    {
        if (state == GameCellState.Empty)
        {
            if (player == PlayerType.One)
            {
                GetComponent<Renderer>().material = PlayerOneArea;
            }
            state = GameCellState.PlayerOneArea;
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
            }
        }
        else if (state == GameCellState.PlayerTwoArea)
        {
            if (player == PlayerType.One)
            {

            }
            else 
            { 
            }
        }
    }

    private void MakeCore(PlayerType player, bool cascade)
    {
        GetComponent<Renderer>().material = PlayerOneCore;
        state = GameCellState.PlayerOneCore;

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
