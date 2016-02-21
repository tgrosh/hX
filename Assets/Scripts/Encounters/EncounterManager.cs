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
            StartRandomEncounter(ship.netId);
        }
    }

    [Server]
    public void StartRandomEncounter(NetworkInstanceId ownerShip)
    {
        this.ownerShip = ownerShip;
        currentEncounterIndex = Random.Range(0, encounters.Count - 1);
        NetworkServer.FindLocalObject(ownerShip).GetComponent<Ship>().owner.GetComponent<Player>().Rpc_ShowEncounter(this.ownerShip, currentEncounterIndex);
    }

    [Client]
    public void ShowEncounter(NetworkInstanceId ownerShip, int currentEncounterIndex)
    {
        this.currentEncounterIndex = currentEncounterIndex;
        this.ownerShip = ownerShip;
        GetComponent<Animator>().SetBool("IsOpen", true);
    }

    [Client]
    public void EndEncounter()
    {
        encounters[currentEncounterIndex].EndEncounter();
        GetComponent<Animator>().SetBool("IsOpen", false);
    }

    [Client]
    public void DisplayInitialEncounterStage()
    {
        if (GetComponent<Animator>().GetBool("IsOpen")) {
            encounters[currentEncounterIndex].StartEncounter(ClientScene.FindLocalObject(ownerShip).GetComponent<Ship>());
        }
    }
}
