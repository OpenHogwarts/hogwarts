using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour {

	private float cachedX;
	private float minYValue;
	private float maxYValue;

	public RectTransform RemainingHealth;
	public Text healthText;
	public static HealthBar _instance;
	
	public static HealthBar Instance {
		get
		{
			return _instance;
		}
	}

	// Use this for initialization
	void Start () {
		_instance = this;

		cachedX = RemainingHealth.position.x;
		maxYValue = RemainingHealth.position.y;
		minYValue = RemainingHealth.position.y - RemainingHealth.rect.height;
	}

	public void updateUI (int health, int maxHealth) {
		healthText.text = health.ToString();

		// prevent weird bugs in UI if user is "Overpowered"
		if (health > maxHealth) {
			health = maxHealth;
		}

		float currentYValue = mapValues (health, 0, maxHealth, minYValue, maxYValue);
		RemainingHealth.position = Vector3.Lerp(RemainingHealth.position, new Vector3(cachedX, currentYValue), Time.deltaTime * 5);
	}

	private float mapValues (float x, float inMin, float inMax, float outMin, float outMax) {
		return (x - inMin) * (outMax -outMin) / (inMax -inMin) + outMin;
	}
}
