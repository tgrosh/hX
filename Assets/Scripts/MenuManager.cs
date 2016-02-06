using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts;

public class MenuManager : MonoBehaviour
{
    public GameObject GameSetupPanel;
    public GameRule ruleEmpty;
    public GameRule ruleOwnArea;
    public GameRule ruleOwnCore;
    public GameRule ruleEnemyArea;
    public GameRule ruleEnemyCore;

    private GameObject hotbar;    

    void Start()
    {

    }

    void Update()
    {
        hotbar = GameObject.Find("Hotbar");
        if (hotbar != null)
        {
            foreach (Toggle t in hotbar.GetComponentsInChildren<Toggle>())
            {
                if (t.isOn)
                {
                    t.image.color = t.colors.highlightedColor;
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
        Player.localPlayer.Cmd_SetIsBuyingShip(isOn);
    }
}
