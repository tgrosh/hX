using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

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
}
