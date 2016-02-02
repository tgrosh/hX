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
    private Color color;
    private bool isColorSet;
    private float rotateSpeedMin = 5f;
    private float rotateSpeedMax = 15f;
    private float rotateSpeed;
    private Transform sphere;
    //private Transform halo;

	// Use this for initialization
	void Start () {
        rotateSpeed = Random.Range(rotateSpeedMin, rotateSpeedMax);
        sphere = transform.FindChild("Sphere");
        //halo = transform.FindChild("Halo");
	}
	
	// Update is called once per frame
	void Update () {
        sphere.Rotate(Vector3.forward * Time.deltaTime * rotateSpeed);

        if (!isColorSet)
        {
            SetColor(Resource.GetColor(type));
            //if (!halo.GetComponent<ParticleSystem>().isPlaying)
            //{
            //    halo.GetComponent<ParticleSystem>().Play();
            //}
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
        //set the sphere color (emission)
        sphere.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
        //halo.GetComponent<ParticleSystem>().startColor = color;
    }

    [Client]
    public static Color GetColor(ResourceType type)
    {
        //move this to the resource late
        if (type == ResourceType.Blue) return Color.blue;
        if (type == ResourceType.Green) return Color.green;
        if (type == ResourceType.Purple) return Color.magenta;
        if (type == ResourceType.Red) return Color.red;
        if (type == ResourceType.Yellow) return Color.yellow;

        return Color.black;
    }
}
