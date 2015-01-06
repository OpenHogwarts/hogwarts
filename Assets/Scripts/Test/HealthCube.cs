using UnityEngine;
using System.Collections;

public class HealthCube : MonoBehaviour {

	public bool isBad = false;

	void OnTriggerEnter(Collider other) {
		if (other.transform.tag != "Player") {
			return;
		}
		
		Player player = other.transform.GetComponent<Player>();

		if (isBad) {
			player.health -= 10;
		} else {
			player.health += 10;
		}
	}
}
