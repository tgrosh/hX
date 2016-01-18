using UnityEngine;
using System.Collections;

public class LocalPlayerInfo {
    public static LocalPlayerInfo singleton;
    public string name;

    public LocalPlayerInfo()
    {
        singleton = this;
    }
}
