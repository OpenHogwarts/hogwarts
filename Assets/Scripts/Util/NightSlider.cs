using UnityEngine;
using System.Collections;

public class NightSlider : MonoBehaviour {

	public GameObject mainLight;
	public Color mainLightDay;
	public Color mainLightNight;
	public GameObject secLight;
	public Color secLightDay;
	public Color secLightNight;
	public float dayAngle = 68f;
	public float nightAngle = 38f;
	[Range(0.0f, 1.0f)]
	public float slider = 0f;
	
	// Update is called once per frame
	void Update () {
		mainLight.GetComponent<Light> ().color = Color.Lerp (mainLightDay, mainLightNight, slider);
		secLight.GetComponent<Light> ().color = Color.Lerp (secLightDay, secLightNight, slider);
		mainLight.transform.localEulerAngles = Vector3.Lerp (new Vector3 (dayAngle, 200, 0), new Vector3 (nightAngle, 200, 0), slider);
		RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(1f, 0.75f, slider));
		RenderSettings.ambientIntensity = Mathf.Lerp (1.1f, 0.5f, slider);
	}
}
