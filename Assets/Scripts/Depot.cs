using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class Depot : NetworkBehaviour {
    [SyncVar]
    public Color color;
    public Player owner;
    private float rotateSpeed = 5f;
    public Transform depot;
    private bool isColorSet;
    private bool animatingEntrance;
    private float animationCurrentTime;
    private float animationSpeed = 3f;
    private Vector3 origPosition;

	// Use this for initialization
    void Start()
    {
        origPosition = transform.position;
        depot = transform.FindChild("DepotModel");
        transform.position = transform.position + new Vector3(0, 0, 1);
        animatingEntrance = true;

        MenuManager.singleton.ToggleBuildDepot(false);

        if (isServer)
        {
            GameManager.singleton.AddEvent(String.Format("Player {0} created a new Depot", GameManager.singleton.CreateColoredText(owner.seat.ToString(), owner.color)));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (animatingEntrance && animationCurrentTime / animationSpeed < .95f)
        {
            transform.position = Vector3.Lerp(transform.position, origPosition, animationSpeed * Time.deltaTime);
            animationCurrentTime += Time.deltaTime;
        }
        else if (animatingEntrance)
        {
            transform.localPosition = origPosition;

            animationCurrentTime = 0;
            animatingEntrance = false;

            //GameManager.singleton.ResetCamera();
            //if (OnShipSpawnEnd != null)
            //{
            //    OnShipSpawnEnd();
            //}
        }

        depot.Rotate(Vector3.up * Time.deltaTime * rotateSpeed);

        if (!isColorSet)
        {
            SetColor(this.color);
            isColorSet = true;
        }
    }

    [Client]
    private void SetColor(Color color)
    {
        foreach (Renderer rend in transform.FindChild("DepotModel").GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in rend.materials)
            {
                mat.color = color + (Color.white * .5f);
            }
        }
    }
}
