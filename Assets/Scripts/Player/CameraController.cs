using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public Transform cameraTarget;
	private float x = 0.0f;
	private float y = 0.0f;

	private int mouseXSpeedMod = 5;
	private int mouseYSpeedMod = 3;

	public float maxViewDistance = 25;
	public float minViewDistance = 1;
	public int zoomRate = 30;
	private int lerpRate = 5;
	public float distance = 6;
	public float desiredDistance;
	public float correctedDistance;
	public float currentDistance;
	private float oldDistance;
	private bool isHitting = false;
	public float lastDistance;
	private bool reachedDist = true;

	public float cameraTargetHeight = 1.0f;

	void Start (){
		Vector3 angles = transform.eulerAngles;
		x = angles.x;
		y = angles.y;
		distance = PlayerPrefs.GetFloat("CameraDistance", distance);
		currentDistance = distance;
		desiredDistance = distance;
		correctedDistance = distance;
	}

	void LateUpdate () {

		if (GamePanel.isMovingAPanel) {
			return;
		}

		if (Input.GetMouseButton (1)) {
			x += Input.GetAxis ("Mouse X") * mouseXSpeedMod;
			y -= Input.GetAxis ("Mouse Y") * mouseYSpeedMod;
		}

		else if(!Player.Instance.isFlying && (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)) {
			float targetRotationAngle = cameraTarget.eulerAngles.y;
			float cameraRotationAngle = transform.eulerAngles.y;
			x = Mathf.LerpAngle(cameraRotationAngle, targetRotationAngle, lerpRate * Time.deltaTime);
		}

		y = ClampAngle (y, -50, 80);

		Quaternion rotation = Quaternion.Euler (y, x, 0);
		
		desiredDistance -= Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs (desiredDistance);
		desiredDistance = Mathf.Clamp (desiredDistance, minViewDistance, maxViewDistance);
		correctedDistance = desiredDistance;
		currentDistance = correctedDistance;
		
		Vector3 position = cameraTarget.position - (rotation * Vector3.forward * desiredDistance);

		position = cameraTarget.position - (rotation * Vector3.forward * currentDistance + new Vector3 (0, -cameraTargetHeight, 0));

		transform.rotation = rotation;
		transform.position = position;

		if (isHitting) {
			desiredDistance -= 0.01f * Time.deltaTime * zoomRate * Mathf.Abs (desiredDistance);
			desiredDistance = Mathf.Clamp (desiredDistance, minViewDistance, maxViewDistance);
		} else {
			Debug.DrawLine(transform.position - (transform.forward * 0.5f), transform.position - (transform.forward * (lastDistance-desiredDistance)));
			if (desiredDistance < lastDistance) {
				if ((!Physics.Raycast (transform.position - (transform.forward * 0.5f), -transform.forward, (lastDistance - desiredDistance)))&&(!Physics.Raycast(transform.position - (transform.forward * ((lastDistance - desiredDistance)+0.5f)), Vector3.down, 0.5f)))  {
					desiredDistance += 0.01f * Time.deltaTime * zoomRate * Mathf.Abs (desiredDistance);
					desiredDistance = Mathf.Clamp (desiredDistance, minViewDistance, maxViewDistance);
				}
			} else {
				reachedDist = true;
			}
		}
	}

	// set play camera preferences before quit
	void OnApplicationQuit() 
	{
		PlayerPrefs.SetFloat("CameraDistance", currentDistance);
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

	void OnTriggerEnter(){
		isHitting = true;
		if (reachedDist) {
			lastDistance = currentDistance;
			reachedDist = false;
		}
	}

	void OnTriggerStay(){
		isHitting = true;
	}

	void OnTriggerExit(){
		isHitting = false;
	}

}
