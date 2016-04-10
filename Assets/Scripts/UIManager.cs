using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Reflection;

[assembly: AssemblyVersion("1.0.0.*")]
public class UIManager : NetworkBehaviour
{
    public static UIManager singleton;
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
    }    

    void Update()
    {     
    }

    public void CreateMultiplayerGame()
    {
        NetManager.singleton.StartHost();
    }

    public void JoinMultiplayerGame(Text hostName)
    {
        NetManager.singleton.networkAddress = hostName.text;
        NetManager.singleton.StartClient();
    }

    public void DisconnectMultiplayer()
    {
        NetManager.singleton.StopHost();
        NetManager.singleton.StopClient();
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

    public void ShowShipTooltip(FleetVessel ship)
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
