using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Assets.Scripts;

public class Resource : NetworkBehaviour {
    [SyncVar]
    public ResourceType type;

    private Color color;
    private bool isColorSet;
    private float rotateSpeedMin = 5f;
    private float rotateSpeedMax = 15f;
    private float rotateSpeed;

	// Use this for initialization
	void Start () {
        rotateSpeed = Random.Range(rotateSpeedMin, rotateSpeedMax);
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.forward * Time.deltaTime * rotateSpeed);

        if (!isColorSet)
        {
            SetColor(Resource.GetColor(type));
            isColorSet = true;
        }
	}

    [Client]
    private void SetColor(Color color)
    {
        //set the sphere color (emission)
        transform.FindChild("Sphere").GetComponent<Renderer>().material.SetColor("_EmissionColor", color); 
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
