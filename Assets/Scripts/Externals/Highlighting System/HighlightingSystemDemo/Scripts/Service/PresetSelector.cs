using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;

public class PresetSelector : MonoBehaviour
{
	protected struct Preset
	{
		public string name;
		public int downsampleFactor;
		public int iterations;
		public float blurMinSpread;
		public float blurSpread;
		public float blurIntensity;
	}

	List<Preset> presets = new List<Preset>()
	{
		new Preset() { name = "Default",	downsampleFactor = 4,	iterations = 2,	blurMinSpread = 0.65f,	blurSpread = 0.25f, blurIntensity = 0.3f }, 
		new Preset() { name = "Strong",		downsampleFactor = 4,	iterations = 2,	blurMinSpread = 0.5f,	blurSpread = 0.15f,	blurIntensity = 0.325f }, 
		new Preset() { name = "Wide",		downsampleFactor = 4,	iterations = 4,	blurMinSpread = 0.5f,	blurSpread = 0.15f,	blurIntensity = 0.325f }, 
		new Preset() { name = "Speed",		downsampleFactor = 4,	iterations = 1,	blurMinSpread = 0.75f,	blurSpread = 0f,	blurIntensity = 0.35f }, 
		new Preset() { name = "Quality",	downsampleFactor = 2,	iterations = 3,	blurMinSpread = 0.5f,	blurSpread = 0.5f,	blurIntensity = 0.28f }, 
		new Preset() { name = "Solid 1px",	downsampleFactor = 1,	iterations = 1,	blurMinSpread = 1f,		blurSpread = 0f,	blurIntensity = 1f }, 
		new Preset() { name = "Solid 2px",	downsampleFactor = 1,	iterations = 2,	blurMinSpread = 1f,		blurSpread = 0f,	blurIntensity = 1f }
	};

	private int h = 30;

	// 
	void OnGUI()
	{
		float ox = Screen.width - 140f;
		float oy = 10f;

		GUI.Label(new Rect(ox, oy, 500, 100), "Highlighting Preset:");
		
		for (int i = 0; i < presets.Count; i++)
		{
			Preset p = presets[i];

			if (GUI.Button(new Rect(ox, oy + 20f + i * h, 120, 20), p.name))
			{
				SetPresetSettings(p);
			}
		}
	}

	// 
	void SetPresetSettings(Preset p)
	{
		HighlightingBase hb = FindObjectOfType<HighlightingBase>();

		if (hb == null) { return; }

		hb.downsampleFactor = p.downsampleFactor;
		hb.iterations = p.iterations;
		hb.blurMinSpread = p.blurMinSpread;
		hb.blurSpread = p.blurSpread;
		hb.blurIntensity = p.blurIntensity;
	}
}
