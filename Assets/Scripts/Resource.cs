using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Assets.Scripts;

public class Resource : NetworkBehaviour {
    [SyncVar]
    public ResourceType type;
    public int available { get { return amount; } }

    [SyncVar]
    private int amount = 10; //set later
    private int collectionRate = 3; //collecting per player per round
    private float rotateSpeedMin = 5f;
    private float rotateSpeedMax = 15f;
    private float rotateSpeed;
    private Transform sphere;
    private Transform TrilliumCrystal;
    private Transform FuelTank;
    private Transform Crate;
    private Transform Workers;

	// Use this for initialization
	void Start () {
        rotateSpeed = Random.Range(rotateSpeedMin, rotateSpeedMax);
        sphere = transform.FindChild("Sphere");
        TrilliumCrystal = sphere.FindChild("TrilliumCrystal");
        FuelTank = sphere.FindChild("fueltank");
        Crate = sphere.FindChild("crate");
        Workers = sphere.FindChild("Workers");
	}
	
	// Update is called once per frame
	void Update () {
        sphere.Rotate(Vector3.forward * Time.deltaTime * rotateSpeed);

        if (TrilliumCrystal != null)
        {
            TrilliumCrystal.Rotate(Vector3.one * Time.deltaTime * rotateSpeed);
        }

        if (FuelTank != null)
        {
            FuelTank.Rotate(Vector3.one * Time.deltaTime * rotateSpeed);
        }

        if (Crate != null)
        {
            Crate.Rotate(Vector3.one * Time.deltaTime * rotateSpeed);
        }

        if (Workers != null)
        {
            Workers.Rotate(Vector3.one * Time.deltaTime * rotateSpeed);
        }
	}

    public int Collect(int requestAmount)
    {
        int collection = Mathf.Min(new int[] { requestAmount, collectionRate, amount });
        amount -= collection;
        return collection;
    }

    [Client]
    private void SetColor(Color color)
    {
        sphere.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
    }

    public static Color GetColor(ResourceType type)
    {
        //move this to the resource late
        if (type == ResourceType.Corium) return new Color(.75f, 0, 0);
        if (type == ResourceType.Hydrazine) return new Color(0, .75f, 0);
        if (type == ResourceType.Workers) return new Color(0, .5f, 1);
        if (type == ResourceType.Supplies) return new Color(1, .8f, 0);

        return Color.black;
    }
}
