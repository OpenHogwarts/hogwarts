using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TargetPanel : MonoBehaviour {

	public new Text name;
	public Image health;
	public Transform target;
	
	private bool isNPC = false;
	private NPC npc;
	private Player player;

	public void init (Transform newTarget) {
		target = newTarget;

		if (target.GetComponent<NPC>() != null) {
			isNPC = true;
			npc = target.GetComponent<NPC>();
		} else {
			isNPC = false;
			player = target.GetComponent<Player>();
		}

		if (isNPC) {
			name.text = npc.name;

			if (npc.isFriendly) {
				name.color = NamePlate.COLOR_SELECTED;
			} else {
				name.color = NamePlate.COLOR_ENEMY;
			}
		} else {
			name.text = player.name;

			if (player.isFriendly) {
				name.color = NamePlate.COLOR_SELECTED;
			} else {
				name.color = NamePlate.COLOR_ENEMY;
			}
		}

		InvokeRepeating("updateHealth", 1f, 1f);
	}

	public void updateHealth () {
		int currentHealth;
		int maxHealth;

		if (isNPC) {
			currentHealth = npc.health;
			maxHealth = npc.maxHealth;
		} else {
			currentHealth = player.health;
			maxHealth = player.maxHealth;
		}

		health.fillAmount = currentHealth / (float)maxHealth;
	}

	void OnDisable () {
		CancelInvoke("updateHealth");
	}
}