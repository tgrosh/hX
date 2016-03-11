using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Starport : Station {
    public delegate void StarportStarted(Starport starport);
    public static event StarportStarted OnStarportStarted;

    // Use this for initialization
    new void Start()
    {
        base.Start();

        if (isServer)
        {
            EventLog.singleton.AddEvent(String.Format("Player {0} created a new Starport", EventLog.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
        }

        if (OnStarportStarted != null)
        {
            OnStarportStarted(this);
        }
    }

    [Server]
    public void ToggleArea(bool show)
    {
        if (show)
        {
            foreach (GameCell cell in nearbyCells)
            {
                if (cell.state == GameCellState.Empty && !cell.hasShip)
                {
                    cell.SetCell(owner, GameCellState.ShipBuildArea);
                }
            }
        }
        else
        {
            foreach (GameCell cell in nearbyCells)
            {
                if (cell.state == GameCellState.ShipBuildArea)
                {
                    cell.SetCell(null, GameCellState.Empty);
                }
            }
        }
    }
}
