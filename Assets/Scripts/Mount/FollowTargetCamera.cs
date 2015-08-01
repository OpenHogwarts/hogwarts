using UnityEngine;

public class FollowTargetCamera : MonoBehaviour
{
    public Transform Target;
    public float PositionFollowForce = 5f;
    public float RotationFollowForce = 5f;
	public float maxViewDistance = 25;
	public float minViewDistance = 1;
	public int zoomRate = 30;
	public float cameraTargetHeight = 1.0f;

	public float distance = 6;
	public float desiredDistance;
	public float correctedDistance;
	public float currentDistance;


	private float oldDistance;
	private int lerpRate = 5;
	private float x = 0.0f;
	private float y = 0.0f;
	private int mouseXSpeedMod = 5;
	private int mouseYSpeedMod = 3;


	void Start () {
		Vector3 angles = transform.eulerAngles;
		x = angles.x;
		y = angles.y;
	}

    void FixedUpdate()
	{
		bool manualControl = false;

		if (Input.GetMouseButton (1)) {
			x += Input.GetAxis ("Mouse X") * mouseXSpeedMod;
			y -= Input.GetAxis ("Mouse Y") * mouseYSpeedMod;
			manualControl = true;
		}

		if (manualControl) {
			y = ClampAngle (y, -50, 80);
			
			Quaternion rotation = Quaternion.Euler (y, x, 0);

			desiredDistance -= Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs (desiredDistance);
			desiredDistance = Mathf.Clamp (desiredDistance, minViewDistance, maxViewDistance);
			correctedDistance = desiredDistance;
			currentDistance = correctedDistance;
			
			Vector3 position = Target.position - (rotation * Vector3.forward * desiredDistance);
			
			position = Target.position - (rotation * Vector3.forward * currentDistance + new Vector3 (0, -cameraTargetHeight, 0));

			transform.rotation = rotation;
			transform.position = position;
		} else {
			var vector = Vector3.forward;
			var dir = Target.rotation * Vector3.forward;
			dir.y = 0f;
			if (dir.magnitude > 0f) vector = dir / dir.magnitude;

			transform.position = Vector3.Lerp(transform.position, Target.position, PositionFollowForce * Time.deltaTime);
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vector), RotationFollowForce * Time.deltaTime);
		}
        
	}

	private static float ClampAngle(float angle, float min, float max){
		if (angle < -360) {
			angle += 360;
		}
		if (angle > 360) {
			angle -= 360;
		}
		
		return Mathf.Clamp (angle, min, max);
	}

}

