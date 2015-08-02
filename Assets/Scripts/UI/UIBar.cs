using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBar : MonoBehaviour {


	public Image image;
	public Text text;

	public bool isVertical = false;


	public void updateVertical (int current, int max) {

		if (text != null) {
			text.text = current.ToString();
		}

		// prevent weird bugs in UI if user is "Overpowered"
		if (current > max) {
			current = max;
		}

		image.fillAmount = current / (float)max; // forces float return
	}

	public void updateHoritzontal (int current, int max) {
		if (text != null) {
			text.text = current.ToString();
		}

		// prevent weird bugs in UI
		if (current > max) {
			current = max;
		}

		image.fillAmount = current / (float)max; // forces float return
	}
}
