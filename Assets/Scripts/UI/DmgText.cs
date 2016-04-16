using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DmgText : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine ("Destroy");
	}

	public void SetDmg(string t, Color c, TextAnchor a, int type){
		gameObject.GetComponent<Text> ().text = t;
		gameObject.GetComponent<Text> ().color = c;
		gameObject.GetComponent<Text> ().alignment = a;
		gameObject.GetComponent<Animator> ().SetInteger ("Type", type);
	}

	IEnumerator Destroy(){
		yield return new WaitForSeconds (2f);
		Destroy (gameObject);
	}
}
