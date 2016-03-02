using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Encounter : MonoBehaviour {
    public List<EncounterStage> stages;
    public EncounterStage currentStage;
    public Ship playerShip;
    public int encounterStrengthMin;
    public int encounterStrengthMax;
    [HideInInspector]
    public int encounterStrength;
    public List<ResourceType> tradeResources = new List<ResourceType>();

	// Use this for initialization
	void Start () {
        
	}

    void OnEnable()
    {
        foreach (EncounterStage stage in stages)
        {
            stage.gameObject.SetActive(false);
        }
    }
	
    public void StartEncounter(Ship playerShip)
    {
        tradeResources.Clear();
        this.playerShip = playerShip;
        gameObject.SetActive(true);
        encounterStrength = Random.Range(encounterStrengthMin, encounterStrengthMax);      
        SetStage(stages[0]);
    }

    public void EndEncounter()
    {
        foreach (EncounterStage stage in stages)
        {
            stage.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
        currentStage = null;
    }

    public void SetStage(EncounterStage newStage)
    {
        foreach (EncounterStage stage in stages)
        {
            stage.gameObject.SetActive(false);
        }
        currentStage = newStage;
        currentStage.StartStage();
    }

    public void SetStage(EncounterChoice choice)
    {
        SetStage(choice.NextApplicableRandomStage(this, playerShip));
    }
}
