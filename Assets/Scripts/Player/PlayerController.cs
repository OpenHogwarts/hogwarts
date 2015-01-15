using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float rotateSpeed;
	public float forwardSpeed;
	public float jumpHeight;
	public Camera cam;
	private CharacterController playerController;
	private Animator anim;
	private Vector3 forward;
	private float speed;

	// Use this for initialization
	void Start () {
		playerController = GetComponent<CharacterController> ();
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {

		//If chating, do not move
		if (Chat.isWritting) {
			return;
		}

		//Jump behaviour
		if (Input.GetKeyDown ("space") && playerController.isGrounded) {
			playerController.Move (Vector3.up*jumpHeight);
			anim.SetBool ("Jumping", true);
		} else {
			anim.SetBool("Jumping", false);
		}

		//If you click with both mouse buttons, move in the camera direction, else, move using WASD
		if (Input.GetKey (KeyCode.Mouse0) && Input.GetKey (KeyCode.Mouse1)) {
			forward = transform.TransformDirection (Vector3.forward);

			speed = forwardSpeed;
			transform.rotation = Quaternion.Euler (0, cam.transform.rotation.eulerAngles.y, 0);
		} else {
			transform.Rotate(0, Input.GetAxis("Horizontal")*rotateSpeed, 0);
			forward = transform.TransformDirection(Vector3.forward);
			speed = forwardSpeed * Input.GetAxis ("Vertical");
		}

		playerController.SimpleMove(speed * forward);
		anim.SetFloat("Speed", speed);

	}
}
