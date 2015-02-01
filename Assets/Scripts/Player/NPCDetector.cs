using UnityEngine;
using System.Collections;

/*
NPC detection is separated into a empty child to prevent having 2 collider components in the same GameObject (root has already 1)
 */

public class NPCDetector : MonoBehaviour {

	public void OnTriggerEnter (Collider other) {
		if (other.gameObject.tag != "NPC") {return;}
		other.gameObject.GetComponent<NPC>().setTarget (this.gameObject.transform.parent.gameObject);
	}
	
	public void OnTriggerStay (Collider other) {
		if (other.gameObject.tag != "NPC") {return;}
		other.gameObject.GetComponent<NPC>().setTarget (this.gameObject.transform.parent.gameObject);
	}
}
