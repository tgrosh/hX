using UnityEngine;
using System.Collections;
using UnityEngine.Networking.Match;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

public class NetMatch : NetworkMatch {
    public static NetMatch singleton;

    public GameObject playerPrefab;

    private bool isHost = false;
    private object networkId;
    private object nodeId;
    private Color[] playerColors = new Color[] { Color.blue, Color.green };

    void Start()
    {
        singleton = this;
        NetManager.singleton.StartMatchMaker();
    }

    public void CreateGame()
    {
        NetManager.singleton.matchMaker.CreateMatch("NewRoom", 2, true, "", OnMatchCreate);
    }

    public void QuickJoin()
    {
        ListMatches(0, 20, "", (ListMatchResponse matchListResponse) =>
        {
            if (matchListResponse.success && matchListResponse.matches != null)
            {
                NetManager.singleton.matchMaker.JoinMatch(matchListResponse.matches[0].networkId, "", OnMatchJoined);
            }
        });
    }

    public void EndGame()
    {
        if (isHost)
        {
            NetManager.singleton.matchMaker.DestroyMatch((NetworkID)this.networkId, OnDestroyMatch);
            NetManager.singleton.StopHost();
            NetManager.singleton.StopMatchMaker();
        }
        else
        {
            NetManager.singleton.matchMaker.DropConnection((NetworkID)this.networkId, (NodeID)this.nodeId, OnDropConnection);
            NetManager.singleton.StopClient();
            NetManager.singleton.StopMatchMaker();
        }
    }

    private void OnDropConnection(BasicResponse response)
    {
    }

    private void OnDestroyMatch(BasicResponse response)
    {
    }

    private void OnMatchCreate(CreateMatchResponse matchResponse)
    {
        isHost = true;
        this.networkId = matchResponse.networkId;
        this.nodeId = matchResponse.nodeId;

        if (matchResponse.success)
        {
            NetManager.singleton.OnMatchCreate(matchResponse);
        }
    }

    public void OnMatchJoined(JoinMatchResponse matchJoin)
    {
        this.networkId = matchJoin.networkId;
        this.nodeId = matchJoin.nodeId;

        if (matchJoin.success)
        {
            NetManager.singleton.OnMatchJoined(matchJoin);
        }
    }

    public void OnConnected(NetworkMessage msg)
    {
        Debug.Log("Connected!");
    }
}
