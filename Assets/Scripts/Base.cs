using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Assets.Scripts;
using System.Collections.Generic;

public class Base : NetworkBehaviour {
    [SyncVar]
    public Color color;
    public CargoHold cargoHold = new CargoHold(200);
    [SyncVar]
    public NetworkInstanceId owner;
    public delegate void ResourceAdded(Base playerBase, ResourceType resource);
    public event ResourceAdded OnResourceAdded;
    public delegate void ResourceDumped(Base playerBase, ResourceType resource);
    public event ResourceDumped OnResourceDumped;
    public Transform camTarget;

    private bool isColorSet;
    
	// Use this for initialization
	void Start () {
        cargoHold.OnResourceAdded += cargoHold_OnResourceAdded;
        cargoHold.OnResourceDumped += cargoHold_OnResourceDumped;

        camTarget = transform.FindChild("CameraTarget");

        if (isServer)
        {
            NetworkServer.FindLocalObject(owner).GetComponent<Player>().playerBase = this.netId;

            //seed with initial values, enough to buy one ship
            cargoHold.Add(ResourceType.Corium, 2);
            cargoHold.Add(ResourceType.Workers, 1);
        }
	}
    
    void cargoHold_OnResourceDumped(ResourceType resource)
    {
        //Debug.Log("Base (" + this.netId + ") dumped resource (" + resource + ")");
        if (OnResourceDumped != null)
        {
            OnResourceDumped(this, resource);
        } 
        if (isServer)
        {
            NetworkServer.FindLocalObject(owner).GetComponent<Player>().Rpc_DumpResource(resource);
        }
    }

    void cargoHold_OnResourceAdded(ResourceType resource)
    {
        //Debug.Log("Base (" + this.netId + ") added resource (" + resource + ")");
        if (OnResourceAdded != null)
        {
            OnResourceAdded(this, resource);            
        }
        if (isServer)
        {
            NetworkServer.FindLocalObject(owner).GetComponent<Player>().Rpc_AddResource(resource);
        }
    }
	
	// Update is called once per frame
    void Update () {
        if (!isColorSet)
        {
            SetColor(this.color);
            isColorSet = true;
        }
	}

    [Client]
    private void SetColor(Color color)
    {
        transform.FindChild("Group01").FindChild("GeoSphere01").GetComponent<Renderer>().material.color = new Color(color.r,color.g,color.b,transform.FindChild("Group01").FindChild("GeoSphere01").GetComponent<Renderer>().material.color.a);
    }
}
