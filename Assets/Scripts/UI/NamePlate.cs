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
		transform.SetParent(GameObject.Find ("Canvas/Plates").transform);
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
        GameObject inst = Instantiate(dmgPrefab) as GameObject;
        inst.transform.SetParent(dmgParent.transform);

        if (isDPS) {
			inst.GetComponent<TemporalText>().setText("-" + amount, Color.blue, TextAnchor.UpperRight, 2);
        } else {
            inst.GetComponent<TemporalText>().setText("-" + amount, Color.white, TextAnchor.UpperCenter, 1);
        }

        inst.transform.localScale = inst.transform.parent.transform.localScale;
        inst.transform.localPosition = inst.transform.parent.localPosition;
    }
}