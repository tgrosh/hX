using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PurchaseCost
{
    public ResourceType resource;
    public int quantity;

    public PurchaseCost(ResourceType resource, int quantity)
    {
        this.resource = resource;
        this.quantity = quantity;
    }
}
