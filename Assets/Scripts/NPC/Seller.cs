using UnityEngine;
using System;
using System.Collections;

public class Seller : NPC {
	
	new public void OnMouseDown() {
        var distanceToTarget = Vector3.Distance(transform.position, GameObject.Find("Player(Clone)").transform.position);
        if (distanceToTarget >= 5.0f) {
            // Alert User Somehow?
            setSelected(true);
            Debug.Log("[User Cannot See This] Target Too Far Away To Interact With.");
            return;
        }
        Menu.Instance.showPanel ("SellerPanel", false);
		base.OnMouseDown();
	}

	/**
	 * Sells an item to player
	 *
	 * @param int id item id
	 * @return void
	 */
	public void sellItem (int id) {
		Item item = Item.get (id);

		// looks like he cant pay
		if (item.price > Player.Instance.money) {
			return;
		}

		Player.Instance.money -= item.price;
		Player.Instance.addItem (id);
	}
}
