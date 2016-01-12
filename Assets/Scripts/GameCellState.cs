using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public enum GameCellState
    {
        PlayerTwoCore = -2,
        PlayerTwoArea = -1,
        Empty = 0,
        PlayerOneArea = 1,
        PlayerOneCore = 2
    }
}
