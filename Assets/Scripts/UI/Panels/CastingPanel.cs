using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CastingPanel : MonoBehaviour {

	public static CastingPanel Instance;
	public Text name;
	public Image bar;
	private float skillTime = 10f;
	private float curTime = 10f;
	public bool isCasting = false;

	// Use this for initialization
	void Start () {
		Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
			curTime -= Time.deltaTime;
			bar.fillAmount = curTime / skillTime;
		if (curTime / skillTime <= 0) {
			gameObject.SetActive (false);
			isCasting = false;
		} else {
			isCasting = true;
		}
	}

	public void Cast(string n, float t){
		gameObject.SetActive (true);
		name.text = n;
		skillTime = t;
		curTime = t;
	}
}
