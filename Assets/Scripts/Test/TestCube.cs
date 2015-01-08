using UnityEngine;
using System.Collections;

public class TestCube : MonoBehaviour {

	public bool isBad = false;
	public bool giveHealth = false;
	public bool giveExp = false;
	public bool giveMana = false;

	void OnTriggerStay(Collider other) {
		if (other.transform.tag != "Player") {
			return;
		}
		
		Player player = other.transform.GetComponent<Player>();
		int val = 0;

		if (isBad) {
			val = -1;
		} else {
			val = 1;
		}

		if (giveHealth) {
			player.health += val;
		}
		if (giveExp) {
			player.exp += val;
		}
		if (giveMana) {
			player.mana += val;
		}

	}
}
