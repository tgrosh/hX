using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameBoard : NetworkBehaviour {

    public GameObject emptyBoardSpace;

    private List<GameObject> boardSpaces = new List<GameObject>();
        
    public void CreateBoard(float numCols, float numRows, float boardSpacing)
    {
        float posX, posY;
        float spaceWidth = (emptyBoardSpace.GetComponent<Renderer>().bounds.size.x * boardSpacing);
        float spaceHeight = (emptyBoardSpace.GetComponent<Renderer>().bounds.size.z * boardSpacing) * .75f;
        float startX = numCols / 2 * -spaceWidth + spaceWidth / 2;
        float startY = numRows / 2 * -spaceHeight + spaceHeight / 2;
        GameObject obj = null;

        //create hex board
        for (var y = 0; y < numCols; y++)
        {
            for (var x = 0; x < numRows; x++)
            {
                posX = x * spaceWidth;
                posY = y * spaceHeight;

                if (y % 2 == 0)
                {
                    //even rows
                    posX -= (emptyBoardSpace.GetComponent<Renderer>().bounds.size.x / 2f);
                }
                                
                obj = (GameObject)Instantiate(emptyBoardSpace, new Vector3(startX + posX, startY + posY, 0), Quaternion.identity);
                obj.name = x + "." + y;
                obj.transform.parent = transform;
                
                boardSpaces.Add(obj);
            }
        }
    }
}
