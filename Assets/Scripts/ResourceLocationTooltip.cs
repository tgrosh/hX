using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResourceLocationTooltip : MonoBehaviour {
    public string title;
    [TextArea(3,10)]
    public string description;

    private GameObject tooltip;
    private float hoverTime;
    private float tooltipDelay = 1f;
    private bool pendingTooltip;

	// Use this for initialization
	void Start () {
        tooltip = GameObject.Find("ResourceLocationTooltip");        
	}

    void Update()
    {
        if (pendingTooltip)
        {
            hoverTime += Time.deltaTime;

            if (hoverTime >= tooltipDelay)
            {
                pendingTooltip = false;
                hoverTime = 0f;
                tooltip.transform.FindChild("Title").GetComponent<Text>().text = title;
                tooltip.transform.FindChild("Description").GetComponent<Text>().text = description;
                ShowTooltip();
            }
        }
    }
    
    void OnMouseEnter()
    {
        pendingTooltip = true;
    }

    void OnMouseExit()
    {
        hoverTime = 0f;
        pendingTooltip = false;
        tooltip.transform.position = new Vector2(transform.position.x, -1050);
    }

    void ShowTooltip()
    {        
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);

        tooltip.SetActive(true);
        tooltip.GetComponentInChildren<RectTransform>().anchoredPosition = new Vector2(screenPoint.x, screenPoint.y + 20);
        tooltip.GetComponentInChildren<RectTransform>().anchorMin = screenPoint;
        tooltip.GetComponentInChildren<RectTransform>().anchorMax = screenPoint;
        
    }
}
