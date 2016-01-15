using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetManager : NetworkManager {

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.forward, Quaternion.identity);
        player.GetComponent<Player>().color = Color.yellow;
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}
