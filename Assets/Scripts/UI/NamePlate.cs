using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NamePlate : MonoBehaviour {

	public Text Name;
	public Text level;
	public Image health;
	public Text damage;
    private float lastHitTime;
    private bool checkHideDamageStarted;

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
    public void setDamage (int amount) {
        int rand = Random.Range(0, 2);
        damage.text = amount.ToString();
        lastHitTime = Time.time;

		// make it look more dynamic
        switch (rand) {
            case 0:
                damage.alignment = TextAnchor.MiddleLeft;
                break;
            case 1:
                damage.alignment = TextAnchor.MiddleCenter;
                break;
            case 2:
                damage.alignment = TextAnchor.MiddleRight;
                break;
        }

        if (!checkHideDamageStarted) {
            StartCoroutine(checkHideDamage());
        }
    }

    private IEnumerator checkHideDamage()
    {
        checkHideDamageStarted = true;

        while (Time.time < (lastHitTime + 4)) {

            yield return new WaitForSeconds(4f);
        }

        damage.text = "";
        checkHideDamageStarted = false;
    }
}