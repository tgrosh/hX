using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public int numRows = 10;
    public int numCols = 10;
    public GameObject boardSpace;
    private float spacing = 1.05f;

	// Use this for initialization
	void Start () {
        float posX, posY;
        GameObject obj = null;

	    //create hex board
        for (var y = 0; y < numCols; y++) {
            for (var x = 0; x < numRows; x++) {
                posX = x * (boardSpace.GetComponent<Renderer>().bounds.size.x * spacing);
                posY = y * (boardSpace.GetComponent<Renderer>().bounds.size.z * spacing) * .75f;

                if (y % 2 == 0)
                {
                    //even rows
                    posX -= (boardSpace.GetComponent<Renderer>().bounds.size.x / 2f);
                }

                obj = (GameObject)Instantiate(boardSpace, new Vector3(posX, posY, 0), Quaternion.identity);
                obj.transform.parent = GameObject.Find("GameBoard").transform;
            }
        }

        Camera.main.transform.LookAt(GameObject.Find("GameBoard").transform);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
