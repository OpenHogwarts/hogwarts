using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NamePlate : MonoBehaviour {

	public Text Name;
	public Text level;
	public Image health;

	public static Color COLOR_SELECTED = Color.green;
	public static Color COLOR_NORMAL = Color.white;
	public static Color COLOR_ENEMY = Color.red;

	void Start(){
		gameObject.name = transform.parent.name;
		transform.parent = GameObject.Find ("Canvas/Plates").transform;
	}

	public void setName (string name, Color color) {
		Name.text = name;
		Name.color = color;
	}

	public void setLevel (int level) {
		this.level.text = level.ToString();
	}
}