using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float rotateSpeed;
	public float forwardSpeed;
	private CharacterController playerController;
	private Animator anim;

	// Use this for initialization
	void Start () {
		playerController = GetComponent<CharacterController> ();
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (Chat.isWritting) {
			return;
		}

		if (Input.GetKeyDown ("space") && playerController.isGrounded) {
			playerController.Move (Vector3.up);
			anim.SetBool ("Jumping", true);
		} else {
			anim.SetBool("Jumping", false);
		}

		transform.Rotate(0, Input.GetAxis("Horizontal")*rotateSpeed, 0);
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		float speed = forwardSpeed * Input.GetAxis ("Vertical");

		playerController.SimpleMove(speed * forward);
		anim.SetFloat("Speed", speed);

	}
}
