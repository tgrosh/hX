using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts;
using UnityEngine.Networking;

public class MenuManager : MonoBehaviour
{
    public static MenuManager singleton;
    public GameObject GameSetupPanel;
    public GameRule ruleEmpty;
    public GameRule ruleOwnArea;
    public GameRule ruleOwnCore;
    public GameRule ruleEnemyArea;
    public GameRule ruleEnemyCore;
    public GameObject ShipTooltip;

    private GameObject hotbar;
    private GameObject FullEventLog;
    private GameObject MostRecentEventLog;
    private bool isFullLogOpen;

    void Start()
    {
        singleton = this;
        hotbar = GameObject.Find("Hotbar");
        FullEventLog = GameObject.Find("FullEventLog");
        MostRecentEventLog = GameObject.Find("MostRecentEventBackground");
    }

    void OnStartClient()
    {
        ShipTooltip.SetActive(false);
    }

    void Update()
    {        
        if (hotbar != null)
        {
            foreach (Toggle t in hotbar.GetComponentsInChildren<Toggle>())
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
    }

    public void CreateMultiplayerGame()
    {
        NetManager.singleton.StartHost();

        SetPlayerName();

        if (GameRuleManager.singleton == null)
        {
            new GameRuleManager();
        }
        GameRuleManager.singleton.ruleEmpty = ruleEmpty;
        GameRuleManager.singleton.ruleOwnArea = ruleOwnArea;
        GameRuleManager.singleton.ruleOwnCore = ruleOwnCore;
        GameRuleManager.singleton.ruleEnemyArea = ruleEnemyArea;
        GameRuleManager.singleton.ruleEnemyCore = ruleEnemyCore;
    }

    public void JoinMultiplayerGame(Text hostName)
    {
        NetManager.singleton.networkAddress = hostName.text;
        NetManager.singleton.StartClient();
        SetPlayerName();
    }

    public void DisconnectMultiplayer()
    {
        NetManager.singleton.StopHost();
        NetManager.singleton.StopClient();
    }

    private static void SetPlayerName()
    {
        string playerName = GameObject.Find("PlayerName").GetComponent<InputField>().text;
        if (LocalPlayerInfo.singleton == null)
        {
            new LocalPlayerInfo(); //initialize the singleton
        }
        LocalPlayerInfo.singleton.name = playerName;
    }

    public void ShowGameSetup(bool show)
    {
        GameSetupPanel.SetActive(show);
    }

    public void ToggleShip(bool isOn)
    {
        if (hotbar != null)
        {
            hotbar.transform.FindChild("ShipToggle").GetComponent<Toggle>().isOn = isOn;
        }
        if (Player.localPlayer != null)
        {
            Player.localPlayer.Cmd_SetIsBuyingShip(isOn);
        }        
        if (isOn)
        {
            GameManager.singleton.cam.SetTarget(ClientScene.FindLocalObject(Player.localPlayer.playerBase).GetComponent<Base>().camTarget);
            GameManager.singleton.cam.GetComponent<CameraWatcher>().OnCameraReachedDestination += MenuManager_OnCameraReachedDestination;
        }
        else
        {
            if (GameManager.singleton != null)
            {
                GameManager.singleton.ResetCamera();
            }            
        }
    }

    //used during radial menu
    public void ShipButtonClick()
    {
        if (Player.localPlayer != null)
        {
            Player.localPlayer.Cmd_SetIsBuyingShip(true);
        }
        Player.localPlayer.SelectCell(GameManager.singleton.selectedCell.netId);
    }

    void MenuManager_OnCameraReachedDestination()
    {
        GameManager.singleton.cam.GetComponent<CameraWatcher>().OnCameraReachedDestination -= MenuManager_OnCameraReachedDestination;
    }

    public void ToggleEventLog()
    {
        if (!isFullLogOpen)
        {
            FullEventLog.GetComponent<RectTransform>().anchoredPosition = new Vector2(24, 202);
            MostRecentEventLog.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            isFullLogOpen = true;
        }
        else
        {
            FullEventLog.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            MostRecentEventLog.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 32);
            isFullLogOpen = false;
        }
    }

    public void EndTurn()
    {
        Player.localPlayer.EndTurn();
    }

    public void ToggleBuildDepot(bool isOn)
    {
        if (hotbar != null)
        {
            hotbar.transform.FindChild("DepotToggle").GetComponent<Toggle>().isOn = isOn;
        }
        if (Player.localPlayer != null)
        {
            Player.localPlayer.Cmd_SetIsBuyingDepot(isOn);
        } 
        //if (isOn)
        //{
        //    GameManager.singleton.cam.SetTarget(ClientScene.FindLocalObject(Player.localPlayer.playerBase).GetComponent<Base>().camTarget);
        //    GameManager.singleton.cam.GetComponent<CameraWatcher>().OnCameraReachedDestination += MenuManager_OnCameraReachedDestination;
        //}
        //else
        //{
        //    if (GameManager.singleton != null)
        //    {
        //        GameManager.singleton.ResetCamera();
        //    }
        //}
    }

    public void ShowResourceTracker()
    {
        GameObject.Find("ResourceTracker").GetComponent<Animator>().SetBool("IsOpen", true);
    }

    public void ShowYourTurn()
    {
        GameObject.Find("YourTurn").GetComponent<Animator>().SetTrigger("Start");
    }

    public void ToggleRadialMenu(Vector3 point)
    {
        GameObject menu = GameObject.Find("RadialMenu");

        if (menu.transform.position.z != -1)
        {
            menu.transform.position = new Vector3(point.x, point.y, -1);
        }
        else
        {
            menu.transform.position = new Vector3(transform.position.x, transform.position.y, -1000);
        }   
    }

    public void ToggleHotbar(bool show)
    {
        GameObject.Find("Hotbar").GetComponent<Animator>().SetBool("IsOpen", show);
    }

    public void ShowShipTooltip(Ship ship)
    {
        if (!ShipTooltip.activeInHierarchy) {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(ship.transform.position);

            ShipTooltip.transform.FindChild("ResourceTooltipCorium").transform.FindChild("Count").GetComponent<Text>().text = (ship.cargoHold.GetCargo(ResourceType.Corium).Count).ToString();
            ShipTooltip.transform.FindChild("ResourceTooltipHydrazine").transform.FindChild("Count").GetComponent<Text>().text = (ship.cargoHold.GetCargo(ResourceType.Hydrazine).Count).ToString();
            ShipTooltip.transform.FindChild("ResourceTooltipWorkers").transform.FindChild("Count").GetComponent<Text>().text = (ship.cargoHold.GetCargo(ResourceType.Workers).Count).ToString();
            ShipTooltip.transform.FindChild("ResourceTooltipSupplies").transform.FindChild("Count").GetComponent<Text>().text = (ship.cargoHold.GetCargo(ResourceType.Supplies).Count).ToString();

            ShipTooltip.SetActive(true);
            ShipTooltip.GetComponentInChildren<RectTransform>().anchoredPosition = new Vector2(screenPoint.x, screenPoint.y + 35);
            ShipTooltip.GetComponentInChildren<RectTransform>().anchorMin = screenPoint;
            ShipTooltip.GetComponentInChildren<RectTransform>().anchorMax = screenPoint;
        }
    }

    public void HideShipTooltip()
    {
        ShipTooltip.SetActive(false);
    }
}
