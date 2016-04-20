using UnityEngine;
using System.Collections;

public class BroomstickControl : MonoBehaviour {

	public Rigidbody rigidbody;
	public float curSpeed = 0f;
	public float maxSpeed = 10.0f;
	public float damping = 1f;
	private Vector3 rotation;
	private Vector3 targetVelocity;

	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
	}

	void FixedUpdate () {
		curSpeed = Mathf.Lerp(curSpeed, (Input.GetAxis ("Vertical"))*maxSpeed, 0.1f);
		targetVelocity = new Vector3 (0, 0, curSpeed);
		targetVelocity = Camera.main.transform.TransformDirection (targetVelocity);


		rigidbody.velocity = targetVelocity;
	}

	void Update(){
		var rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
	}
}
