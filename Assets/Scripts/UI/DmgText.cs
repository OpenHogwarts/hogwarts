using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DmgText : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine ("Destroy");
	}

	public void SetDmg(string t){
		gameObject.GetComponent<Text> ().text = t;
	}

	IEnumerator Destroy(){
		yield return new WaitForSeconds (1f);
		Destroy (gameObject);
	}
}
