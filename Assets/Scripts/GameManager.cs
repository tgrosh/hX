using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityStandardAssets.Cameras;

public class GameManager : NetworkBehaviour
{
    public static GameManager singleton;
    public AutoCam cam;
    public GameBoard gameBoard;
    public GameObject PlayerName;
    public GameObject ResourceCounter;
    public GameObject EventLogEntryPrefab;
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

    public List<string> events = new List<string>();
        
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
        cam = Camera.main.GetComponent<AutoCam>();

        foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
        {
            if (t != ResourceType.None)
            {
                CreateResourceCounter(t);
            }
        }
    }

    public void ResetCamera()
    {
        cam.SetTarget(GameObject.Find("DefaultCameraTarget").transform);
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

    [ClientRpc]
    private void Rpc_AddEvent(string text)
    {
        GameObject.Find("MostRecentEvent").GetComponent<Text>().text = text;
        events.Add(text);
        PopulateEventLog(events);
    }

    [Client]
    private void PopulateEventLog(List<string> events)
    {
        GameObject eventLogContent = GameObject.Find("EventLogContent");
        GameObject fullEventLog = GameObject.Find("FullEventLog");
        float newHeight = 14;

        foreach (Transform child in eventLogContent.transform)
        {
            Destroy(child.gameObject);
        }

        
        //.GetRange(Mathf.Max(events.Count - 41, 0), Mathf.Min(40, events.Count))
        foreach (string s in events)
        {
            GameObject obj = Instantiate(EventLogEntryPrefab);
            obj.GetComponent<Text>().text = s;

            //increase content area to be big enough to support the new item
            eventLogContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 1200);
            obj.transform.SetParent(eventLogContent.transform);

            newHeight += obj.GetComponent<RectTransform>().rect.height + 3;
            newHeight = Mathf.Max(newHeight, 200);
            eventLogContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, newHeight);
        }

        Canvas.ForceUpdateCanvases();
        fullEventLog.GetComponent<ScrollRect>().verticalScrollbar.value = 0f;
        Canvas.ForceUpdateCanvases();
    }

    [Server]
    public void AddEvent(string text) {
        Rpc_AddEvent(text);
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
        GameManager.singleton.AddEvent(String.Format("Player {0}'s turn has ended", GameManager.singleton.CreateColoredText(players[activePlayerIndex].seat.ToString(), players[activePlayerIndex].color)));                
        activePlayerIndex++;
        if (activePlayerIndex >= players.Count)
        {
            activePlayerIndex = 0;
        }

        GameManager.singleton.AddEvent(String.Format("Player {0}'s turn has started", GameManager.singleton.CreateColoredText(players[activePlayerIndex].seat.ToString(), players[activePlayerIndex].color)));
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

    public string CreateColoredText(String text, Color color)
    {
        return "<color=#" + ColorToHex(color) + ">" + text + "</color>";
    }

    public string ColorToHex(Color color)
    {
        return Convert.ToInt32(color.r * 255).ToString("X2") + Convert.ToInt32(color.g * 255).ToString("X2") + Convert.ToInt32(color.b * 255).ToString("X2");
    }
}
