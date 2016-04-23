using UnityEngine;
using System.Collections;

public class NPCActivator : MonoBehaviour {

	void Start(){
	}

	void OnTriggerEnter(Collider col){
		if ((col.tag == "NPC")&&(!col.isTrigger)) {
			if (col.gameObject.GetComponent<PhotonView> ().owner == null) {
				Debug.Log ("Now you are the owner");
				col.gameObject.GetComponent<PhotonView> ().TransferOwnership (PhotonNetwork.player);
			}
			col.gameObject.GetComponent<Animation> ().enabled = true;
			//col.gameObject.GetComponent<NPC> ().enabled = true;
			col.gameObject.transform.FindChild ("Model").gameObject.SetActive (true);
		}
	}

	void OnTriggerExit(Collider col){
		if ((col.tag == "NPC")&&(!col.isTrigger)) {
			Debug.Log ("Now the scene owns the object, previous owner was "+col.gameObject.GetComponent<PhotonView> ().owner.name);
			col.gameObject.GetComponent<PhotonView> ().TransferOwnership (PhotonNetwork.masterClient);
			col.gameObject.GetComponent<Animation> ().enabled = false;
			//col.gameObject.GetComponent<NPC> ().enabled = false;
			col.gameObject.transform.FindChild ("Model").gameObject.SetActive (false);
		}
	}
}
