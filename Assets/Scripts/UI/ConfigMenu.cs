using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.CinematicEffects;

public class ConfigMenu : MonoBehaviour {

	public GameObject menu;
	public Dropdown qdrop;
	public Toggle ssao;
	public Toggle dof;
	public Toggle performance;
	public GameObject rightbar;
	public GameObject perftext;


	void LoadCfg(){
		ssao.isOn = Camera.main.GetComponent<SESSAO> ().enabled;
		dof.isOn = Camera.main.GetComponent<DepthOfField> ().enabled;
		qdrop.value = QualitySettings.GetQualityLevel ();
	}

	public void ConfigShow(){
		menu.SetActive(!menu.activeSelf);
		LoadCfg ();
	}

	public void ConfigDisconnect(){
		PhotonNetwork.Disconnect ();
		Application.LoadLevel ("MainMenu");
	}

	public void ConfigQuit(){
		Application.Quit ();
	}

	public void OptionsSetQuality(){
		QualitySettings.SetQualityLevel (qdrop.value);
	}

	public void OptionsSetSSAO(){
		Camera.main.GetComponent<SESSAO> ().enabled = ssao.isOn;
	}

	public void OptionsSetDOF(){
		Camera.main.GetComponent<DepthOfField> ().enabled = dof.isOn;
	}

	public void OptionsRightBar(){
		//rightbar.SetActive ();
	}

	public void OptionsPerformance(){
		perftext.SetActive (performance.isOn);
	}
}
