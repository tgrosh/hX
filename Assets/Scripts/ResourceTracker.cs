using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;

public class ResourceTracker : NetworkBehaviour {

    public GameObject prefabResourceCounter;

    private GameObject resourceCountPanel;

	// Use this for initialization
	void Start () {
        
	}

    public override void OnStartClient()
    {
        resourceCountPanel = GameObject.Find("ResourceCountPanel");
        foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
        {
            if (t != ResourceType.None)
            {
                CreateResourceCounter(t);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void Show()
    {
        GetComponent<Animator>().SetBool("IsOpen", true);
    }
    
    [Client]
    private void CreateResourceCounter(ResourceType type)
    {
        GameObject objCounter = Instantiate(prefabResourceCounter);
        objCounter.name = type.ToString();
        objCounter.transform.FindChild("ResourceName").GetComponent<Text>().text = type.ToString();
        objCounter.transform.FindChild("Count").GetComponent<Text>().text = "0";
        objCounter.transform.FindChild("Image").GetComponent<Image>().color = Resource.GetColor(type, .5f);
        objCounter.transform.SetParent(resourceCountPanel.transform);
        objCounter.transform.localScale = Vector3.one;
        objCounter.transform.localPosition = new Vector3(objCounter.transform.localPosition.x, objCounter.transform.localPosition.y, 0);
    }
        
    [Client]
    public void IncrementResource(ResourceType resource)
    {
        if (resource != ResourceType.None)
        {
            int count = Convert.ToInt32(resourceCountPanel.transform.FindChild(resource.ToString()).FindChild("Count").GetComponent<Text>().text);
            resourceCountPanel.transform.FindChild(resource.ToString()).FindChild("Count").GetComponent<Text>().text = (count + 1).ToString();
        }
    }

    [Client]
    public void DecrementResource(ResourceType resource)
    {
        if (resource != ResourceType.None)
        {
            int count = Convert.ToInt32(resourceCountPanel.transform.FindChild(resource.ToString()).FindChild("Count").GetComponent<Text>().text);
            resourceCountPanel.transform.FindChild(resource.ToString()).FindChild("Count").GetComponent<Text>().text = (count - 1).ToString();
        }
    }
}
