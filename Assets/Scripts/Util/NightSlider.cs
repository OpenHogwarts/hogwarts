using UnityEngine;
using System.Collections;

public class NightSlider : MonoBehaviour {

	public GameObject mainLight;
	public Color mainLightDay;
	public Color mainLightNight;
	public GameObject secLight;
	public Color secLightDay;
	public Color secLightNight;
	[Range(0.0f, 1.0f)]
	public float slider = 0f;
	
	// Update is called once per frame
	void Update () {
		mainLight.GetComponent<Light> ().color = Color.Lerp (mainLightDay, mainLightNight, slider);
		secLight.GetComponent<Light> ().color = Color.Lerp (secLightDay, secLightNight, slider);
	}
}
