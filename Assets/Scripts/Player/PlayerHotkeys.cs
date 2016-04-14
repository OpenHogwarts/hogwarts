using UnityEngine;
using System.Collections;

public class PlayerHotkeys : MonoBehaviour
{	
	public static bool isClickingATarget = false;

	void Update () {

        if (Chat.Instance.isWritting) {
            return;
        }

		if (Input.GetKeyDown (KeyCode.F1)) {
			GameObject.Find ("Canvas/TopMenu/Config").GetComponent<ConfigMenu> ().dev.SetActive (true);
		}
		if (Input.GetKey (KeyCode.F) && Player.Instance.isFlying) {
			Broomstick.Instance.leave();
		}
		if (Input.GetKeyDown (KeyCode.B)) {
			Menu.Instance.togglePanel("BagPanel");
		}
		if (Input.GetKeyDown (KeyCode.C)) {
			Menu.Instance.togglePanel("CharacterPanel");
		}
        if (Input.GetKeyDown(KeyCode.T)) {
            Chat.Instance.input2.ActivateInputField();
        }

        if (Player.Instance.target)
		{
			// unselect target
			if (Input.GetKeyUp(KeyCode.Mouse0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
				if(isClickingATarget) {
					isClickingATarget = false;
				} else {
					Player.Instance.target = null;
				}
			}

			if (Input.GetKeyDown(KeyCode.Alpha1)) {
				PlayerCombat.Instance.spellCast(0);
			} else if (Input.GetKeyDown(KeyCode.Alpha2)) {
				PlayerCombat.Instance.spellCast(1);
			} else if (Input.GetKeyDown(KeyCode.Alpha3)) {
				PlayerCombat.Instance.spellCast(2);
			}
		}
	}
}
