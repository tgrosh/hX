using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Purchase
{
    public List<PurchaseCost> cost;

    public Purchase(List<PurchaseCost> cost)
    {
        this.cost = cost;
    }
}
