using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Assets.Scripts;
using System.Collections.Generic;
using System;

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
    public DateTime lastResourceTime;

    private bool isColorSet;
    
	// Use this for initialization
	void Start () {
        cargoHold.OnResourceAdded += cargoHold_OnResourceAdded;
        cargoHold.OnResourceDumped += cargoHold_OnResourceDumped;
        lastResourceTime = DateTime.Now;
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
        if (DateTime.Now.AddSeconds(-5) > lastResourceTime)
        {
            GameManager.singleton.AddEvent(String.Format("Player {0} has delivered resources to their base", GameManager.singleton.CreateColoredText(NetworkServer.FindLocalObject(owner).GetComponent<Player>().seat.ToString(), NetworkServer.FindLocalObject(owner).GetComponent<Player>().color)));
            lastResourceTime = DateTime.Now;
        }
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
