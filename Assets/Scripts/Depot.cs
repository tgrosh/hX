using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using Assets.Scripts;

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

	// Use this for initialization
    void Start()
    {
        cargoHold.OnResourceAdded += cargoHold_OnResourceAdded;

        origPosition = transform.position;
        depot = transform.FindChild("DepotModel");
        transform.position = transform.position + new Vector3(0, 0, 1);
        animatingEntrance = true;

        MenuManager.singleton.ToggleBuildDepot(false);

        if (isServer)
        {
            GameManager.OnTurnStart += GameManager_OnTurnStart;
            GameManager.singleton.AddEvent(String.Format("Player {0} created a new Depot", GameManager.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
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

            //GameManager.singleton.ResetCamera();
            //if (OnShipSpawnEnd != null)
            //{
            //    OnShipSpawnEnd();
            //}
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
            CollectAvailableResources();
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
    private void CollectAvailableResources()
    {
        foreach (Resource resource in nearbyResources)
        {
            float distance = Vector3.Distance(resource.transform.position, transform.position);
            
            if (!cargoHold.IsFull)
            {
                ResourceType collectedResource = resource.type;
                int collectedCount = resource.Collect(cargoHold.AvailableCapacity);

                if (collectedCount > 0)
                {
                    GameManager.singleton.AddEvent(String.Format("Player {0}'s Depot collected " + collectedCount + " {1}",
                        GameManager.singleton.CreateColoredText(owner.seat.ToString(), owner.color),
                        GameManager.singleton.CreateColoredText(resource.type.ToString(), Resource.GetColor(resource.type))));
                    GameObject item = (GameObject)Instantiate(resource.resourceItemPrefab, resource.sphere.transform.position, resource.sphere.transform.rotation);
                    item.GetComponent<ResourceItem>().FlyTo(netId);
                    NetworkServer.Spawn(item);
                }

                cargoHold.Add(collectedResource, collectedCount);
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
