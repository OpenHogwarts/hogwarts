using UnityEngine;
using System.Collections;

public class NPC : MonoBehaviour {

	public static Color COLOR_SELECTED = Color.green;
	public static Color COLOR_NORMAL = Color.white;
	public static Color COLOR_ENEMY = Color.red;

	TextMesh namePlate;

	public void Start () {
		namePlate = transform.FindChild ("NamePlate").GetComponent<TextMesh> ();
	}

	public void setNamePlate (string name, Color color) {
		namePlate.text = name;
		namePlate.color = color;
	}

	public void setSelected () {
		namePlate.color = COLOR_SELECTED;
	}
}
