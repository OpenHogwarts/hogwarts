using UnityEngine;
using System.Collections;

public class NPCSpawner : MonoBehaviour {

	// Use this for initialization
	void Start () {
		PhotonNetwork.Instantiate("NPC/Spider", transform.position, Quaternion.identity, 0);
	}
}
