using UnityEngine;
public class CharacterItem {

	protected string TABLE_NAME = "inventory";
	
	public int item;
	public int character;
	public int quantity;
	public int slot = 0;
	public int _position = 0;
	public int attrition = 0;

	public equipmentPosition position {
		get {
			return (equipmentPosition)_position;
		}
		set {
			_position = (int)value;
		}
	}

	public enum equipmentPosition {
		head = 1,
		handLeft = 2,
		handRight = 3,
		hands = 4
	}

	public void save () {
		if (quantity < 1) {
			delete();
		} else {
			Service.db.Update (TABLE_NAME, this);
		}
	}
	
	public bool create () {
		if (character == 0) {
			character = (int)PhotonNetwork.player.customProperties["characterId"];
		}

		return Service.db.Insert (TABLE_NAME, this);
	}

	public void delete () {
		Service.db.Delete (TABLE_NAME, this.item);
	}
}
