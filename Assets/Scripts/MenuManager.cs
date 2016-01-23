using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject GameSetupPanel;
    public GameRule ruleEmpty;
    public GameRule ruleOwnArea;
    public GameRule ruleOwnCore;
    public GameRule ruleEnemyArea;
    public GameRule ruleEnemyCore;

    void Start()
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
}
