using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Station : NetworkBehaviour {
    [SyncVar]
    public Color color;
    public Player owner;
    public float rotateSpeed;
    public Transform actor;
    private bool isColorSet;
    public List<Resource> nearbyResources = new List<Resource>();
    public CargoHold cargoHold = new CargoHold();

	// Use this for initialization
	protected void Start () {
        cargoHold.OnResourceAdded += cargoHold_OnResourceAdded;
        
        if (isServer)
        {
            GameManager.OnTurnStart += GameManager_OnTurnStart;
        }

        GetComponentInChildren<AnimationHandler>().animator.SetBool("IsActive", true);
	}
	
	// Update is called once per frame
	void Update () {
        actor.Rotate(Vector3.up * Time.deltaTime * rotateSpeed);

        if (!isColorSet)
        {
            SetColor(this.color);
            isColorSet = true;
        }
	}

    private void cargoHold_OnResourceAdded(ResourceType resource)
    {
        if (isServer)
        {
            owner.Rpc_AddResource(resource);
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
        foreach (Renderer rend in actor.GetComponentsInChildren<Renderer>())
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
