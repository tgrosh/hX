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
    public GameObject hex;

    private Vector3 boardPosition;
    private float cellAnimationSpeed = 3f;
    private float cellAnimationTime = 0f;
    private Vector3 cellPopoutDestination;
    private float cellPopoutZArea = -.4f;
    private float cellPopoutZCore = -.8f;
    private bool animating;
    private Color cellColorTarget = Color.white;
    private Player ownerPlayer;

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
        if (owner != NetworkInstanceId.Invalid)
        {
            Material cellMaterial = hex.GetComponent<Renderer>().material;
            
            if (state == GameCellState.Core && !cellMaterial.name.Contains(Core.name))
            {
                ownerPlayer = ClientScene.FindLocalObject(owner).GetComponent<Player>();
                cellColorTarget = new Color(ownerPlayer.color.r, ownerPlayer.color.g, ownerPlayer.color.b, hex.GetComponent<Renderer>().material.color.a);
                cellPopoutDestination = boardPosition + new Vector3(0, 0, cellPopoutZCore);
                hex.GetComponent<Renderer>().material = Core;
                animating = true;
            }
            else if (state == GameCellState.Area && !cellMaterial.name.Contains(Area.name))
            {
                ownerPlayer = ClientScene.FindLocalObject(owner).GetComponent<Player>();
                cellColorTarget = new Color(ownerPlayer.color.r, ownerPlayer.color.g, ownerPlayer.color.b, hex.GetComponent<Renderer>().material.color.a);
                cellPopoutDestination = boardPosition + new Vector3(0, 0, cellPopoutZArea);
                hex.GetComponent<Renderer>().material = Area;
                animating = true;
            }

            if (animating && cellAnimationTime / cellAnimationSpeed < .9f) {
                transform.position = Vector3.Lerp(transform.position, cellPopoutDestination, cellAnimationSpeed * Time.deltaTime);
                hex.GetComponent<Renderer>().material.color = Color.Lerp(cellMaterial.color, cellColorTarget, cellAnimationSpeed * Time.deltaTime);

                cellAnimationTime += Time.deltaTime;
            }
            else if (cellAnimationTime < cellAnimationSpeed)
            {
                transform.position = cellPopoutDestination;
                hex.GetComponent<Renderer>().material.color = cellColorTarget;

                cellAnimationTime = 0;
                animating = false;
            }
        }
        else
        {
            transform.position = boardPosition;
        }
    }

    private void SetColor()
    {
        
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
