using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameCell : NetworkBehaviour
{
    public Material Empty;
    public Material Core;
    public Material Area;

    [SyncVar]
    public GameCellState state = GameCellState.Empty;
    [SyncVar]
    public NetworkInstanceId owner = NetworkInstanceId.Invalid;

    private List<GameObject> adjacent = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
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

        if (owner != NetworkInstanceId.Invalid)
        {
            Player ownerPlayer = ClientScene.FindLocalObject(owner).GetComponent<Player>();
            GetComponent<Renderer>().material.color = new Color(ownerPlayer.color.r, ownerPlayer.color.g, ownerPlayer.color.b, GetComponent<Renderer>().material.color.a);
        }
    }

    [Server]
    public bool Select(NetworkInstanceId playerId)
    {
        GameRule rule = null;

        if (state == GameCellState.Empty)
        {
            rule = GameRuleManager.singleton.ruleEmpty;
        }
        else if (owner == playerId && state == GameCellState.Area)
        {
            rule = GameRuleManager.singleton.ruleOwnArea;
        }
        else if (owner == playerId && state == GameCellState.Core)
        {
            rule = GameRuleManager.singleton.ruleOwnCore;
        }
        else if (owner != playerId && state == GameCellState.Area)
        {
            rule = GameRuleManager.singleton.ruleEnemyArea;
        }
        else if (owner != playerId && state == GameCellState.Core)
        {
            rule = GameRuleManager.singleton.ruleEnemyCore;
        }

        return ApplyRule(playerId, rule);
    }
        
    bool ApplyRule(NetworkInstanceId playerId, GameRule rule)
    {
        bool result = false;

        if (rule.clickResult == CellAction.MakeOwnArea)
        {
            SetCell(playerId, GameCellState.Area);
            result = true;
        }
        else if (rule.clickResult == CellAction.MakeOwnCore)
        {
            SetCell(playerId, GameCellState.Core);
            result = true;
        }
        else if (rule.clickResult == CellAction.MakeEmpty)
        {
            SetCell(NetworkInstanceId.Invalid, GameCellState.Empty);
            result = true;
        }
        else if (rule.clickResult == CellAction.MakeEnemyArea)
        {
            SetCell(GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Empty);
            result = true;
        }
        else if (rule.clickResult == CellAction.MakeEnemyCore)
        {
            SetCell(GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Empty);
            result = true;
        }

        if (rule.cascadeEmptyResult == CellAction.MakeOwnArea)
        {
            SetAdjacentCells(this, NetworkInstanceId.Invalid, GameCellState.Empty, playerId, GameCellState.Area, rule.continueEmptyCascade);
        }
        else if (rule.cascadeEmptyResult == CellAction.MakeOwnCore)
        {
            SetAdjacentCells(this, NetworkInstanceId.Invalid, GameCellState.Empty, playerId, GameCellState.Core, rule.continueEmptyCascade);
        }
        else if (rule.cascadeEmptyResult == CellAction.MakeEnemyArea)
        {
            SetAdjacentCells(this, NetworkInstanceId.Invalid, GameCellState.Empty, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, rule.continueEmptyCascade);
        }
        else if (rule.cascadeEmptyResult == CellAction.MakeEnemyCore)
        {
            SetAdjacentCells(this, NetworkInstanceId.Invalid, GameCellState.Empty, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, rule.continueEmptyCascade);
        }

        if (rule.cascadeOwnAreaResult == CellAction.MakeEmpty)
        {
            SetAdjacentCells(this, playerId, GameCellState.Empty, NetworkInstanceId.Invalid, GameCellState.Empty, rule.continueOwnAreaCascade);
        }
        else if (rule.cascadeOwnAreaResult == CellAction.MakeOwnCore)
        {
            SetAdjacentCells(this, playerId, GameCellState.Area, playerId, GameCellState.Core, rule.continueOwnAreaCascade);
        }
        else if (rule.cascadeOwnAreaResult == CellAction.MakeEnemyArea)
        {
            SetAdjacentCells(this, playerId, GameCellState.Area, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, rule.continueOwnAreaCascade);
        }
        else if (rule.cascadeOwnAreaResult == CellAction.MakeEnemyCore)
        {
            SetAdjacentCells(this, playerId, GameCellState.Area, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, rule.continueOwnAreaCascade);
        }

        if (rule.cascadeEnemyAreaResult == CellAction.MakeEmpty)
        {
            SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, NetworkInstanceId.Invalid, GameCellState.Empty, rule.continueEnemyAreaCascade);
        }
        else if (rule.cascadeEnemyAreaResult == CellAction.MakeOwnArea)
        {
            SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, playerId, GameCellState.Area, rule.continueEnemyAreaCascade);
        }
        else if (rule.cascadeEnemyAreaResult == CellAction.MakeOwnCore)
        {
            SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, playerId, GameCellState.Core, rule.continueEnemyAreaCascade);
        }
        else if (rule.cascadeOwnAreaResult == CellAction.MakeEnemyCore)
        {
            SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, rule.continueEnemyAreaCascade);
        }

        if (rule.cascadeEnemyCoreResult == CellAction.MakeEmpty)
        {
            SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, NetworkInstanceId.Invalid, GameCellState.Empty, rule.continueEnemyCoreCascade);
        }
        else if (rule.cascadeEnemyCoreResult == CellAction.MakeOwnArea)
        {
            SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, playerId, GameCellState.Area, rule.continueEnemyCoreCascade);
        }
        else if (rule.cascadeEnemyCoreResult == CellAction.MakeOwnCore)
        {
            SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, playerId, GameCellState.Core, rule.continueEnemyCoreCascade);
        }
        else if (rule.cascadeEnemyCoreResult == CellAction.MakeEnemyArea)
        {
            SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, rule.continueEnemyCoreCascade);
        }

        return result;
    }
    
    void SetCell(NetworkInstanceId playerId, GameCellState state)
    {
        SetCell(this, playerId, state);
    }

    void SetCell(GameCell cell, NetworkInstanceId playerId, GameCellState state)
    {
        cell.owner = playerId;
        cell.state = state;
    }

    void SetAdjacentCells(GameCell cell, NetworkInstanceId adjacentPlayerId, GameCellState adjacentCellState, NetworkInstanceId newPlayerId, GameCellState newCellState, bool continueCascade)
    {
        foreach (GameObject obj in cell.adjacent)
        {
            GameCell adjacentCell = obj.GetComponent<GameCell>();

            if (adjacentCell.state == adjacentCellState && adjacentCell.owner == adjacentPlayerId)
            {
                SetCell(adjacentCell, newPlayerId, newCellState);

                if (continueCascade)
                {
                    SetAdjacentCells(adjacentCell, adjacentPlayerId, adjacentCellState, newPlayerId, newCellState, continueCascade);
                }
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!adjacent.Contains(other.gameObject))
        {
            adjacent.Add(other.gameObject);
        }
    }

}
