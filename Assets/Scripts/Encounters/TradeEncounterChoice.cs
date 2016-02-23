using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class TradeEncounterChoice : EncounterChoice {
    public int playerTradeQuanitity;
    public int cpuTradeQuanitity;

    void OnEnable()
    {
        mgr = GameObject.Find("EncounterPanel").GetComponent<EncounterManager>();

        GetComponent<Button>().interactable = mgr.CurrentEncounter.playerShip.cargoHold.GetCargo(value).Count >= playerTradeQuanitity;
    }

    public override void Select()
    {
        base.Select();

        if (endsEncounter) { 
            if (mgr.CurrentEncounter.values.Count == 2)
            {
                //affect trade
                mgr.CurrentEncounter.playerShip.cargoHold.Dump((ResourceType)mgr.CurrentEncounter.values[0], playerTradeQuanitity);
                mgr.CurrentEncounter.playerShip.cargoHold.Add((ResourceType)mgr.CurrentEncounter.values[1], cpuTradeQuanitity);
            }
        }
    }
}
