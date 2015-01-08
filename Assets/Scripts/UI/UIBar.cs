using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBar : MonoBehaviour {

	private float cachedAxis;
	private float minValue;
	private float maxValue;

	public RectTransform rect;
	public Text text;

	public bool isVertical = false;
	private bool firstUpdate = true;


	// Use this for initialization
	void Start () {
		if (isVertical) {
			cachedAxis = rect.position.x;
			maxValue = rect.position.y;
			minValue = rect.position.y - rect.rect.height;
		} else {
			cachedAxis = rect.position.y;
			maxValue = rect.position.x;
			minValue = rect.position.x - rect.rect.width;
		}
	}

	public void updateVertical (int current, int max) {
		text.text = current.ToString();

		// prevent weird bugs in UI if user is "Overpowered"
		if (current > max) {
			current = max;
		}

		float currentYValue = mapValues (current, 0, max, minValue, maxValue);

		if (firstUpdate) {
			rect.position = new Vector3(cachedAxis, currentYValue);
			firstUpdate = false;
		} else {
			rect.position = Vector3.Lerp(rect.position, new Vector3(cachedAxis, currentYValue), Time.deltaTime * 5);
		}
	}

	public void updateHoritzontal (int current, int max) {
		//text.text = exp.ToString();

		// prevent weird bugs in UI
		if (current > max) {
			current = max;
		}
		
		float currentXValue = mapValues (current, 0, max, minValue, maxValue);

		if (firstUpdate) {
			rect.position = new Vector3(currentXValue, cachedAxis);
			firstUpdate = false;
		} else {
			rect.position = Vector3.Lerp(rect.position, new Vector3(currentXValue, cachedAxis), Time.deltaTime * 5);
		}
	}

	private float mapValues (float x, float inMin, float inMax, float outMin, float outMax) {
		return (x - inMin) * (outMax -outMin) / (inMax -inMin) + outMin;
	}
}
