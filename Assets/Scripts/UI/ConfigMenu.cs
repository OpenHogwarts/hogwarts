using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CinematicEffects;
using UnityEngine.SceneManagement;
using System;

public class ConfigMenu : MonoBehaviour {

	public GameObject menu;
	public Dropdown qdrop;
	public Toggle ssao;
	public Toggle dof;
	public Toggle bloom;
	public Toggle performance;
	public Toggle map;
	public GameObject rightbar;
	public GameObject perftext;
	public GameObject[] panel;
	public GameObject player;
	public GameObject dev;
	public GameObject lightSlider;


	void LoadCfg(){
		//ssao.isOn = Camera.main.GetComponent<SESSAO> ().enabled;
        try {
            dof.isOn = Camera.main.GetComponent<DepthOfField>().enabled;
        } catch(Exception e) { }
		
		qdrop.value = QualitySettings.GetQualityLevel ();
	}

	public void ConfigShow(){
		menu.SetActive(!menu.activeSelf);
		LoadCfg ();
	}

	public void ConfigDisconnect(){
		PhotonNetwork.Disconnect ();
		SceneManager.LoadScene("MainMenu");
	}

	public void ConfigQuit(){
		Application.Quit ();
	}

	public void OptionsSetQuality(){
		QualitySettings.SetQualityLevel (qdrop.value);
	}

	public void OptionsSetSSAO(){
		//Camera.main.GetComponent<SESSAO> ().enabled = ssao.isOn;
	}

	public void OptionsSetDOF(){
		Camera.main.GetComponent<DepthOfField> ().enabled = dof.isOn;
	}

	public void OptionsSetBloom(){
		Camera.main.GetComponent<Bloom> ().enabled = bloom.isOn;
	}

	public void OptionsRightBar(){
		//rightbar.SetActive ();
	}

	public void OptionsPerformance(){
		perftext.SetActive (performance.isOn);
	}

	public void MapPos(){
		if (map.isOn) {
			GameObject.Find ("Canvas/MiniMap").GetComponent<RectTransform> ().anchoredPosition = new Vector3 (-6, 10, 1);
		} else {
			GameObject.Find ("Canvas/MiniMap").GetComponent<RectTransform> ().anchoredPosition = new Vector3 (-6, 66, 1);
		}
	}

	public void ShowPanel(int p){
		for (int i = 0; i < panel.Length; i++) {
			panel [i].SetActive (false);
		}
		panel [p].SetActive (true);
	}

	public void Respawn(){
		player.GetComponent<Player> ().Respawn ();
	}

	public void AddHP(int n){
		Player.Instance.health += n;
	}

    public void updateDaytime () {
        try {
            GameObject.Find("Lights").GetComponent<NightSlider>().slider = lightSlider.GetComponent<Slider>().value;
        } catch (System.Exception) {
        }
    }
}
