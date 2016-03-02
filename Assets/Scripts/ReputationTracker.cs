using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReputationTracker : MonoBehaviour {
    public Sprite redDotSprite;
    public Sprite greenDotSprite;
    public Sprite normalDotSprite;

    private RectTransform repDial;
    private Image topDot;
    private Image bottomDot;
    private Text repLabelTop;
    private Text repLabelBottom;

    private float maxRotation = 56;
    private float maxReputation = 20;
    private Quaternion dialTargetRotation;
    private float rotationSpeed = 3f;
    private bool isOpen;
    private bool alreadyOpened;
    
	// Use this for initialization
	void Start () {
        repDial = GameObject.Find("RepDial").GetComponent<RectTransform>();
        topDot = GameObject.Find("RepDotTop").GetComponent<Image>();
        bottomDot = GameObject.Find("RepDotBottom").GetComponent<Image>();
        repLabelTop = GameObject.Find("RepLabelTop").GetComponent<Text>();
        repLabelBottom = GameObject.Find("RepLabelBottom").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (isOpen || !alreadyOpened && Player.localPlayer != null && Player.localPlayer.reputation != 0)
        {
            GetComponent<Animator>().SetBool("IsOpen", true);
            alreadyOpened = true;
        }

	    dialTargetRotation = Quaternion.Euler(0, 0, (Player.localPlayer.reputation / maxReputation) * maxRotation * -1);

        repDial.rotation = Quaternion.Slerp(repDial.rotation, dialTargetRotation, Time.deltaTime * rotationSpeed);

        if (Mathf.Abs(repDial.rotation.eulerAngles.z - dialTargetRotation.eulerAngles.z) < 1)
        {
            repDial.rotation = dialTargetRotation;
        }

        if (Mathf.Abs(repDial.rotation.eulerAngles.z - maxRotation) < .01f)
        {
            //point at bottom
            bottomDot.overrideSprite = redDotSprite;
        }
        else if (Mathf.Abs(repDial.rotation.eulerAngles.z - (360 - maxRotation)) < .01f)
        {
            //point at top
            topDot.overrideSprite = greenDotSprite;
        }
        else
        {
            topDot.overrideSprite = normalDotSprite;
            bottomDot.overrideSprite = normalDotSprite;
        }

        if (Player.localPlayer.reputation < 0)
        {
            repLabelTop.text = Player.localPlayer.reputation.ToString();
            repLabelTop.gameObject.SetActive(true);
            repLabelBottom.gameObject.SetActive(false);
        }
        else if (Player.localPlayer.reputation > 0)
        {
            repLabelBottom.text = Player.localPlayer.reputation.ToString();
            repLabelTop.gameObject.SetActive(false);
            repLabelBottom.gameObject.SetActive(true);
        }
        else
        {
            repLabelTop.gameObject.SetActive(false);
            repLabelBottom.gameObject.SetActive(false);
        }
	}

    public void Show()
    {
        isOpen = true;
    }
}
