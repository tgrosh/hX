using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class ClickHandler : MonoBehaviour {   

    void OnMouseDown()
    {
        if(Input.GetMouseButtonDown(0)){
            GetComponent<GameCell>().SelectCore(GameObject.Find("GameManager").GetComponent<GameManager>().currentPlayerType);
        }
    }
    
}
