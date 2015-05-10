using UnityEngine;
using System;
using System.Collections;

public class Seller : NPC {
	
	new public void OnMouseDown() {
		UIMenu.Instance.showPanel ("SellerPanel");
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
