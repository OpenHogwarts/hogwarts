using UnityEngine;
using System;
using System.Collections;

public class Seller : NPC {

	public int id;
	private string name;

	void Start () {
		if (id == 0) {
			throw new Exception ("Id not assigned");
		}
		base.Start();

		// db call based on seller id
		name = "Lara Blame";
		//---

		setNamePlate (name, NPC.COLOR_NORMAL);
	}

	
	void OnMouseDown() {
		UIMenu.Instance.showPanel ("SellerPanel");
		setSelected ();
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
