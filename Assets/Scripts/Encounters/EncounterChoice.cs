using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterChoice : MonoBehaviour {

    public List<EncounterStage> potentialStages;
    public List<int> randomStageFactors;
    public bool endsEncounter;
    public ResourceType value;
    public delegate void OnEncounterEnd();
    protected EncounterManager mgr;
    protected List<EncounterStage> possibles = new List<EncounterStage>();

    void Start()
    {
        for (int x = 0; x < potentialStages.Count; x++)
        {
            for (int y = 0; y < randomStageFactors[x]; y++)
            {
                possibles.Add(potentialStages[x]);
            }
        }
    }

    void OnEnable()
    {
        mgr = GameObject.Find("EncounterPanel").GetComponent<EncounterManager>();
    }
    
    private EncounterStage NextRandomStage(List<EncounterStage> possibles)
    {
        return possibles[Random.Range(0, possibles.Count - 1)];
    }

    public EncounterStage NextApplicableRandomStage(Encounter encounter, Ship ship)
    {
        return NextRandomStage(possibles.FindAll((EncounterStage stage) => {
            return stage.MeetsRequirements(encounter, ship); 
        }));
    }
    
    public virtual void Select()
    {        
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
