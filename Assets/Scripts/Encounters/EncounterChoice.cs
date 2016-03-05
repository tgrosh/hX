using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EncounterChoice : MonoBehaviour {

    public List<EncounterStage> potentialStages;
    public List<int> randomStageFactors;
    public bool endsEncounter;
    public delegate void OnEncounterEnd();
    protected EncounterManager mgr;
    protected List<EncounterStage> possibles = new List<EncounterStage>();
    public int reputationValue;
    public bool disablesShip;
    public int resourceMinGrant;
    public int resourceMaxGrant;
    public bool causesPurge;
    public bool causesHalfPurge;

    protected virtual void Start()
    {
        if (randomStageFactors.Count > 0)
        {
            for (int x = 0; x < potentialStages.Count; x++)
            {
                if (randomStageFactors.Count >= x + 1)
                {
                    for (int y = 0; y < randomStageFactors[x]; y++)
                    {
                        possibles.Add(potentialStages[x]);
                    }
                }
            }
        }
        else
        {
            foreach (EncounterStage stage in potentialStages)
            {
                possibles.Add(stage);
            }
        }
    }

    protected virtual void OnEnable()
    {
        mgr = GameObject.Find("EncounterPanel").GetComponent<EncounterManager>();
    }
    
    private EncounterStage NextRandomStage(List<EncounterStage> possibles)
    {
        return possibles[UnityEngine.Random.Range(0, possibles.Count)];
    }

    public EncounterStage NextApplicableRandomStage(Encounter encounter, Ship ship)
    {
        return NextRandomStage(possibles.FindAll((EncounterStage stage) => {
            return stage.MeetsRequirements(encounter, ship); 
        }));
    }
    
    public virtual void Select()
    {        
        //affect player reputation here
        Player.localPlayer.Cmd_UpdateReputation(reputationValue);
        if (disablesShip)
        {
            Player.localPlayer.Cmd_SetShipDisabled(mgr.CurrentEncounter.playerShip.netId, true);
        }

        if (resourceMinGrant > 0 && resourceMaxGrant > 0)
        {
            List<ResourceType> resourcesGranted = new List<ResourceType>();
            for (int x = 0; x < UnityEngine.Random.Range(resourceMinGrant, resourceMaxGrant); x++)
            {
                resourcesGranted.Add((ResourceType)Enum.GetValues(typeof(ResourceType)).GetValue(UnityEngine.Random.Range(1, 4)));
            }
            foreach (ResourceType resource in resourcesGranted)
            {
                mgr.CurrentEncounter.playerShip.cargoHold.Add(resource, 1);
            }
        }


        if (causesPurge)
        {
            mgr.CurrentEncounter.playerShip.cargoHold.Purge();
        }
        else if (causesHalfPurge)
        {
            mgr.CurrentEncounter.playerShip.cargoHold.PurgeHalf();
        }
        

        if (!endsEncounter)
        {
            mgr.CurrentEncounter.SetStage(this);
        }
        else
        {
            mgr.EndEncounter();
        }
    }
}
