using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class EncounterManager : NetworkBehaviour {
    public List<Encounter> encounters = new List<Encounter>();
    public int currentEncounterIndex;
    public float shipMoveEncounterChance;
    NetworkInstanceId ownerShip;
    
	// Use this for initialization
	void Start () {
        if (isServer)
        {
            Ship.OnShipMoveEnd += Ship_OnShipMoveEnd;
        }
	}

    [Server]
    void Ship_OnShipMoveEnd(Ship ship)
    {
        if (Random.value <= shipMoveEncounterChance)
        {
            ship.owner.GetComponent<Player>().Rpc_ShowEncounter(ship.netId, Random.Range(0, encounters.Count));
        }
    }
    
    [Client]
    public void ShowEncounter(NetworkInstanceId ownerShip, int currentEncounterIndex)
    {
        this.currentEncounterIndex = currentEncounterIndex;
        this.ownerShip = ownerShip;
        GetComponent<Animator>().SetBool("IsOpen", true);
        GameManager.singleton.ResetCamera();
    }

    [Client]
    public void EndEncounter()
    {
        encounters[currentEncounterIndex].EndEncounter();
        GetComponent<Animator>().SetBool("IsOpen", false);
        UIManager.singleton.hotbar.GetComponent<CanvasGroup>().interactable = true;
    }

    [Client]
    public void DisplayInitialEncounterStage()
    {
        if (GetComponent<Animator>().GetBool("IsOpen")) {
            encounters[currentEncounterIndex].StartEncounter(ClientScene.FindLocalObject(ownerShip).GetComponent<Ship>());
        }
    }
        
    public Encounter CurrentEncounter
    {
        get
        {
            return encounters[currentEncounterIndex];
        }
    }
}
