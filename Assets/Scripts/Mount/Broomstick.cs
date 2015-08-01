using UnityEngine;
using System.Collections;

public class Broomstick : MonoBehaviour
{
	public Transform exitPos;
	public Transform playerPos;
	public static Broomstick Instance;
	public BroomstickController controller;
	public GameObject camera;

	private float speed = 0.5f;
	private Player driver;
	private bool inUse {
		get {
			if (driver == null) {
				return false;
			}
			return true;
		}
	}

	// When a player collides, he joins the bromstick
	public void OnCollisionEnter (Collision collision)
	{
		Player player = collision.transform.GetComponent<Player>();

		if (inUse || player == null || !player.isMine) {
			return;
		}

		// freeze player
		driver = player;
		driver.transform.eulerAngles = new Vector3(0, 0, 0);
		driver.transform.SetParent(transform);
		driver.freeze();
		driver.GetComponent<Rigidbody>().useGravity = false;
		driver.GetComponent<Rigidbody>().isKinematic = true;
		driver.GetComponent<Collider>().enabled = false;
		driver.isFlying = true;
		driver.transform.position = playerPos.position;
		player.transform.FindChild ("Main Camera").gameObject.SetActive(false);

		controller.enabled = true;

		camera.SetActive(true);

		Instance = this;
	}

	public void leave ()
	{
		// unfreeze player
		Vector3 currentRot = driver.transform.rotation.eulerAngles;
		driver.transform.SetParent(null);
		driver.transform.position = exitPos.position;
		driver.transform.eulerAngles = new Vector3(0, currentRot.y, 0);
		driver.unfreeze();
		driver.GetComponent<Rigidbody>().useGravity = true;
		driver.GetComponent<Rigidbody>().isKinematic = false;
		driver.GetComponent<Collider>().enabled = true;
		driver.isFlying = false;
		driver.transform.FindChild ("Main Camera").gameObject.SetActive(true);
		driver = null;

		controller.enabled = false;

		camera.SetActive(false);
	}
}
