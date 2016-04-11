using UnityEngine;
using System.Collections;

public class ColonyShip : Ship {
    public GameObject actor;

	// Use this for initialization
    new void Start()
    {
        base.Start();
	}
	
	// Update is called once per frame
    new void Update()
    {
        base.Update();
	}

    protected override void SetColor(Color color)
    {
        actor.GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", color + (Color.white * .75f));
    }
}
