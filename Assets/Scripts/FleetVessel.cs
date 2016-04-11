using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using UnityStandardAssets.Cameras;

public class FleetVessel : Ship {
    public float collectionRange;
    public float buildRange;
    public float cargoDropRange;
    [HideInInspector]
    public List<Resource> nearbyResources = new List<Resource>();
    [HideInInspector]
    public List<Depot> nearbyDepots = new List<Depot>();
    [HideInInspector]
    public CargoHold cargoHold = new CargoHold();

    private int blasterCount = 0;
    private int tractorBeamCount = 0;
    
    public delegate void BlastersChanged(int count);
    public static event BlastersChanged OnBlastersChanged;
    public delegate void TractorBeamsChanged(int count);
    public static event TractorBeamsChanged OnTractorBeamsChanged;
        
	// Use this for initialization
    new void Start()
    {
        base.Start();

        GameManager.OnTurnStart += GameManager_OnTurnStart;        
        if (isServer) {
            cargoHold.OnResourceAdded += cargoHold_OnResourceAdded;
            cargoHold.OnResourceDumped += cargoHold_OnResourceDumped;
            EventLog.singleton.AddEvent(String.Format("Player {0} created a new Trade Ship", EventLog.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
        }
	}
            
    [Server]
    void GameManager_OnTurnStart()
    {
        if (GameManager.singleton.activePlayer == owner)
        {
            TransferResources();
            CollectAvailableResources();            
        }
    }
        	
	// Update is called once per frame
	new void Update () {
        base.Update();
	}
    
    [Server]
    private void CollectAvailableResources()
    {
        if (IsDisabled) return;

        foreach (Resource resource in nearbyResources)
        {
            float distance = Vector3.Distance(resource.transform.position, transform.position);
            if (distance <= collectionRange)
            {
                if (!cargoHold.IsFull)
                {
                    ResourceType collectedResource = resource.type;
                    int collectedCount = resource.Collect(cargoHold.AvailableCapacity) + tractorBeamCount;

                    if (collectedCount > 0)
                    {
                        EventLog.singleton.AddEvent(String.Format("Player {0}'s Trade Ship collected " + collectedCount + " {1}",
                            EventLog.singleton.CreateColoredText(owner.seat.ToString(), owner.color),
                            EventLog.singleton.CreateColoredText(resource.type.ToString(), Resource.GetColor(resource.type))));
                        GameObject item = (GameObject)Instantiate(resource.resourceItemPrefab, resource.sphere.transform.position, resource.sphere.transform.rotation);
                        item.GetComponent<ResourceItem>().FlyTo(netId);
                        NetworkServer.Spawn(item);
                    } 

                    cargoHold.Add(collectedResource, collectedCount);
                }
            }
        }
    }

    [Server]
    private void TransferResources()
    {
        if (cargoHold.GetCargo().Count <= 0) return;

        if (nearbyBase != null)
        {
            float distance = Vector3.Distance(nearbyBase.transform.position, transform.position);
            if (distance <= cargoDropRange)
            {
                cargoHold.Transfer(nearbyBase.cargoHold);
                EventLog.singleton.AddEvent(String.Format("Player {0}'s Trade Ship has delivered resources to their Base", EventLog.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
            }
        }

        foreach (Depot depot in nearbyDepots)
        {
            float distance = Vector3.Distance(depot.transform.position, transform.position);
            if (distance <= cargoDropRange)
            {
                cargoHold.Transfer(depot.cargoHold);
                EventLog.singleton.AddEvent(String.Format("Player {0}'s Trade Ship has delivered resources to their Depot", EventLog.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
            }
        }
    }
    
    [Server]
    private void cargoHold_OnResourceAdded(ResourceType resource)
    {
 	    if (isServer)
        {
            Rpc_AddResource(resource);
        }
    }

    [Server]
    void cargoHold_OnResourceDumped(ResourceType resource)
    {
        if (isServer)
        {
            Rpc_DumpResource(resource);
        }
    }

    [Client]
    protected override void SetColor(Color color)
    {
        foreach (Renderer rend in transform.FindChild("SSO-2").FindChild("SSO-2").gameObject.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in rend.materials)
            {
                if (mat.name.Contains("Default"))
                {
                    mat.color = color + (Color.white * .5f);
                }
            }
        }
    }
    
    public int Blasters
    {
        get { return blasterCount; }
        set
        {
            blasterCount = value;
            Rpc_BlasterChange(blasterCount);
        }
    }

    public int TractorBeams
    {
        get { return tractorBeamCount; }
        set
        {
            tractorBeamCount = value;
            Rpc_TractorBeamChange(tractorBeamCount);
        }
    }
    
    [ClientRpc]
    void Rpc_BlasterChange(int count)
    {
        if (OnBlastersChanged != null)
        {
            OnBlastersChanged(count);
        }
    }

    [ClientRpc]
    void Rpc_TractorBeamChange(int count)
    {
        if (OnTractorBeamsChanged != null)
        {
            OnTractorBeamsChanged(count);
        }
    }

    [ClientRpc]
    void Rpc_AddResource(ResourceType resource)
    {
        if (!isServer)
        {
            cargoHold.Add(resource, 1);
        }
    }

    [ClientRpc]
    void Rpc_DumpResource(ResourceType resource)
    {
        if (!isServer)
        {
            cargoHold.Dump(resource, 1);
        }
    }
    
    new void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent && other.transform.parent.GetComponent<Resource>() && !nearbyResources.Contains(other.transform.parent.GetComponent<Resource>()))
        {
            nearbyResources.Add(other.transform.parent.GetComponent<Resource>());
        }
        if (other.GetComponent<Depot>() && !nearbyDepots.Contains(other.GetComponent<Depot>()))
        {
            nearbyDepots.Add(other.GetComponent<Depot>());
        }

        base.OnTriggerEnter(other);
    }

    new void OnTriggerExit(Collider other)
    {
        if (other.transform.parent && nearbyResources.Contains(other.transform.parent.GetComponent<Resource>()))
        {
            nearbyResources.Remove(other.transform.parent.GetComponent<Resource>());
        }

        base.OnTriggerExit(other);
    }
}
