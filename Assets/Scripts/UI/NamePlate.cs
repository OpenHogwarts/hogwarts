using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NamePlate : MonoBehaviour {

	public Text Name;
	public Text level;
	public Image health;
	public Text damage;
	public Text minorDamage;
    private float lastHitTime;
    private float lastMinorHitTime;
    private bool checkHideDamageStarted;

    public static Color COLOR_SELECTED = Color.green;
	public static Color COLOR_NORMAL = Color.white;
	public static Color COLOR_ENEMY = Color.red;

    const float SHOW_DAMAGE_DURATION = 4f; // seconds

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
        int rand = Random.Range(0, 2);
        TextAnchor alignment;

        // make it look more dynamic
        switch (rand)
        {
            case 0:
                alignment = TextAnchor.MiddleLeft;
                break;
            case 1:
                alignment = TextAnchor.MiddleCenter;
                break;
            case 2:
            default:
                alignment = TextAnchor.MiddleRight;
                break;
        }

        if (isDPS) {
            minorDamage.text = amount.ToString();
            minorDamage.alignment = alignment;
            lastMinorHitTime = Time.time;
        } else {
            damage.text = amount.ToString();
            damage.alignment = alignment;
            lastHitTime = Time.time;
        }

        if (!checkHideDamageStarted) {
            StartCoroutine(checkHideDamage());
        }
    }

    private IEnumerator checkHideDamage()
    {
        checkHideDamageStarted = true;

        while (Time.time < (lastHitTime + SHOW_DAMAGE_DURATION)) {
            yield return new WaitForSeconds(SHOW_DAMAGE_DURATION);
        }

        damage.text = "";

        while (Time.time < (lastMinorHitTime + SHOW_DAMAGE_DURATION)) {
            yield return new WaitForSeconds(SHOW_DAMAGE_DURATION);
        }

        minorDamage.text = "";
        checkHideDamageStarted = false;
    }
}