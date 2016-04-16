using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
Used to display temporal messages like, hits damage, won xp, etc..
*/

public class TemporalText : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine ("Destroy");
	}

	public void setText(string t, Color c, TextAnchor a, int type) {
		gameObject.GetComponent<Text> ().text = t;
		gameObject.GetComponent<Text> ().color = c;
		gameObject.GetComponent<Text> ().alignment = a;
		gameObject.GetComponent<Animator> ().SetInteger ("Type", type);
	}

	IEnumerator Destroy() {
		yield return new WaitForSeconds (4f);
		Destroy (gameObject);
	}
}
