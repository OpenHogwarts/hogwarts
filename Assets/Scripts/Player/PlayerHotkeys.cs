using UnityEngine;
using System.Collections;

public class PlayerHotkeys : MonoBehaviour
{	
	void Update () {
		if (Input.GetKey (KeyCode.F) && Player.Instance.isFlying) {
			Broomstick.Instance.leave();
		}
	}
}
