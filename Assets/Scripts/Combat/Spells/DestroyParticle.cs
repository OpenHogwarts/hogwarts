using UnityEngine;
using System.Collections;

public class DestroyParticle : MonoBehaviour {

	// Use this for initialization
	void Start () {

		ParticleSystem ps = GetComponentInChildren<ParticleSystem> ();
		Destroy (this.gameObject,ps.duration);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
