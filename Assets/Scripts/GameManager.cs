using UnityEngine;
using System.Collections;
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
    public GameCell selectedCell;    
    public int activePlayerIndex = 0;
    public List<Player> players = new List<Player>();
    public List<GameCell> cells = new List<GameCell>();     
    public Player activePlayer
    {
        get
        {
            return players[activePlayerIndex];
        }
    }
    public int tempusReputation = 20;

    public delegate void TurnStart();
    public static event TurnStart OnTurnStart;

    public delegate void RoundStart();
    public static event RoundStart OnRoundStart;

    private float dieDisplayXOffset = 0f;

    [SyncVar]
    public bool gameBoardLocked;

    void Awake()
    {
        singleton = this;

        //GameObject.Find("d6-1").GetComponent<d6>().Roll();
        //GameObject.Find("d6-2").GetComponent<d6>().Roll();
        //d6.OnDiceRollComplete += d6_OnDiceRollComplete;

        //dieDisplayPosition = Camera.main.GetComponent<AutoCam>().transform.position + Vector3.forward * 10f;        
    }

    void d6_OnDiceRollComplete(d6 die)
    {
        if (dieDisplayXOffset == 0)
        {
            dieDisplayXOffset = -1f;
        }
        else if (dieDisplayXOffset == -1f)
        {
            dieDisplayXOffset = 1f;
        }

        die.Display(new Vector3(dieDisplayXOffset, 0, 0));
    }

    public override void OnStartClient()
    {
        gameBoard.gameObject.SetActive(false);
        cam = Camera.main.GetComponent<AutoCam>();

        FleetVessel.OnShipMoveStart += Ship_OnShipMoveStart;
        FleetVessel.OnShipMoveEnd += Ship_OnShipMoveEnd;
        FleetVessel.OnShipSpawnStart += Ship_OnShipSpawnStart;
        FleetVessel.OnShipSpawnEnd += Ship_OnShipSpawnEnd;
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

    void Ship_OnShipMoveEnd(Ship ship)
    {
        if (ship.ownerId == Player.localPlayer.netId)
        {
            gameBoardLocked = false;
        }
    }

    void Ship_OnShipMoveStart(Ship ship)
    {
        if (ship.ownerId == Player.localPlayer.netId)
        {
            gameBoardLocked = true;
        }
    }

    void Ship_OnShipSpawnEnd(Ship ship)
    {
        gameBoardLocked = false;
    }

    void Ship_OnShipSpawnStart(Ship ship)
    {
        gameBoardLocked = true;
    }
            
    [Server]
    public void StartGame()
    {        
        gameBoard.SpawnBoard();
        foreach (Player player in players)
        {
            player.Rpc_StartGame();
        }
        if (OnRoundStart != null)
        {
            OnRoundStart();
        }
        if (OnTurnStart != null)
        {
            OnTurnStart();
        }
    }

    [Server]
    public void EndPlayerTurn()
    {
        EventLog.singleton.AddEvent(String.Format("Player {0}'s turn has ended", EventLog.singleton.CreateColoredText(players[activePlayerIndex].seat.ToString(), players[activePlayerIndex].color)));                
        activePlayerIndex++;
        if (activePlayerIndex >= players.Count)
        {
            activePlayerIndex = 0;
        }

        EventLog.singleton.AddEvent(String.Format("Player {0}'s turn has started", EventLog.singleton.CreateColoredText(players[activePlayerIndex].seat.ToString(), players[activePlayerIndex].color)));

        if (activePlayerIndex == 0 && OnRoundStart != null)
        {
            OnRoundStart();
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
}
