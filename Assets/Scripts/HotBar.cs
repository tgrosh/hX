using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HotBar : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Depot.OnDepotStarted += Depot_OnDepotStarted;
        Ship.OnShipStarted += Ship_OnShipStarted;
        Ship.OnBoostersChanged += Ship_OnBoostersChanged;
        Ship.OnBlastersChanged += Ship_OnBlastersChanged;
        Ship.OnTractorBeamsChanged += Ship_OnTractorBeamsChanged;
        Ship.OnShipMoveStart += Ship_OnShipMoveStart;
        Ship.OnShipMoveEnd += Ship_OnShipMoveEnd;
        Ship.OnShipSpawnStart += Ship_OnShipSpawnStart;
        Ship.OnShipSpawnEnd += Ship_OnShipSpawnEnd;
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
    
    void Ship_OnShipSpawnEnd(Ship ship)
    {
        SetInteractable(true);
    }

    void Ship_OnShipSpawnStart(Ship ship)
    {
        SetInteractable(false);
    }

    void Ship_OnShipMoveEnd(Ship ship)
    {
        if (ship.ownerId == Player.localPlayer.netId)
        {
            SetInteractable(true);
        }
    }

    void Ship_OnShipMoveStart(Ship ship)
    {
        if (ship.ownerId == Player.localPlayer.netId)
        {
            SetInteractable(false);
        }
    }

    void Ship_OnTractorBeamsChanged(int count)
    {
        ToggleTractorBeamUpgrade(false);
    }

    void Ship_OnBlastersChanged(int count)
    {
        ToggleBlasterUpgrade(false);
    }

    void Ship_OnBoostersChanged(int count)
    {
        ToggleBoosterUpgrade(false);
    }

    void Ship_OnShipStarted(Ship ship)
    {
        ToggleShip(false);
    }

    void Depot_OnDepotStarted(Depot depot)
    {
        ToggleBuildDepot(false);
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

    public void ToggleBlasterUpgrade(bool isOn)
    {
        GameObject.Find("UpgradeBlasterToggle").GetComponent<Toggle>().isOn = isOn;

        if (Player.localPlayer != null)
        {
            Player.localPlayer.Cmd_SetIsBuyingBlasterUpgrade(isOn);
        }
    }

    public void ToggleTractorBeamUpgrade(bool isOn)
    {
        GameObject.Find("UpgradeTractorBeamToggle").GetComponent<Toggle>().isOn = isOn;

        if (Player.localPlayer != null)
        {
            Player.localPlayer.Cmd_SetIsBuyingTractorBeamUpgrade(isOn);
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

    public void SetInteractable(bool interactable)
    {
        GetComponent<CanvasGroup>().interactable = interactable;
    }
}
