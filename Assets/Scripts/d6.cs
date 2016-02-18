using UnityEngine;
using System.Collections;

public class d6 : MonoBehaviour {

    public float velocityMin;
    public float velocityMax;
    public float rotationMin;
    public float rotationMax;
    public float displaySpeed;
    public int value;

    Vector3 displayPosition = Vector3.zero;
    Vector3 startPosition;
    Vector3 randomRotation;
    float randomMovement;
    Rigidbody body;

    public delegate void DiceRollComplete(d6 die);
    public static event DiceRollComplete OnDiceRollComplete;

	// Use this for initialization
	void Awake () {
        startPosition = transform.position;
        body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        if (body.IsSleeping() && value == 0)
        {
            GetDiceCount();
            body.isKinematic = true;

            if (OnDiceRollComplete != null)
            {
                OnDiceRollComplete(this);
            }
        }

        if (displayPosition != Vector3.zero && displayPosition != transform.position)
        {
            transform.position = Vector3.Lerp(transform.position, displayPosition, Time.deltaTime * displaySpeed);

            if (Vector3.Distance(transform.position, displayPosition) < .01f)
            {
                transform.position = displayPosition;
                displayPosition = Vector3.zero;
            }
        }
	}

    public void Roll()
    {
        body.isKinematic = false;

        randomMovement = Random.Range(velocityMin, velocityMax);
        body.AddForce(new Vector3(randomMovement * -.5f, randomMovement * .5f, randomMovement * .5f), ForceMode.Impulse);

        transform.Rotate(Vector3.forward * Random.Range(rotationMin, rotationMax));
        randomRotation = Vector3.right * Random.Range(rotationMin, rotationMax);
        body.AddTorque(randomRotation, ForceMode.Force);
    }

    public void Display(Vector3 position)
    {
        displayPosition = position;
    }

    public void Reset()
    {
        transform.position = startPosition;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void GetDiceCount()
    {
        if (Vector3.Dot(transform.forward, Vector3.back) > .99f)
            value = 5;
        else if (Vector3.Dot(transform.forward, Vector3.back) < -.99f)
            value = 2;
        else if (Vector3.Dot(transform.right, Vector3.back) > .99f)
            value = 4;
        else if (Vector3.Dot(transform.right, Vector3.back) < -.99f)
            value = 3;
        else if (Vector3.Dot(transform.up, Vector3.back) > .99f)
            value = 6;
        else if (Vector3.Dot(transform.up, Vector3.back) < -.99f)
            value = 1;
    }
}
