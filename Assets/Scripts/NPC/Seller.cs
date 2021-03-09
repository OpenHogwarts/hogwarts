using UnityEngine;

public class Seller : NPC {
	
	public override void OnClick() {
        Menu.Instance.showPanel ("SellerPanel", false);
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
