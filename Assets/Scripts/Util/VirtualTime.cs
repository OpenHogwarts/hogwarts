using UnityEngine;
using System.Collections;
using System;

public class VirtualTime : MonoBehaviour {

	void Update () {
		TimeSpan t = TimeSpan.FromSeconds( ((System.DateTime.Now.Minute*60)+System.DateTime.Now.Second)*24 );

		string vTime = string.Format("{0:D2}:{1:D2}", 
			t.Hours, 
			t.Minutes );

		Debug.Log (vTime);
	}
}
