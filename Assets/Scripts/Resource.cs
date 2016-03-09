using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Resource : NetworkBehaviour {
    [SyncVar]
    public ResourceType type;
    public GameObject resourceItemPrefab;

    private float rotateSpeedMin = 5f;
    private float rotateSpeedMax = 15f;
    private float rotateSpeed;
    public Transform sphere;

	// Use this for initialization
	void Start () {
        rotateSpeed = Random.Range(rotateSpeedMin, rotateSpeedMax);
        sphere = transform.FindChild("Sphere");
	}
	
	// Update is called once per frame
	void Update () {
        sphere.Rotate(Vector3.forward * Time.deltaTime * rotateSpeed);
	}

    public int Collect(int requestAmount)
    {
        int collection = Mathf.Min(new int[] { requestAmount, Random.Range(0, 4) });
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
        if (type == ResourceType.Workers) return new Color(.5f, 0, 1);
        if (type == ResourceType.Supplies) return new Color(1, .8f, 0);

        return Color.black;
    }

    public static Color GetColor(ResourceType type, float alpha)
    {
        Color color = GetColor(type);
        color.a = alpha;

        return color;
    }
}
