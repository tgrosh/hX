using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HotBar : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        foreach (Toggle t in GetComponentsInChildren<Toggle>())
        {
            if (t.isOn)
            {
                t.image.color = Color.cyan;
            }
            else
            {
                t.image.color = t.colors.normalColor;
            }
        }
	}

    public void ToggleShip(bool isOn)
    {
        GameObject.Find("ShipToggle").GetComponent<Toggle>().isOn = isOn;

        if (Player.localPlayer != null)
        {
            Player.localPlayer.Cmd_SetIsBuyingShip(isOn);
        }
        if (isOn)
        {
            GameManager.singleton.cam.SetTarget(ClientScene.FindLocalObject(Player.localPlayer.playerBase).GetComponent<Base>().camTarget);
            GameManager.singleton.cam.GetComponent<CameraWatcher>().OnCameraReachedDestination += UIManager_OnCameraReachedDestination;
        }
        else
        {
            if (GameManager.singleton != null)
            {
                GameManager.singleton.ResetCamera();
            }
        }
    }

    public void ToggleBoosterUpgrade(bool isOn)
    {
        GameObject.Find("UpgradeBoosterToggle").GetComponent<Toggle>().isOn = isOn;

        if (Player.localPlayer != null)
        {
            Player.localPlayer.Cmd_SetIsBuyingBoosterUpgrade(isOn);
        }
    }

    public void EndTurn()
    {
        Player.localPlayer.EndTurn();
    }

    public void ToggleBuildDepot(bool isOn)
    {
        GameObject.Find("DepotToggle").GetComponent<Toggle>().isOn = isOn;

        if (Player.localPlayer != null)
        {
            Player.localPlayer.Cmd_SetIsBuyingDepot(isOn);
        }
    }

    public void Toggle(bool show)
    {
        GetComponent<Animator>().SetBool("IsOpen", show);
    }

    void UIManager_OnCameraReachedDestination()
    {
        GameManager.singleton.cam.GetComponent<CameraWatcher>().OnCameraReachedDestination -= UIManager_OnCameraReachedDestination;
    }
}
