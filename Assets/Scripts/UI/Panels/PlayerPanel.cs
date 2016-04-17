using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerPanel : MonoBehaviour {

	public TargetPanel targetPanel;
	public static PlayerPanel Instance;
    public GameObject temporalTextPrefab;
    public Image expBar;
    public Image manaBar;
    public Text healthBarText;

    public enum BarType
    {
        Health = 1,
        Mana = 2,
        Exp = 3
    }

    void Start () {
		Instance = this;
	}

	public void showTargetPanel (Transform target) {
		targetPanel.gameObject.SetActive(true);
		targetPanel.init(target);
	}

	public void hideTargetPanel () {
		targetPanel.gameObject.SetActive(false);
	}

    public void showWonXP (int amount) {
        GameObject inst = Instantiate(temporalTextPrefab) as GameObject;
        inst.transform.parent = transform;

    }

    public void updateBar (BarType type, int current, int max)
    {
        // prevent weird bugs in UI
        if (current > max) {
            current = max;
        }
        float fillAmount = current / (float)max;

        switch (type) {
            case BarType.Health:
                healthBarText.text = current.ToString();
                break;
            case BarType.Mana:
                manaBar.fillAmount = fillAmount;
                break;
            case BarType.Exp:
                expBar.fillAmount = fillAmount;
                break;
        }
    }
}
