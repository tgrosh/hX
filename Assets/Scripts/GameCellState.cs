using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum GameCellState
{
    Empty = 0,
    Area = 1,
    Core = 2,
    Base = 3,
    BaseArea = 4,
    MovementArea = 6,
    Resource = 7,
    Depot = 8,
    DepotBuildArea = 9
}
