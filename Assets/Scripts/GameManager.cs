using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager singleton;

    public GameBoard gameBoard;
    public GameObject PlayerName;
    public GameObject ResourceCounter;
    public int numRows = 10;
    public int numCols = 10;
    public float boardSpacing = 1.05f;
    public GameCell selectedCell;
    
    public int activePlayerIndex = 0;
    public List<Player> players = new List<Player>();

    private GameObject playerNamePanel;
    private GameObject resourceCountPanel;
    public List<GameCell> cells = new List<GameCell>();

    public delegate void TurnStart();
    public static event TurnStart OnTurnStart;
        
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
        resourceCountPanel = GameObject.Find("ResourceCountPanel");
        
        foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
        {
            if (t != ResourceType.None)
            {
                CreateResourceCounter(t);
            }
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
    private void CreateResourceCounter(ResourceType type)
    {
        GameObject objCounter = Instantiate(ResourceCounter);
        objCounter.name = type.ToString();
        objCounter.transform.FindChild("ResourceName").GetComponent<Text>().text = type.ToString();
        objCounter.transform.FindChild("Count").GetComponent<Text>().text = "0";
        objCounter.transform.FindChild("Image").GetComponent<Image>().color = Resource.GetColor(type);
        objCounter.transform.SetParent(resourceCountPanel.transform);
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
        if (OnTurnStart != null)
        {
            OnTurnStart();
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
        if (OnTurnStart != null)
        {
            OnTurnStart();
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

    [Client]
    public void IncrementResource(ResourceType resource)
    {
        if (resource != ResourceType.None)
        {
            int count = Convert.ToInt32(resourceCountPanel.transform.FindChild(resource.ToString()).FindChild("Count").GetComponent<Text>().text);
            resourceCountPanel.transform.FindChild(resource.ToString()).FindChild("Count").GetComponent<Text>().text = (count + 1).ToString();
        }
    }

    [Client]
    public void DecrementResource(ResourceType resource)
    {
        if (resource != ResourceType.None)
        {
            int count = Convert.ToInt32(resourceCountPanel.transform.FindChild(resource.ToString()).FindChild("Count").GetComponent<Text>().text);
            resourceCountPanel.transform.FindChild(resource.ToString()).FindChild("Count").GetComponent<Text>().text = (count - 1).ToString();
        }
    }
}
