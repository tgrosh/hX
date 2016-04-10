using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum GameCellState
{
    Empty = 0,
    Base = 3,
    ShipBuildArea = 4,
    MovementArea = 6,
    Resource = 7,
    Depot = 8,
    DepotBuildArea = 9,
    Starport = 10,
    TempusSpace = 11,
    Tempus = 12,
    TempusPlanet = 13,
    NoMovement = 1
}
