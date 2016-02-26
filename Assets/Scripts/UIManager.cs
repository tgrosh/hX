using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Reflection;

[assembly: AssemblyVersion("1.0.0.*")]
public class UIManager : NetworkBehaviour
{
    public static UIManager singleton;
    public GameObject GameSetupPanel;
    public GameRule ruleEmpty;
    public GameRule ruleOwnArea;
    public GameRule ruleOwnCore;
    public GameRule ruleEnemyArea;
    public GameRule ruleEnemyCore;
    public GameObject ShipTooltip;

    private GameObject FullEventLog;
    private GameObject MostRecentEventLog;
    private bool isFullLogOpen;
    public HotBar hotbar;
    private string version;

    void Start()
    {
        singleton = this;
        FullEventLog = GameObject.Find("FullEventLog");
        MostRecentEventLog = GameObject.Find("MostRecentEventBackground");

        if (GameObject.Find("VersionNumber") != null)
        {
            GameObject.Find("VersionNumber").GetComponent<Text>().text = "v" + Version;
        }
    }

    public override void OnStartClient()
    {
        if (GameObject.Find("Hotbar") != null)
        {
            hotbar = GameObject.Find("Hotbar").GetComponent<HotBar>();
        }

        ShipTooltip.SetActive(false);

        Depot.OnDepotStarted += Depot_OnDepotStarted;
        Ship.OnShipStarted += Ship_OnShipStarted;
        Ship.OnBoostersChanged += Ship_OnBoostersChanged;
        Ship.OnBlastersChanged += Ship_OnBlastersChanged;
        Ship.OnTractorBeamsChanged += Ship_OnTractorBeamsChanged;
        Ship.OnShipMoveStart += Ship_OnShipMoveStart;
        Ship.OnShipMoveEnd += Ship_OnShipMoveEnd;
    }

    void Ship_OnShipMoveEnd(Ship ship)
    {
        if (ship.ownerId == Player.localPlayer.netId)
        {
            hotbar.GetComponent<CanvasGroup>().interactable = true;
        }
    }

    void Ship_OnShipMoveStart(Ship ship)
    {
        if (ship.ownerId == Player.localPlayer.netId)
        {
           hotbar.GetComponent<CanvasGroup>().interactable = false;
        }
    }

    void Ship_OnTractorBeamsChanged(int count)
    {
        hotbar.ToggleTractorBeamUpgrade(false);
    }

    void Ship_OnBlastersChanged(int count)
    {
        hotbar.ToggleBlasterUpgrade(false);
    }

    void Ship_OnBoostersChanged(int count)
    {
        hotbar.ToggleBoosterUpgrade(false);
    }

    void Ship_OnShipStarted(Ship ship)
    {
        hotbar.ToggleShip(false);
    }

    void Depot_OnDepotStarted(Depot depot)
    {
        hotbar.ToggleBuildDepot(false);
    }

    void Update()
    {     
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

    //used during radial menu
    public void ShipButtonClick()
    {
        if (Player.localPlayer != null)
        {
            Player.localPlayer.Cmd_SetIsBuyingShip(true);
        }
        Player.localPlayer.SelectCell(GameManager.singleton.selectedCell.netId);
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
    
    /// <summary>
    /// Gets the version.
    /// </summary>
    /// <value>The version.</value>
    public string Version
    {
        get
        {
            if (version == null)
            {
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            return version;
        }
    }
}
