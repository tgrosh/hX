using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CellClickHandler : NetworkBehaviour {   

    void OnMouseDown()
    {
        if(Input.GetMouseButtonDown(0) && !GameManager.singleton.gameBoardLocked){
            Player.localPlayer.SelectCell(this.netId);

            if (GetComponent<GameCell>().hasShip)
            {   
                GameManager.singleton.cam.SetTarget(ClientScene.FindLocalObject(GetComponent<GameCell>().associatedShip).GetComponent<FleetVessel>().cameraTarget);
            }
        }
    }
    
}
