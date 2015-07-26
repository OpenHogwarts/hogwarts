using UnityEngine;
using System.Collections;

public class PlayerPanel : MonoBehaviour {

	public TargetPanel targetPanel;
	public static PlayerPanel Instance;

	// Use this for initialization
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
}
