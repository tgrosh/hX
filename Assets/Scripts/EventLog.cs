using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class EventLog : NetworkBehaviour {

    public static EventLog singleton;
    public GameObject EventLogEntryPrefab;

    [HideInInspector]
    public List<string> events = new List<string>();

	// Use this for initialization
	void Start () {
        singleton = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    [ClientRpc]
    private void Rpc_AddEvent(string text)
    {
        GameObject.Find("MostRecentEvent").GetComponent<Text>().text = text;
        events.Add(text);
        PopulateEventLog(events);
    }

    [Server]
    public void AddEvent(string text)
    {
        Rpc_AddEvent(text);
    }

    [Client]
    private void PopulateEventLog(List<string> events)
    {
        GameObject eventLogContent = GameObject.Find("EventLogContent");
        GameObject fullEventLog = GameObject.Find("FullEventLog");
        float newHeight = 14;

        foreach (Transform child in eventLogContent.transform)
        {
            Destroy(child.gameObject);
        }


        //.GetRange(Mathf.Max(events.Count - 41, 0), Mathf.Min(40, events.Count))
        foreach (string s in events)
        {
            GameObject obj = Instantiate(EventLogEntryPrefab);
            obj.GetComponent<Text>().text = s;

            //increase content area to be big enough to support the new item
            eventLogContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 1200);
            obj.transform.SetParent(eventLogContent.transform);

            newHeight += obj.GetComponent<RectTransform>().rect.height + 3;
            newHeight = Mathf.Max(newHeight, 200);
            eventLogContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, newHeight);
        }

        Canvas.ForceUpdateCanvases();
        fullEventLog.GetComponent<ScrollRect>().verticalScrollbar.value = 0f;
        Canvas.ForceUpdateCanvases();
    }
    
    public string CreateColoredText(String text, Color color)
    {
        return "<color=#" + ColorToHex(color) + ">" + text + "</color>";
    }

    public string ColorToHex(Color color)
    {
        return Convert.ToInt32(color.r * 255).ToString("X2") + Convert.ToInt32(color.g * 255).ToString("X2") + Convert.ToInt32(color.b * 255).ToString("X2");
    }
}
