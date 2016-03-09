using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

public class Depot : NetworkBehaviour {
    [SyncVar]
    public Color color;
    public Player owner;
    private float rotateSpeed = 5f;
    public Transform depot;
    private bool isColorSet;
    private bool animatingEntrance;
    private float animationCurrentTime;
    private float animationSpeed = 3f;
    private Vector3 origPosition;
    public List<Resource> nearbyResources = new List<Resource>();
    public CargoHold cargoHold = new CargoHold();

    public delegate void DepotStarted(Depot depot);
    public static event DepotStarted OnDepotStarted;

	// Use this for initialization
    void Start()
    {
        cargoHold.OnResourceAdded += cargoHold_OnResourceAdded;

        origPosition = transform.position;
        depot = transform.FindChild("DepotModel");
        transform.position = transform.position + new Vector3(0, 0, 1);
        animatingEntrance = true;

        if (isServer)
        {
            GameManager.OnTurnStart += GameManager_OnTurnStart;
            EventLog.singleton.AddEvent(String.Format("Player {0} created a new Depot", EventLog.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
        }

        if (OnDepotStarted != null)
        {
            OnDepotStarted(this);
        }
    }
    
    private void cargoHold_OnResourceAdded(ResourceType resource)
    {
        if (isServer)
        {
            owner.Rpc_AddResource(resource);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (animatingEntrance && animationCurrentTime / animationSpeed < .95f)
        {
            transform.position = Vector3.Lerp(transform.position, origPosition, animationSpeed * Time.deltaTime);
            animationCurrentTime += Time.deltaTime;
        }
        else if (animatingEntrance)
        {
            transform.localPosition = origPosition;

            animationCurrentTime = 0;
            animatingEntrance = false;
        }

        depot.Rotate(Vector3.up * Time.deltaTime * rotateSpeed);

        if (!isColorSet)
        {
            SetColor(this.color);
            isColorSet = true;
        }
    }

    [Server]
    void GameManager_OnTurnStart()
    {
        if (GameManager.singleton.activePlayer == owner)
        {
            TransferResources();
        }
    }

    [Client]
    private void SetColor(Color color)
    {
        foreach (Renderer rend in transform.FindChild("DepotModel").GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in rend.materials)
            {
                mat.color = color + (Color.white * .5f);
            }
        }
    }
    
    [Server]
    private void TransferResources()
    {
        if (cargoHold.GetCargo().Count > 0)
        {
            cargoHold.Transfer(NetworkServer.FindLocalObject(owner.playerBase).GetComponent<Base>().cargoHold);        
        }        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent && other.transform.parent.GetComponent<Resource>() && !nearbyResources.Contains(other.transform.parent.GetComponent<Resource>()))
        {
            nearbyResources.Add(other.transform.parent.GetComponent<Resource>());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform.parent && nearbyResources.Contains(other.transform.parent.GetComponent<Resource>()))
        {
            nearbyResources.Remove(other.transform.parent.GetComponent<Resource>());
        }
    }
}
