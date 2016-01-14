using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class ClickHandler : MonoBehaviour {   

    void OnMouseDown()
    {
        if(Input.GetMouseButtonDown(0)){
            GameObject.Find("GameManager").GetComponent<GameManager>().currentPlayer.Cmd_SelectCell(GetComponent<GameCell>().name);
        }
    }
    
}
