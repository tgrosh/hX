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

    private Vector3 boardPosition;
    private float cellAnimationSpeed = 3f;

    [SyncVar]
    public GameCellState state = GameCellState.Empty;
    [SyncVar]
    public NetworkInstanceId owner = NetworkInstanceId.Invalid;

    private List<GameObject> adjacent = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        boardPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Material cellMaterial = GetComponent<Renderer>().material;

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
            transform.position = Vector3.Lerp(transform.position, boardPosition + new Vector3(0, 0, -.4f), cellAnimationSpeed * Time.deltaTime);

            Player ownerPlayer = ClientScene.FindLocalObject(owner).GetComponent<Player>();
            GetComponent<Renderer>().material.color = Color.Lerp(cellMaterial.color, new Color(ownerPlayer.color.r, ownerPlayer.color.g, ownerPlayer.color.b, GetComponent<Renderer>().material.color.a), cellAnimationSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = boardPosition;
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

    [Server]
    bool ApplyRule(NetworkInstanceId playerId, GameRule rule)
    {
        bool result = false;

        if (rule.clickResult == CellAction.MakeOwnArea)
        {
            result = SetCell(playerId, GameCellState.Area);
        }
        else if (rule.clickResult == CellAction.MakeOwnCore)
        {
            result = SetCell(playerId, GameCellState.Core);
        }
        else if (rule.clickResult == CellAction.MakeEmpty)
        {
            result = SetCell(NetworkInstanceId.Invalid, GameCellState.Empty);
        }
        else if (rule.clickResult == CellAction.MakeEnemyArea)
        {
            result = SetCell(GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Empty);
        }
        else if (rule.clickResult == CellAction.MakeEnemyCore)
        {
            result = SetCell(GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Empty);
        }

        if (rule.cascadeEmptyResult == CellAction.MakeOwnArea)
        {
            result = SetAdjacentCells(this, NetworkInstanceId.Invalid, GameCellState.Empty, playerId, GameCellState.Area, rule.continueEmptyCascade) || result;
        }
        else if (rule.cascadeEmptyResult == CellAction.MakeOwnCore)
        {
            result = SetAdjacentCells(this, NetworkInstanceId.Invalid, GameCellState.Empty, playerId, GameCellState.Core, rule.continueEmptyCascade) || result;
        }
        else if (rule.cascadeEmptyResult == CellAction.MakeEnemyArea)
        {
            result = SetAdjacentCells(this, NetworkInstanceId.Invalid, GameCellState.Empty, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, rule.continueEmptyCascade) || result;
        }
        else if (rule.cascadeEmptyResult == CellAction.MakeEnemyCore)
        {
            result = SetAdjacentCells(this, NetworkInstanceId.Invalid, GameCellState.Empty, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, rule.continueEmptyCascade) || result;
        }

        if (rule.cascadeOwnAreaResult == CellAction.MakeEmpty)
        {
            result = SetAdjacentCells(this, playerId, GameCellState.Empty, NetworkInstanceId.Invalid, GameCellState.Empty, rule.continueOwnAreaCascade) || result;
        }
        else if (rule.cascadeOwnAreaResult == CellAction.MakeOwnCore)
        {
            result = SetAdjacentCells(this, playerId, GameCellState.Area, playerId, GameCellState.Core, rule.continueOwnAreaCascade) || result;
        }
        else if (rule.cascadeOwnAreaResult == CellAction.MakeEnemyArea)
        {
            result = SetAdjacentCells(this, playerId, GameCellState.Area, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, rule.continueOwnAreaCascade) || result;
        }
        else if (rule.cascadeOwnAreaResult == CellAction.MakeEnemyCore)
        {
            result = SetAdjacentCells(this, playerId, GameCellState.Area, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, rule.continueOwnAreaCascade) || result;
        }

        if (rule.cascadeEnemyAreaResult == CellAction.MakeEmpty)
        {
            result = SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, NetworkInstanceId.Invalid, GameCellState.Empty, rule.continueEnemyAreaCascade) || result;
        }
        else if (rule.cascadeEnemyAreaResult == CellAction.MakeOwnArea)
        {
            result = SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, playerId, GameCellState.Area, rule.continueEnemyAreaCascade) || result;
        }
        else if (rule.cascadeEnemyAreaResult == CellAction.MakeOwnCore)
        {
            result = SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, playerId, GameCellState.Core, rule.continueEnemyAreaCascade) || result;
        }
        else if (rule.cascadeOwnAreaResult == CellAction.MakeEnemyCore)
        {
            result = SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, rule.continueEnemyAreaCascade) || result;
        }

        if (rule.cascadeEnemyCoreResult == CellAction.MakeEmpty)
        {
            result = SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, NetworkInstanceId.Invalid, GameCellState.Empty, rule.continueEnemyCoreCascade) || result;
        }
        else if (rule.cascadeEnemyCoreResult == CellAction.MakeOwnArea)
        {
            result = SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, playerId, GameCellState.Area, rule.continueEnemyCoreCascade) || result;
        }
        else if (rule.cascadeEnemyCoreResult == CellAction.MakeOwnCore)
        {
            result = SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, playerId, GameCellState.Core, rule.continueEnemyCoreCascade) || result;
        }
        else if (rule.cascadeEnemyCoreResult == CellAction.MakeEnemyArea)
        {
            result = SetAdjacentCells(this, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Core, GameManager.singleton.GetPlayerOpponent(playerId), GameCellState.Area, rule.continueEnemyCoreCascade) || result;
        }

        return result;
    }

    [Server]
    bool SetCell(NetworkInstanceId playerId, GameCellState state)
    {
        return SetCell(this, playerId, state);
    }

    [Server]
    bool SetCell(GameCell cell, NetworkInstanceId playerId, GameCellState state)
    {
        bool result = (cell.owner != playerId || cell.state != state);

        cell.owner = playerId;
        cell.state = state;

        return result;
    }

    [Server]
    bool SetAdjacentCells(GameCell cell, NetworkInstanceId adjacentPlayerId, GameCellState adjacentCellState, NetworkInstanceId newPlayerId, GameCellState newCellState, bool continueCascade)
    {
        bool result = false;

        foreach (GameObject obj in cell.adjacent)
        {
            GameCell adjacentCell = obj.GetComponent<GameCell>();

            if (adjacentCell.state == adjacentCellState && adjacentCell.owner == adjacentPlayerId)
            {
                result = SetCell(adjacentCell, newPlayerId, newCellState) || result;

                if (continueCascade)
                {
                    SetAdjacentCells(adjacentCell, adjacentPlayerId, adjacentCellState, newPlayerId, newCellState, continueCascade);
                }
            }
        }

        return result;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!adjacent.Contains(other.gameObject))
        {
            adjacent.Add(other.gameObject);
        }
    }

}
