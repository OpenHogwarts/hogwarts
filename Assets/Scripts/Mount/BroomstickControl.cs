using UnityEngine;
using System.Collections;

public class BroomstickControl : MonoBehaviour {

	public Rigidbody rigidbody;
	public GameObject models;
	public float curSpeed = 0f;
	public float maxSpeed = 10.0f;
	public float curVSpeed = 0f;
	public float verticalSpeed = 2.0f;
	public float rotateSpeed = 1.0f;
	private Vector3 rotation;
	private Vector3 targetVelocity;

	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
	}

	void FixedUpdate () {
		curVSpeed = Input.GetAxis ("Jump") * verticalSpeed;
		if(Input.GetKey(KeyCode.LeftControl)){
			curVSpeed = -1 * verticalSpeed;
		}
		curSpeed = Mathf.Lerp(curSpeed, (Input.GetAxis ("Vertical"))*maxSpeed, 0.1f);
		targetVelocity = new Vector3 (0, curVSpeed, curSpeed);
		targetVelocity = transform.TransformDirection (targetVelocity);

		rigidbody.velocity = targetVelocity;

		if (Input.GetAxis ("Vertical") > 0) {
			rotation = new Vector3 (0f, Input.GetAxis ("Horizontal"), 0f);
		} else if (Input.GetAxis ("Vertical") < 0) {
			rotation = new Vector3 (0f, -(Input.GetAxis ("Horizontal")), 0f);
		} else {
			rotation = new Vector3 (0f, Input.GetAxis ("Horizontal"), 0f);
		}
		rotation *= rotateSpeed;

		rigidbody.MoveRotation (transform.rotation * Quaternion.Euler (rotation));
	}
}
