using UnityEngine;
using System.Collections;

public class MiniMap : MonoBehaviour {

	public Transform target;
	
	// Update is called once per frame
	void Update () {
		if (!target) {
			return;
		}
		transform.position = new Vector3 (target.position.x, transform.position.y, target.position.z);
	}
}
