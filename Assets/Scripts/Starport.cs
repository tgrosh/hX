using UnityEngine;
using System.Collections;
using System;

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
}
