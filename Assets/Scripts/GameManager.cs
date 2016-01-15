using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public GameBoard gameBoard;
    public Camera gameCamera;
    public int numRows = 10;
    public int numCols = 10;
    public float boardSpacing = 1.05f;
    public Player currentPlayer;
    
    private List<Player> players = new List<Player>();
    
	// Use this for initialization
    void Start()
    {
        gameBoard.CreateBoard(10, 10, 1.05f);
        gameCamera.transform.LookAt(gameBoard.transform);
	}

    public void AddPlayer(Player player)
    {
        players.Add(player);

        if (currentPlayer == null)
        {
            currentPlayer = player;
        }
    }
	
    public void EndPlayerTurn()
    {
        currentPlayer.GetComponent<NetworkIdentity>().localPlayerAuthority = false;

        if (players[0] == currentPlayer && players.Count > 1)
        {
            currentPlayer = players[1];
        }
        else
        {
            currentPlayer = players[0];
        }

        currentPlayer.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
    }

    public void RemovePlayer(Player player)
    {
        players.Remove(player);
    }
}
