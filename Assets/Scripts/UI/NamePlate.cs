using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NamePlate : MonoBehaviour {

	public Text Name;
	public Text level;
	public Image health;
	public GameObject dmgPrefab;
	public Transform dmgParent;

    public static Color COLOR_SELECTED = Color.green;
	public static Color COLOR_NORMAL = Color.white;
	public static Color COLOR_ENEMY = Color.red;

	void Start() {
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
    public void setDamage (int amount, bool isDPS = false)
    {
        if (isDPS) {
			GameObject inst = Instantiate(dmgPrefab) as GameObject;
			inst.transform.parent = dmgParent.transform;
			inst.GetComponent<TemporalText>().setText("-"+amount, Color.blue, TextAnchor.UpperRight, 2);
			inst.transform.localScale = inst.transform.parent.transform.localScale;
			inst.transform.localPosition = inst.transform.parent.localPosition;
        } else {
			GameObject inst2 = Instantiate(dmgPrefab) as GameObject;
			inst2.transform.parent = dmgParent.transform;
			inst2.GetComponent<TemporalText>().setText("-"+amount, Color.white, TextAnchor.UpperCenter, 1);
			inst2.transform.localScale = inst2.transform.parent.transform.localScale;
			inst2.transform.localPosition = inst2.transform.parent.localPosition;
        }
    }
}