using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ConfigMenu : MonoBehaviour {

	public GameObject menu;
	public GameObject options;
	public Toggle fxaa;
	public Toggle ssao;
	public Toggle dof;
	public Toggle rbt;
	public Toggle performance;
	public GameObject rightbar;
	public GameObject perftext;
	public Slider uiscale;
	public Canvas ui;

	public void ConfigShow(){
		if (menu.GetActive()) {
			menu.SetActive(false);
		} else {
			menu.SetActive(true);
		}
	}

	public void ConfigDisconnect(){
		PhotonNetwork.Disconnect ();
		Application.LoadLevel ("MainMenu");
	}

	public void ConfigOptions(){
		menu.SetActive (false);
		options.SetActive (true);
	}

	public void ConfigQuit(){
		Application.Quit ();
	}

	public void OptionsQuit(){
		options.SetActive (false);
	}

	public void OptionsSetQuality(int q){
		QualitySettings.SetQualityLevel (q);
	}

	public void OptionsSetFXAA(){
		//Camera.main.GetComponent<AntialiasingAsPostEffect> ().enabled = fxaa.isOn;
	}

	public void OptionsSetSSAO(){
		//Camera.main.GetComponent<AmbientObscurance> ().enabled = ssao.isOn;
	}

	public void OptionsSetDOF(){
		//Camera.main.GetComponent<DepthOfFieldScatter> ().enabled = dof.isOn;
	}

	public void OptionsSetUIScale(){
		ui.scaleFactor = uiscale.value;
	}

	public void OptionsRightBar(){
		rightbar.SetActive (rbt.isOn);
	}

	public void OptionsPerformance(){
		perftext.SetActive (performance.isOn);
	}
}
