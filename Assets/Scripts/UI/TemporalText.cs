using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
Used to display temporal messages like, hits damage, won xp, etc..
*/

public class TemporalText : MonoBehaviour {

	public Text textObj;
	public Animator anim;

	// Use this for initialization
	void Start () {
		StartCoroutine ("Destroy");
	}

	public void setText(string t, Color c, TextAnchor a, int type) {
		gameObject.GetComponent<Text> ().text = t;
		textObj.text = t;
		textObj.color = c;
		gameObject.GetComponent<Text> ().alignment = a;
		textObj.alignment = a;
		anim.SetInteger ("Type", type);
	}

	IEnumerator Destroy() {
		yield return new WaitForSeconds (4f);
		Destroy (gameObject);
	}
}
