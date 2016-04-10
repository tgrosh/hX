﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class PurchaseManager
{
    public static Purchase FleetVessel = new Purchase(new List<PurchaseCost>() { 
        new PurchaseCost(ResourceType.Corium, 2), 
        new PurchaseCost(ResourceType.Workers, 1) 
    });

    public static Purchase ColonyShip = new Purchase(new List<PurchaseCost>() { 
        new PurchaseCost(ResourceType.Supplies, 3), 
        new PurchaseCost(ResourceType.Hydrazine, 2) 
    });

    public static Purchase Depot = new Purchase(new List<PurchaseCost>() { 
        new PurchaseCost(ResourceType.Supplies, 2), 
        new PurchaseCost(ResourceType.Hydrazine, 1) 
    });

    public static Purchase UpgradeBooster = new Purchase(new List<PurchaseCost>() { 
        new PurchaseCost(ResourceType.Corium, 1), 
        new PurchaseCost(ResourceType.Hydrazine, 2) 
    });

    public static Purchase UpgradeBlaster = new Purchase(new List<PurchaseCost>() { 
        new PurchaseCost(ResourceType.Workers, 2), 
        new PurchaseCost(ResourceType.Supplies, 1) 
    });

    public static Purchase UpgradeTractorBeam = new Purchase(new List<PurchaseCost>() { 
        new PurchaseCost(ResourceType.Workers, 1), 
        new PurchaseCost(ResourceType.Hydrazine, 2) 
    });

    public static Purchase Starport = new Purchase(new List<PurchaseCost>() { 
        new PurchaseCost(ResourceType.Corium, 4), 
        new PurchaseCost(ResourceType.Workers, 2), 
        new PurchaseCost(ResourceType.Supplies, 1)  
    });

}
