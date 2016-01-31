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
    public GameObject PlayerName;
    public int numRows = 10;
    public int numCols = 10;
    public float boardSpacing = 1.05f;
    public GameCell selectedCell;

    public int activePlayerIndex = 0;
    public List<Player> players = new List<Player>();

    private GameObject playerNamePanel;
    public List<GameCell> cells = new List<GameCell>();
    
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

    public override void OnStartClient()
    {
        gameBoard.gameObject.SetActive(false);
        playerNamePanel = GameObject.Find("PlayerNamesPanel");
        
        foreach (Transform t in playerNamePanel.transform)
        {
            GameObject.Destroy(t.gameObject);
        }

        foreach (Player player in GameObject.FindObjectsOfType<Player>())
        {
            CreatePlayerNameText(player.playerName, player.playerActive == true ? player.color : Color.gray, 34);
            CreatePlayerNameText(player.score.ToString(), player.playerActive == true ? player.color : Color.gray, 24);
        }
    }

    void Update()
    {
        foreach (Player player in players)
        {
            player.playerActive = player == activePlayer;
            player.score = 0;
        }

        if (isServer)
        {
            foreach (GameCell cell in cells)
            {
                if (selectedCell != null)
                {
                    cell.selected = cell == selectedCell;
                }
                else
                {
                    cell.selected = false;
                    if (cell.state == GameCellState.MovementArea)
                    {
                        cell.Revert();
                    }
                }
            }
        }
    }

    [Client]
    private void CreatePlayerNameText(string text, Color color, int fontSize)
    {
        GameObject objPlayerName = Instantiate(PlayerName);
        Text txt = objPlayerName.GetComponent<Text>();
        txt.text = text;
        txt.color = color;
        txt.fontSize = fontSize;
        objPlayerName.transform.SetParent(playerNamePanel.transform);
    }

    [Server]
    public void StartGame()
    {
        gameBoard.SpawnBoard();
        foreach (Player player in players)
        {
            player.Rpc_StartGame();
        }
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

    [Server]
    public NetworkInstanceId GetPlayerOpponent(NetworkInstanceId netId)
    {
        return GetPlayerOpponent(NetworkServer.FindLocalObject(netId).GetComponent<Player>()).netId;
    }

    [Server]
    public Player GetPlayerOpponent(Player player)
    {        
        return players.Find((Player p) => { return p.netId != player.netId; });
    }

    [Server]
    public Player PlayerAtSeat(PlayerSeat seat)
    {
        return players.Find((Player p) => { return p.seat == seat; });
    }
}
