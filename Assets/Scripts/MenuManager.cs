using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    public void CreateMultiplayerGame()
    {        
        NetManager.singleton.StartHost();

        SetPlayerName();
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
}
