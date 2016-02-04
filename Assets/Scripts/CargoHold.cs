using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class CargoHold
    {
        public List<ResourceType> cargo;
        private int capacity = 20;

        public delegate void ResourceAdded(ResourceType resource);
        public event ResourceAdded OnResourceAdded;
        public delegate void ResourceDumped(ResourceType resource);
        public event ResourceDumped OnResourceDumped;
        
        public CargoHold()
        {
            cargo = new List<ResourceType>(this.capacity);
        }

        public CargoHold(int capacity)
        {
            this.capacity = capacity;
            cargo = new List<ResourceType>(this.capacity);
        }
        
        public int Add(ResourceType resource, int quantity)
        {
            if (!IsFull)
            {
                for (int x = 0; x < Math.Min(quantity, AvailableCapacity); x++)
                {
                    cargo.Add(resource);
                    if (OnResourceAdded != null)
                    {
                        OnResourceAdded(resource);
                    }                    
                }


                return Math.Min(quantity, AvailableCapacity);
            }

            return 0;
        }

        public int Dump(ResourceType resource, int quantity)
        {
            int removedCount = 0;
            List<ResourceType> temp = new List<ResourceType>();
            
            foreach (ResourceType r in cargo)
            {
                if (r != resource || removedCount >= quantity) //keep ignoring resources, until we have ignored the right amount
                {
                    temp.Add(r);
                }
                else
                {
                    removedCount++;
                    if (OnResourceDumped != null)
                    {
                        OnResourceDumped(r);
                    }
                }
            }

            cargo = temp;

            return removedCount;
        }

        public int Transfer(ResourceType resource, int quantity, CargoHold other)
        {
            int transferred = other.Add(resource, quantity);
            
            Dump(resource, transferred);

            return transferred;
        }

        public void Transfer(CargoHold other)
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                if (type != ResourceType.None)
                {
                    Transfer(type, GetCargo(type).Count, other);
                }
            }
        }

        public int TotalCapacity
        {
            get { return capacity; }
        }

        public int AvailableCapacity
        {
            get { return capacity - cargo.Count; }
        }

        public bool IsFull
        {
            get { return cargo.Count == capacity; }
        }
        
        public List<ResourceType> GetCargo(ResourceType type)
        {
            return cargo.FindAll((ResourceType t) => { return t == type; });
        }

        public List<ResourceType> GetCargo()
        {
            return cargo;
        }
    }
}
