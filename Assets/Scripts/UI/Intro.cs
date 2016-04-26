using UnityEngine;
using System.Collections;

public class Intro : MonoBehaviour {

	// Use this for initialization
	void Start () {
		((MovieTexture)GetComponent<Renderer>().material.mainTexture).Play();
		GetComponent<AudioSource> ().Play ();
		StartCoroutine ("WaitForMovie");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown) {
			Application.LoadLevel ("MainMenu");
		}
	}

	IEnumerator WaitForMovie(){
		yield return new WaitForSeconds (10);
		Application.LoadLevel ("MainMenu");
	}
}
