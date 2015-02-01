using UnityEngine;
using System.Collections;

public class NamePlate : MonoBehaviour {

	public TextMesh Name;
	public TextMesh level;
	public Transform health;

	public static Color COLOR_SELECTED = Color.green;
	public static Color COLOR_NORMAL = Color.white;
	public static Color COLOR_ENEMY = Color.red;
	
	void Update () {
		transform.LookAt (2 * transform.position - Camera.main.transform.position);
	}

	public void setName (string name, Color color) {
		Name.text = name;
		Name.color = color;
	}

	public void setLevel (int level) {
		this.level.text = level.ToString();
	}
}