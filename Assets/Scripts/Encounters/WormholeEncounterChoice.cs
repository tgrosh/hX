using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;

public class WormholeEncounterChoice : EncounterChoice {
    public Wormhole prefabWormhole;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void Select()
    {
        base.Select();
                
        //pick a random empty gamecell
        List<GameCell> allCells = new List<GameCell>(GameObject.FindObjectsOfType<GameCell>()).FindAll((GameCell cell) =>
        {
            return cell.state == GameCellState.Empty &&
                cell.hasShip == false;
        });
        GameCell randomCell = allCells[Random.Range(0, allCells.Count)];
        
        Player.localPlayer.Cmd_ShipWormholeTo(mgr.CurrentEncounter.playerShip.netId, randomCell.netId);        
    }
}
