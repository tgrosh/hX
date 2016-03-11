using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

public class Depot : Station {
    public delegate void DepotStarted(Depot depot);
    public static event DepotStarted OnDepotStarted;

	// Use this for initialization
    new void Start()
    {
        base.Start();

        if (isServer)
        {
            EventLog.singleton.AddEvent(String.Format("Player {0} created a new Depot", EventLog.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
        }

        if (OnDepotStarted != null)
        {
            OnDepotStarted(this);
        }
    }    
}
