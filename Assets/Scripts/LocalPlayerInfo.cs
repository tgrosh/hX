using UnityEngine;
using System.Collections;

public class LocalPlayerInfo {
    public static LocalPlayerInfo singleton;

    public LocalPlayerInfo()
    {
        singleton = this;
    }
}
