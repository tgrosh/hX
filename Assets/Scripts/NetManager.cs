using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class NetManager : NetworkManager {

    private Color[] playerColors = new Color[] {Color.blue, Color.green};

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.forward, Quaternion.identity);
        player.GetComponent<Player>().color = playerColors[NetworkServer.connections.Count-1];
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        
        GameManager.singleton.AddPlayer(player.GetComponent<Player>());
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController playerController)
    {
        // remove players from slots
        var player = playerController.gameObject.GetComponent<Player>();
        //GameObject.Find("GameManager").GetComponent<GameManager>().RemovePlayer(player);
        GameManager.singleton.RemovePlayer(player);

        base.OnServerRemovePlayer(conn, playerController);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        foreach (var playerController in conn.playerControllers)
        {
            var player = playerController.gameObject.GetComponent<Player>();
            //GameObject.Find("GameManager").GetComponent<GameManager>().RemovePlayer(player);
            GameManager.singleton.RemovePlayer(player);
        }

        base.OnServerDisconnect(conn);
    }
}
