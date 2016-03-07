using UnityEngine;
using System.Collections;

public class EncounterStage : MonoBehaviour {

    public EncounterStageRequirements requirement;
    public EncounterStageComparer comparer;
    

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void StartStage()
    {
        gameObject.SetActive(true);
    }

    public bool MeetsRequirements(Encounter encounter, FleetVessel ship)
    {
        if (requirement == EncounterStageRequirements.None) return true;

        if (requirement == EncounterStageRequirements.Blasters)
        {
            return compareCount(ship.Blasters, encounter.encounterStrength);
        }
        else if (requirement == EncounterStageRequirements.Boosters)
        {
            return compareCount(ship.Boosters, encounter.encounterStrength);
        }

        return false;
    }

    private bool compareCount(int value, int requiredValue)
    {
        if (value < requiredValue)
        {
            return comparer == EncounterStageComparer.LessThan || comparer == EncounterStageComparer.LessThanOrEqual;
        }
        if (value > requiredValue)
        {
            return comparer == EncounterStageComparer.GreaterThan || comparer == EncounterStageComparer.GreaterThanOrEqual;
        }
        if (value == requiredValue)
        {
            return comparer == EncounterStageComparer.GreaterThanOrEqual ||
                comparer == EncounterStageComparer.LessThanOrEqual ||
                comparer == EncounterStageComparer.Equal;
        }

        return false;
    }
}

public enum EncounterStageRequirements
{
    None,
    Boosters,
    Blasters,
    Reputation
}

public enum EncounterStageComparer
{
    GreaterThan,
    LessThan,
    Equal,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Max,
    NotMax,
    BelowZero
}
