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
	public GameObject questFlag;
    public GameObject questPrefab;
    public GameObject taskPrefab;
    public GameObject activeQuestsContainer;
    public CastingPanel castingPanel;

    public enum BarType
    {
        Health = 1,
        Mana = 2,
        Exp = 3
    }

    void Start ()
    {
		Instance = this;

        try {
            showActiveQuests();
        } catch (System.Exception) {
        }
    }

	public void showTargetPanel (Transform target) {
		targetPanel.gameObject.SetActive(true);
		targetPanel.init(target);
	}

	public void hideTargetPanel () {
		targetPanel.gameObject.SetActive(false);
	}

    public void showWonXP (int amount)
    {
        Vector3 pos;
        GameObject inst = Instantiate(temporalTextPrefab) as GameObject;
        inst.transform.SetParent(transform);

        inst.GetComponent<TemporalText>().setText("+" + amount, Color.yellow, TextAnchor.UpperCenter, 1);

        inst.transform.localScale = transform.localScale;

        pos = manaBar.transform.position;
        pos.y += 100;
        inst.transform.position = pos;
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

    public void showActiveQuests()
    {
        GameObject questContainer;
        GameObject taskInst;

        foreach (Quest quest in QuestManager.Instance.quests.Values)
        {
            questContainer = Instantiate(questPrefab) as GameObject;
            questContainer.transform.SetParent(activeQuestsContainer.transform);
            quest.ui = questContainer.GetComponent<Text>();
            quest.ui.text = quest.name;
			taskInst = Instantiate (questFlag) as GameObject;
			taskInst.transform.SetParent(questContainer.transform);
			taskInst.GetComponentInChildren<Text> ().text = quest.name;
			taskInst.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (0, -20, 0);

            foreach (Task task in quest.tasks.Values)
            {
                taskInst = Instantiate(taskPrefab) as GameObject;
                taskInst.transform.SetParent(questContainer.transform);
                task.ui = taskInst.GetComponent<TaskUI>();

                task.ui.setPhrase(task.phrase);
                task.ui.setStatus(task.isCompleted);
            }
        }
    }
}
