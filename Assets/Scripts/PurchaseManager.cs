using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class PurchaseManager
    {
        public static Purchase Ship = new Purchase(new List<PurchaseCost>() { 
            new PurchaseCost(ResourceType.Trillium, 2), 
            new PurchaseCost(ResourceType.Workers, 1) 
        });

    }
}
