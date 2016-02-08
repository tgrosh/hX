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

    private GameObject hotbar;    

    void Start()
    {
        singleton = this;
        hotbar = GameObject.Find("Hotbar");
    }

    void Update()
    {        
        if (hotbar != null)
        {
            foreach (Toggle t in hotbar.GetComponentsInChildren<Toggle>())
            {
                if (t.isOn)
                {
                    t.image.color = Color.yellow;
                }
                else
                {
                    t.image.color = Color.white;
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

    void MenuManager_OnCameraReachedDestination()
    {
        Debug.Log("Camera in Position");
        GameManager.singleton.cam.GetComponent<CameraWatcher>().OnCameraReachedDestination -= MenuManager_OnCameraReachedDestination;
    }
}
