using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class TradeEncounterChoice : EncounterChoice {
    public int playerTradeQuanitity;
    public int cpuTradeQuanitity;
    public ResourceType resourceType;
    public bool finalizesTrade;
    public bool requiresResource;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        GetComponent<Button>().interactable = !requiresResource || mgr.CurrentEncounter.playerShip.cargoHold.GetCargo(resourceType).Count >= playerTradeQuanitity;
    }

    public override void Select()
    {
        if (resourceType != ResourceType.None)
        {
            mgr.CurrentEncounter.tradeResources.Add(resourceType);
        }

        base.Select();

        if (finalizesTrade)
        {
            if (mgr.CurrentEncounter.tradeResources.Count == 2)
            {
                //affect trade
                mgr.CurrentEncounter.playerShip.cargoHold.Dump(mgr.CurrentEncounter.tradeResources[0], playerTradeQuanitity);
                mgr.CurrentEncounter.playerShip.cargoHold.Add(mgr.CurrentEncounter.tradeResources[1], cpuTradeQuanitity);
            }
        }
    }
}
