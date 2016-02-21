using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterChoice : MonoBehaviour {

    public List<EncounterStage> potentialStages;
    public List<int> randomStageFactors;

    List<EncounterStage> possibles = new List<EncounterStage>();

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
}
