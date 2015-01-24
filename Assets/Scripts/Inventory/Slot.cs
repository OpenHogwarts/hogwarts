using UnityEngine;
using System.Collections;

public class Slot : MonoBehaviour {

	public bool available = true;
	public int num;
	public slotType type;
	public CharacterItem.equipmentPosition subType;

	public enum slotType {
		inventory = 1,
		equipment = 2,
	}
}
