using UnityEngine;
using System.Collections;

/*
NPC detection is separated into a empty child to prevent having 2 collider components in the same GameObject (root has already 1)
 */

public class NPCDetector : MonoBehaviour {


	public void OnTriggerEnter (Collider other) {
		this.canBeTargeted(other);
	}
	
	public void OnTriggerStay (Collider other) {
		this.canBeTargeted(other);
	}

	public void canBeTargeted (Collider other) {

		if (other.gameObject.tag != "NPC") {return;}
	
		Vector3 hitPoint;

		Ray ray = new Ray(transform.position, other.gameObject.transform.position);
		Transform hitTransform = FindClosestHitObject(ray, 100, out hitPoint);

		// looks like there is nothing between us and the player
		if (hitTransform == null) {
			other.gameObject.GetComponent<NPC>().setTarget (this.gameObject.transform.parent.gameObject);
		}

	}

	Transform FindClosestHitObject (Ray ray, float distance, out Vector3 hitPoint) {
		RaycastHit[] hits = Physics.RaycastAll(ray);
		
		Transform closestHit = null;
		hitPoint = Vector3.zero;
		
		foreach (RaycastHit hit in hits) {
			if (hit.transform != this.transform && (closestHit == null || hit.distance < distance) ) {
				closestHit = hit.transform;
				distance = hit.distance;
				hitPoint = hit.point;
			}
		}
		return closestHit;
	}
}
