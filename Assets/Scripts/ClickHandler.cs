using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.Networking;

public class ClickHandler : NetworkBehaviour {   

    void OnMouseDown()
    {
        if(Input.GetMouseButtonDown(0)){
            Player.localPlayer.SelectCell(GetComponent<GameCell>().name);
        }
    }
    
}
