using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

/*
Base code from Celtc:
http://answers.unity3d.com/questions/8676/make-rigidbody-walk-like-character-controller.html
 */

public class Motor : MonoBehaviour
{
	#region Variables (private)
	
	private bool grounded = false;
	private Vector3 groundVelocity;
	private CapsuleCollider capsule;
	
	// Inputs Cache
	private bool jumpFlag = false;
	private bool isMouseControlled = false;
	private Animator anim;
	
	#endregion
	
	#region Properties (public)

	public Camera cam;
	
	// Speeds
	public float walkSpeed = 8.0f;
	public float walkBackwardSpeed = 4.0f;
	public float runSpeed = 14.0f;
	public float runBackwardSpeed = 6.0f;
	public float sidestepSpeed = 8.0f;
	public float runSidestepSpeed = 12.0f;
	public float maxVelocityChange = 10.0f;
	
	// Air
	public float inAirControl = 0.1f;
	public float jumpHeight = 1.5f;
	
	// Can Flags
	public bool canRunSidestep = true;
	public bool canJump = true;
	#endregion
	
	#region Unity event functions
	
	/// <summary>
	/// Use for initialization
	/// </summary>
	void Awake()
	{
		capsule = GetComponent<CapsuleCollider>();
		rigidbody.freezeRotation = true;
		rigidbody.useGravity = true;
	}
	
	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start()
	{
		anim = GetComponent<Animator> ();
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update()
	{
		if (Input.GetButtonDown ("Jump") && !Chat.isWritting) {
			jumpFlag = true;
		}
		if (Input.GetKey (KeyCode.Mouse0) && Input.GetKey (KeyCode.Mouse1)) {
			isMouseControlled = true;
		} else {
			isMouseControlled = false;
		}
	}
	
	/// <summary>
	/// Update for physics
	/// </summary>
	void FixedUpdate()
	{
		Vector3 inputVector;

		if (isMouseControlled) {
			inputVector = transform.TransformDirection (Vector3.forward);
		} else {
			inputVector = new Vector3(0, 0, Input.GetAxis("Vertical"));
		}


		//If is chating, do not move
		if (Chat.isWritting) {
			inputVector.z = 0f;
		} else {
			if (isMouseControlled) {
				transform.rotation = Quaternion.Euler (0, cam.transform.rotation.eulerAngles.y, 0);
			} else {
				transform.Rotate(new Vector3(0,Input.GetAxis("Horizontal"), 0), Mathf.Clamp(90f * Time.deltaTime, 0f, 360f));
			}
		}
		
		// On the ground
		if (grounded)
		{
			// Apply a force that attempts to reach our target velocity
			var velocityChange = CalculateVelocityChange(inputVector);
			rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
			
			// Jump
			if (canJump && jumpFlag)
			{
				jumpFlag = false;
				rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y + CalculateJumpVerticalSpeed(), rigidbody.velocity.z);
			} else {
				anim.SetBool ("Jumping", false);
				anim.SetFloat("Speed", rigidbody.velocity.magnitude);
			}
			
			// By setting grounded to false in every FixedUpdate we avoid
			// checking if the character is not grounded on OnCollisionExit()
			grounded = false;
		}
		// In mid-air
		else
		{
			// Uses the input vector to affect the mid air direction
			var velocityChange = transform.TransformDirection(inputVector) * inAirControl;
			rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
			anim.SetBool ("Jumping", true);
		}
	}
	
	// Unparent if we are no longer standing on our parent
	void OnCollisionExit(Collision collision)
	{
		if (collision.transform == transform.parent)
			transform.parent = null;
	}
	
	// If there are collisions check if the character is grounded
	void OnCollisionStay(Collision col)
	{
		TrackGrounded(col);
	}
	
	void OnCollisionEnter(Collision col)
	{
		TrackGrounded(col);
	}
	
	#endregion
	
	#region Methods
	
	// From the user input calculate using the setup speeds the velocity change
	private Vector3 CalculateVelocityChange(Vector3 inputVector)
	{
		// Calculate how fast we should be moving
		var relativeVelocity = transform.TransformDirection(inputVector);
		if (inputVector.z > 0)
		{
			relativeVelocity.z *= walkSpeed;
		}
		else
		{
			relativeVelocity.z *= walkBackwardSpeed;
		}
		relativeVelocity.x *= sidestepSpeed;
		
		// Calculate delta velocity
		var currRelativeVelocity = rigidbody.velocity - groundVelocity;
		var velocityChange = relativeVelocity - currRelativeVelocity;
		velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
		velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
		velocityChange.y = 0;
		
		return velocityChange;
	}
	
	// From the jump height and gravity we deduce the upwards speed for the character to reach at the apex.
	private float CalculateJumpVerticalSpeed()
	{
		return Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(Physics.gravity.y));
	}
	
	// Check if the base of the capsule is colliding to track if it's grounded
	private void TrackGrounded(Collision collision)
	{
		var maxHeight = capsule.bounds.min.y + capsule.radius * .9f;
		foreach (var contact in collision.contacts)
		{
			if (contact.point.y < maxHeight)
			{
				if (isKinematic(collision))
				{
					// Get the ground velocity and we parent to it
					groundVelocity = collision.rigidbody.velocity;
					transform.parent = collision.transform;
				}
				else if (isStatic(collision))
				{
					// Just parent to it since it's static
					transform.parent = collision.transform;
				}
				else
				{
					// We are standing over a dinamic object,
					// set the groundVelocity to Zero to avoid jiggers and extreme accelerations
					groundVelocity = Vector3.zero;
				}

				grounded = true;
			}
			
			break;
		}
	}
	
	private bool isKinematic(Collision collision)
	{
		return isKinematic(collider.transform);
	}
	
	private bool isKinematic(Transform transform)
	{
		return transform.rigidbody && transform.rigidbody.isKinematic;
	}
	
	private bool isStatic(Collision collision)
	{
		return isStatic(collision.transform);
	}
	
	private bool isStatic(Transform transform)
	{
		return transform.gameObject.isStatic;
	}
	
	#endregion
}