using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class Purchase
    {
        public List<PurchaseCost> cost;

        public Purchase(List<PurchaseCost> cost)
        {
            this.cost = cost;
        }
    }
}
