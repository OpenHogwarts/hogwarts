using UnityEngine;
public class CharacterItem {

	protected string TABLE_NAME = "inventory";

	public int id;
	public int item;
	public int character;
	public int quantity;
	public int slot = 0;

	public void save () {
		if (quantity < 1) {
			delete();
		} else {
			Menu.db.Update (TABLE_NAME, this);
		}
	}
	
	public bool create () {
		if (character == 0) {
			character = (int)PhotonNetwork.player.customProperties["characterId"];
		}
		return Menu.db.Insert (TABLE_NAME, this);
	}

	public void delete () {
		Menu.db.Delete (TABLE_NAME, this.id);
	}
}
