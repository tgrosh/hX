using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager singleton;

    public GameBoard gameBoard;
    public Camera gameCamera;
    public GameObject PlayerName;
    public int numRows = 10;
    public int numCols = 10;
    public float boardSpacing = 1.05f;

    public int activePlayerIndex = 0;
    public List<Player> players = new List<Player>();

    private GameObject playerNamePanel;

    public Player activePlayer
    {
        get
        {
            return players[activePlayerIndex];
        }
    }

    void Awake()
    {
        singleton = this;
    }

    void Start()
    {
        playerNamePanel = GameObject.Find("PlayerNamesPanel");
    }

    void Update()
    {
        foreach (Transform t in playerNamePanel.transform)
        {
            GameObject.Destroy(t.gameObject);
        }

        foreach (Player player in GameObject.FindObjectsOfType<Player>())
        {
            GameObject obj = Instantiate(PlayerName);
            obj.GetComponent<Text>().text = player.playerName;
            obj.transform.SetParent(playerNamePanel.transform);
        }
    }
    
    [Server]
    public void StartGame()
    {
        gameBoard.CreateBoard(10, 10, 1.05f);
        //gameCamera.transform.LookAt(gameBoard.transform);
    }
    
    [Server]
    public void EndPlayerTurn()
    {
        activePlayerIndex++;
        if (activePlayerIndex >= players.Count)
        {
            activePlayerIndex = 0;
        }
    }
    
    [Server]
    public void AddPlayer(Player player)
    {
        players.Add(player);

        if (players.Count == 2)
        {
            StartGame();
        }
    }

    [Server]
    public void RemovePlayer(Player player)
    {
        players.Remove(player);
    }
}
