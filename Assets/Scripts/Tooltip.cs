using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public string title;
    [TextArea(3,10)]
    public string description;

    private GameObject tooltip;
    private float hoverTime;
    private float tooltipDelay = 1f;
    private bool pendingTooltip;

	// Use this for initialization
	void Start () {
        tooltip = GameObject.Find("HotbarTooltip");        
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
                tooltip.transform.position = new Vector2(transform.position.x, 50);
            }
        }
    }
    
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        pendingTooltip = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        hoverTime = 0f;
        pendingTooltip = false;
        tooltip.transform.position = new Vector2(transform.position.x, -1050);
    }
}
