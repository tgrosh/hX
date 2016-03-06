using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class EncounterManager : NetworkBehaviour {
    public List<Encounter> potentialEncounters = new List<Encounter>();
    public List<int> randomEncounterFactors;
    protected List<Encounter> possibles = new List<Encounter>();
    public int currentEncounterIndex;
    [Range(0,1)]
    public float shipMoveEncounterChance;
    NetworkInstanceId ownerShip;
    
	// Use this for initialization
	void Start () {
        if (isServer)
        {
            Ship.OnShipMoveEnd += Ship_OnShipMoveEnd;

            if (randomEncounterFactors.Count > 0)
            {
                for (int x = 0; x < potentialEncounters.Count; x++)
                {
                    if (randomEncounterFactors.Count >= x + 1)
                    {
                        for (int y = 0; y < randomEncounterFactors[x]; y++)
                        {
                            possibles.Add(potentialEncounters[x]);
                        }
                    }
                }
            }
            else
            {
                foreach (Encounter encounter in potentialEncounters)
                {
                    possibles.Add(encounter);
                }
            }
        }
	}

    [Server]
    void Ship_OnShipMoveEnd(Ship ship)
    {
        if (Random.value <= shipMoveEncounterChance)
        {
            ship.owner.GetComponent<Player>().Rpc_ShowEncounter(ship.netId, Random.Range(0, possibles.Count));
        }
    }
    
    [Client]
    public void ShowEncounter(NetworkInstanceId ownerShip, int currentEncounterIndex)
    {
        this.currentEncounterIndex = currentEncounterIndex;
        this.ownerShip = ownerShip;
        GetComponent<Animator>().SetBool("IsOpen", true);
        GameManager.singleton.ResetCamera();
        UIManager.singleton.hotbar.SetInteractable(false);
        GameManager.singleton.gameBoardLocked = true;
    }

    [Client]
    public void EndEncounter()
    {
        possibles[currentEncounterIndex].EndEncounter();
        GetComponent<Animator>().SetBool("IsOpen", false);
        UIManager.singleton.hotbar.SetInteractable(true);
        GameManager.singleton.gameBoardLocked = false;
    }

    [Client]
    public void DisplayInitialEncounterStage()
    {
        if (GetComponent<Animator>().GetBool("IsOpen")) {
            possibles[currentEncounterIndex].StartEncounter(ClientScene.FindLocalObject(ownerShip).GetComponent<Ship>());
        }
    }
        
    public Encounter CurrentEncounter
    {
        get
        {
            return possibles[currentEncounterIndex];
        }
    }
}
