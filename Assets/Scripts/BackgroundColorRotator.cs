using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BackgroundColorRotator : MonoBehaviour {

    public float colorChangeSpeed = .25f;

    Color color;
    float blueSpeed, greenSpeed, redSpeed;
    float blueDirection = -1f, greenDirection = -1f, redDirection = -1f;

	// Use this for initialization
	void Start () {
        color = this.gameObject.GetComponent<Image>().color;
        blueSpeed = Random.value * colorChangeSpeed + (colorChangeSpeed*.5f);
        greenSpeed = Random.value * colorChangeSpeed + (colorChangeSpeed * .5f);
        redSpeed = Random.value * colorChangeSpeed + (colorChangeSpeed * .5f);
	}
	
	// Update is called once per frame
	void Update () {
        updateColor(ref color.r, redSpeed, ref redDirection);
        updateColor(ref color.g, greenSpeed, ref greenDirection);
        updateColor(ref color.b, blueSpeed, ref blueDirection);

        this.gameObject.GetComponent<Image>().color = color;
	}

    void updateColor(ref float colorValue, float speed, ref float direction) {
        colorValue += (Time.deltaTime * speed) * direction;
        if (colorValue >= 1f)
        {
            colorValue = 1f;
            direction *= -1f;
        }
        else if (colorValue <= 0f)
        {
            colorValue = 0f;
            direction *= -1f;
        }
    }
}
