using UnityEngine;
using System.Collections;

public class BroomstickControl : MonoBehaviour {

	public Rigidbody rigidbody;
	public float curSpeed = 0f;
    public float upSpeed = 0f;
    public float latSpeed = 0f;
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
        if (Input.GetKey("space"))
            upSpeed = Mathf.Lerp(upSpeed, maxSpeed, 0.1f);
        else
            upSpeed = 0f;

        latSpeed = Mathf.Lerp(latSpeed, (Input.GetAxis("Horizontal")) * maxSpeed, 0.1f);


        targetVelocity = new Vector3 (latSpeed, upSpeed, curSpeed);
		targetVelocity = Camera.main.transform.TransformDirection (targetVelocity);


		rigidbody.velocity = targetVelocity;
	}

	void Update(){
		var rotation = Quaternion.LookRotation(transform.position - Vector3.Scale(Camera.main.transform.position, new Vector3(1, 0.99f, 1)));
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
	}
}
