using UnityEngine;
using System.Collections;

public class KeyCommands : MonoBehaviour {

	public GameObject canvas;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Z)) {
			if(canvas.GetActive()){
				canvas.SetActive(false);
			}else{
				canvas.SetActive(true);
			}
		}else if (Input.GetKeyDown (KeyCode.F2)) {
			Application.CaptureScreenshot(string.Format("Captura{0}.png", System.DateTime.Now.ToString("_d-MMM-yyyy-HH-mm-ss-f")), 4);
		}

	}
}
