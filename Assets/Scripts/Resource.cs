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
    private bool isColorSet;
    private float rotateSpeedMin = 5f;
    private float rotateSpeedMax = 15f;
    private float rotateSpeed;
    private Transform sphere;

	// Use this for initialization
	void Start () {
        rotateSpeed = Random.Range(rotateSpeedMin, rotateSpeedMax);
        sphere = transform.FindChild("Sphere");
	}
	
	// Update is called once per frame
	void Update () {
        sphere.Rotate(Vector3.forward * Time.deltaTime * rotateSpeed);

        if (!isColorSet)
        {
            SetColor(Resource.GetColor(type));
            isColorSet = true;
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
        if (type == ResourceType.Trillium) return Color.blue;
        if (type == ResourceType.Hydrazine) return Color.green;
        if (type == ResourceType.Workers) return Color.magenta;
        if (type == ResourceType.Supplies) return Color.yellow;

        return Color.black;
    }
}
