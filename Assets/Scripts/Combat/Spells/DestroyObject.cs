using UnityEngine;
using System.Collections;

public class DestroyObject : MonoBehaviour {

	// Use this for initialization
	public float destroyTime = 10.0f;

	void Start () {
		StartCoroutine (DestroyOverTime(destroyTime));
	}
	
	// Update is called once per frame
	public IEnumerator DestroyOverTime (float time) {

		yield return new WaitForSeconds (time);
		Destroy (this.gameObject);
	
	}
}
