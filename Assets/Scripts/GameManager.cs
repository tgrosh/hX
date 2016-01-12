using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class GameManager : MonoBehaviour {

    public Camera gameCamera;
    public int numRows = 10;
    public int numCols = 10;
    public GameObject boardSpace;
    public PlayerType currentPlayerType = PlayerType.One;

    private float boardSpacing = 1.05f;

	// Use this for initialization
	void Start () {
        float posX, posY;
        float spaceWidth = (boardSpace.GetComponent<Renderer>().bounds.size.x * boardSpacing);
        float spaceHeight = (boardSpace.GetComponent<Renderer>().bounds.size.z * boardSpacing) * .75f;
        float startX = numCols / 2 * -spaceWidth + spaceWidth/2;
        float startY = numRows / 2 * -spaceHeight + spaceHeight/2;
        GameObject obj = null;

	    //create hex board
        for (var y = 0; y < numCols; y++) {
            for (var x = 0; x < numRows; x++) {
                posX = x * spaceWidth;
                posY = y * spaceHeight;

                if (y % 2 == 0)
                {
                    //even rows
                    posX -= (boardSpace.GetComponent<Renderer>().bounds.size.x / 2f);
                }

                obj = (GameObject)Instantiate(boardSpace, new Vector3(startX + posX, startY + posY, 0), Quaternion.identity);
                obj.transform.parent = GameObject.Find("GameBoard").transform;
            }
        }

        gameCamera.transform.LookAt(GameObject.Find("GameBoard").transform);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void EndPlayerTurn()
    {
        if (currentPlayerType == PlayerType.One)
        {
            currentPlayerType = PlayerType.Two;
        }
        else
        {
            currentPlayerType = PlayerType.One;
        }
    }
}
