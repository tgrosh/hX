using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class WormholeEncounterChoice : EncounterChoice {

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

        //teleport

        //pick a random empty gamecell
        List<GameCell> allCells = new List<GameCell>(GameObject.FindObjectsOfType<GameCell>()).FindAll((GameCell cell) => { 
            return cell.state == GameCellState.Empty &&
                cell.hasShip == false;
        });
        GameCell randomCell = allCells[Random.Range(0, allCells.Count)];

        Player.localPlayer.Cmd_ShipWormholeTo(mgr.CurrentEncounter.playerShip.netId, randomCell.netId);

        //if it is wormhole eligible, instantly move the ship to that location
        //perhaps an animation, down where it is, up where it goes, maybe some particles
        //perhaps this is my opportunity to change the ship entrance to an animation
    }
}
