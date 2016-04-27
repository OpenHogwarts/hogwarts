using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace HighlightingSystem
{
	public enum PresetSaveResult
	{
		Null,		// provided name string is null or empty
		Default,	// overwriting default presets is not allowed
		Exists,		// preset with this name already exists
		Success		// preset successfully saved
	}

	public class HighlightingBaseEditor : Editor
	{
		private readonly string editorPrefsKey = "HighlightingSystem.Presets";
		private const int currentVersion = 0;

		protected struct Preset
		{
			public string name;
			public int downsampleFactor;
			public int iterations;
			public float blurMinSpread;
			public float blurSpread;
			public float blurIntensity;
		}

		static protected readonly string[] downsampleOptions = new string[] { "None", "Half", "Quarter" };
		static protected readonly int[] _downsampleGet = new int[] { -1, 0, 1, -1, 2 };		// maps hb.downsampleFactor to the downsampleOptions index
		static protected readonly int[] _downsampleSet = new int[] {     1, 2,     4 };		// maps downsampleOptions index to the hb.downsampleFactor
		static protected readonly List<Preset> defaultPresets = new List<Preset>()
		{
			new Preset() { name = "Default",	downsampleFactor = 4,	iterations = 2,	blurMinSpread = 0.65f,	blurSpread = 0.25f, blurIntensity = 0.3f }, 
			new Preset() { name = "Strong",		downsampleFactor = 4,	iterations = 2,	blurMinSpread = 0.5f,	blurSpread = 0.15f,	blurIntensity = 0.325f }, 
			new Preset() { name = "Speed",		downsampleFactor = 4,	iterations = 1,	blurMinSpread = 0.75f,	blurSpread = 0f,	blurIntensity = 0.35f }, 
			new Preset() { name = "Quality",	downsampleFactor = 2,	iterations = 3,	blurMinSpread = 0.5f,	blurSpread = 0.5f,	blurIntensity = 0.28f }, 
			new Preset() { name = "Solid 1px",	downsampleFactor = 1,	iterations = 1,	blurMinSpread = 1f,		blurSpread = 0f,	blurIntensity = 1f }, 
			new Preset() { name = "Solid 2px",	downsampleFactor = 1,	iterations = 2,	blurMinSpread = 1f,		blurSpread = 0f,	blurIntensity = 1f }
		};
		static protected readonly GUIContent removeButtonContent = new GUIContent("Remove Preset");
		static protected readonly GUIContent removeButtonContentDisabled = new GUIContent("Remove Preset", "Removing default presets is not allowed.");

		protected SavePresetWindow window;

		protected string[] presetNames;
		protected List<Preset> customPresets;

		protected int presetIndex = -1;
		protected List<Preset> presets;
		
		protected HighlightingBase hb;

		// 
		protected virtual void OnEnable()
		{
			hb = target as HighlightingBase;
			hb.CheckInstance();

			LoadPresets();
			presetIndex = FindCurrentPreset();
		}

		// 
		protected void CommonGUI()
		{
			#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
			if (!PlayerSettings.use32BitDisplayBuffer)
			{
				EditorGUILayout.HelpBox("Highlighting System requires 32-bit display buffer. Set the 'Use 32-bit Display Buffer' checkbox under the 'Resolution and Presentation' section of Player Settings.", MessageType.Error);
			}
			#endif

			EditorGUILayout.HelpBox("Depth Offset properties should be used only when Dynamic Batching is enabled in Player Settings. Otherwise set them to 0's to avoid rendering artifacts.", MessageType.Info);
			EditorGUI.BeginChangeCheck();
			hb.offsetFactor = EditorGUILayout.Slider("Depth Offset Factor:", hb.offsetFactor, -1f, 0f);
			hb.offsetUnits = EditorGUILayout.Slider("Depth Offset Units:", hb.offsetUnits, -100f, 0f);
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(hb);
			}

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Preset:");

			int oldIndex = presetIndex;
			int newIndex = EditorGUILayout.Popup(presetIndex, presetNames);
			if (oldIndex != newIndex)
			{
				SetPresetSettings(newIndex);
				presetIndex = newIndex;
			}

			Preset currentPreset = new Preset();
			if (presetIndex >= 0) { currentPreset = presets[presetIndex]; }

			EditorGUI.BeginChangeCheck();

			SetBoldDefaultFont(presetIndex < 0 || hb.downsampleFactor != currentPreset.downsampleFactor);
			hb.downsampleFactor = _downsampleSet[EditorGUILayout.Popup("Downsampling:", _downsampleGet[hb.downsampleFactor], downsampleOptions)];

			SetBoldDefaultFont(presetIndex < 0 || hb.iterations != currentPreset.iterations);
			hb.iterations = Mathf.Clamp(EditorGUILayout.IntField("Iterations:", hb.iterations), 0, 50);

			SetBoldDefaultFont(presetIndex < 0 || hb.blurMinSpread != currentPreset.blurMinSpread);
			hb.blurMinSpread = EditorGUILayout.Slider("Min Spread:", hb.blurMinSpread, 0f, 3f);

			SetBoldDefaultFont(presetIndex < 0 || hb.blurSpread != currentPreset.blurSpread);
			hb.blurSpread = EditorGUILayout.Slider("Spread:", hb.blurSpread, 0f, 3f);

			SetBoldDefaultFont(presetIndex < 0 || hb.blurIntensity != currentPreset.blurIntensity);
			hb.blurIntensity = EditorGUILayout.Slider("Intensity:", hb.blurIntensity, 0f, 1f);

			SetBoldDefaultFont(false);

			if (EditorGUI.EndChangeCheck())
			{
				// If settings have changed when default preset has been selected
				if (presetIndex < defaultPresets.Count)
				{
					presetIndex = -1;
				}
				EditorUtility.SetDirty(hb);
			}

			int customPresetIndex = presetIndex - defaultPresets.Count;

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Save Preset"))
			{
				string defaultName;
				if (customPresetIndex < 0) { defaultName = "My Preset"; }
				else { defaultName = customPresets[customPresetIndex].name; }
				window = SavePresetWindow.Init(defaultName, SavePresetAs);
			}

			if (presetIndex < defaultPresets.Count) { GUI.enabled = false; }
			if (GUILayout.Button(GUI.enabled ? removeButtonContent : removeButtonContentDisabled))
			{
				bool delete = EditorUtility.DisplayDialog("Removing Preset", "Are you sure - you want to remove Preset '" + customPresets[customPresetIndex].name + "'?", "Yes", "No");
				if (delete)
				{
					customPresets.RemoveAt(customPresetIndex);
					SavePresets();
					LoadPresets();
					SetPresetSettings(0);
					presetIndex = 0;
				}
			}
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();
		}

		// 
		protected virtual PresetSaveResult SavePresetAs(string name, bool overwrite)
		{
			window = null;

			if (string.IsNullOrEmpty(name)) { return PresetSaveResult.Null; }

			int customPresetIndex = -1;
			for (int i = 0, imax = presets.Count; i < imax; i++)
			{
				if (presets[i].name == name)
				{
					if (i < defaultPresets.Count)
					{
						// Overwriting default presets is not allowed
						return PresetSaveResult.Default;
					}
					else if (!overwrite)
					{
						// Preset with this name already exists
						return PresetSaveResult.Exists;
					}
					else
					{
						// Overwrite custom preset with this name
						customPresetIndex = i - defaultPresets.Count;
						break;
					}
				}
			}

			Preset p = new Preset();
			p.name = name;
			p.downsampleFactor = hb.downsampleFactor;
			p.iterations = hb.iterations;
			p.blurMinSpread = hb.blurMinSpread;
			p.blurSpread = hb.blurSpread;
			p.blurIntensity = hb.blurIntensity;

			int newIndex = -1;
			if (overwrite && customPresetIndex >= 0)
			{
				customPresets[customPresetIndex] = p;
				newIndex = customPresetIndex + defaultPresets.Count;
			}
			else
			{
				customPresets.Add(p);
				newIndex = customPresets.Count + defaultPresets.Count - 1;
			}

			SavePresets();
			LoadPresets();
			presetIndex = newIndex;
			EditorUtility.SetDirty(hb);

			return PresetSaveResult.Success;
		}

		// Returns current preset index
		protected virtual int FindCurrentPreset()
		{
			for (int i = presets.Count - 1; i >= 0; i--)
			{
				Preset p = presets[i];
				if (hb.downsampleFactor == p.downsampleFactor && 
				    hb.iterations == p.iterations && 
				    hb.blurMinSpread == p.blurMinSpread && 
				    hb.blurSpread == p.blurSpread && 
				    hb.blurIntensity == p.blurIntensity)
				{
					return i;
				}
			}
			return -1;
		}

		// 
		protected virtual void SetPresetSettings(int index)
		{
			if (index < 0 || index >= presets.Count) { return; }

			Preset p = presets[index];
			hb.downsampleFactor = p.downsampleFactor;
			hb.iterations = p.iterations;
			hb.blurMinSpread = p.blurMinSpread;
			hb.blurSpread = p.blurSpread;
			hb.blurIntensity = p.blurIntensity;
		}

		// 
		protected virtual void SavePresets()
		{
			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);

			int l = customPresets.Count;
			Debug.Log("Saving " + l + " custom presets");

			if (l > 0)
			{
				bw.Write(currentVersion);
				bw.Write(l);
				
				for (int i = 0; i < l; i++)
				{
					Preset p = customPresets[i];
					bw.Write(p.name);
					bw.Write(p.downsampleFactor);
					bw.Write(p.iterations);
					bw.Write(p.blurMinSpread);
					bw.Write(p.blurSpread);
					bw.Write(p.blurIntensity);
				}
				
				byte[] bytes = ms.ToArray();
				string result = Convert.ToBase64String(bytes);
				EditorPrefs.SetString(editorPrefsKey, result);
				
				Debug.Log(bytes.Length);
				Debug.Log(result);
			}
			else if (EditorPrefs.HasKey(editorPrefsKey))
			{
				EditorPrefs.DeleteKey(editorPrefsKey);
			}
		}

		// 
		protected virtual void LoadPresets()
		{
			presets = new List<Preset>(defaultPresets);
			
			customPresets = new List<Preset>();
			// Load and parse EditorPrefs presets to customPresets list
			if (EditorPrefs.HasKey(editorPrefsKey))
			{
				string data = EditorPrefs.GetString(editorPrefsKey);

				if (!string.IsNullOrEmpty(data))
				{
					byte[] bytes = Convert.FromBase64String(data);

					MemoryStream ms = new MemoryStream(bytes);
					BinaryReader br = new BinaryReader(ms);

					int loadedVersion = br.ReadInt32();

					if (loadedVersion != currentVersion)
					{
						Debug.LogError("HighlightingSystem : Error reading serialized presets data");
					}
					else
					{
						int l = br.ReadInt32();
						customPresets.Capacity = l;
						for (int i = 0; i < l; i++)
						{
							Preset p = new Preset();
							p.name = br.ReadString();
							p.downsampleFactor = br.ReadInt32();
							p.iterations = br.ReadInt32();
							p.blurMinSpread = br.ReadSingle();
							p.blurSpread = br.ReadSingle();
							p.blurIntensity = br.ReadSingle();
							customPresets.Add(p);
						}
					}
				}
			}
			
			// Combine custom and default presets in one list
			for (int i = 0, imax = customPresets.Count; i < imax; i++)
			{
				presets.Add(customPresets[i]);
			}
			
			// Extract and fill preset names array
			presetNames = new string[presets.Count];
			for (int i = 0; i < presets.Count; i++)
			{
				presetNames[i] = presets[i].name;
			}
		}

		// 
		protected virtual void CloseWindow()
		{
			if (window == null) { return; }

			window.Close();
			window = null;
		}

		private MethodInfo boldFontMethodInfo = null;
		private void SetBoldDefaultFont(bool value)
		{
			if (boldFontMethodInfo == null)
			{
				boldFontMethodInfo = typeof(EditorGUIUtility).GetMethod("SetBoldDefaultFont", BindingFlags.Static | BindingFlags.NonPublic);
			}
			boldFontMethodInfo.Invoke(null, new[] { value as object });
		}
	}

	public class SavePresetWindow : EditorWindow
	{
		static private readonly string presetTextFieldName = "PresetTextFieldName";
		public delegate PresetSaveResult InputResult(string input, bool overwrite);
		private event InputResult callback;
		private string presetName;

		// 
		public static SavePresetWindow Init(string name, InputResult callback)
		{
			Rect rect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 300f, 60f);
			SavePresetWindow window = GetWindowWithRect<SavePresetWindow>(rect, true, "Specify Preset Name", true);
			window.callback = callback;
			window.presetName = name;
			return window;
		}

		// 
		void OnGUI()
		{
			GUI.SetNextControlName(presetTextFieldName);
			presetName = EditorGUILayout.TextField("Preset Name", presetName);

			EditorGUI.FocusTextInControl(presetTextFieldName);

			bool pressed = GUILayout.Button("Save Preset", GUILayout.ExpandHeight(true));
			Event e = Event.current;
			bool submitted = e.type == EventType.KeyUp && GUI.GetNameOfFocusedControl() == presetTextFieldName && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter);

			if (pressed || submitted)
			{
				OnSavePreset();
				GUIUtility.ExitGUI();
			}
		}

		// 
		void OnLostFocus() { Quit(); }
		void OnSelectionChange() { Quit(); }
		void OnProjectChange() { Quit(); }
		
		// 
		private void OnSavePreset()
		{
			presetName = presetName.Trim();
			
			if (string.IsNullOrEmpty(presetName))
			{
				EditorUtility.DisplayDialog("Unable to save Preset", "Please specify valid Preset name.", "Close");
			}
			else
			{
				PresetSaveResult result = callback(presetName, false);
				
				if (result == PresetSaveResult.Success)
				{
					Close();
				}
				else if (result == PresetSaveResult.Default)
				{
					NotifyOverwritingDefault();
				}
				else if (result == PresetSaveResult.Exists)
				{
					bool overwrite = EditorUtility.DisplayDialog("Overwriting Preset", "Preset '" + presetName + "' already exists. Overwrite?", "Yes", "No");
					if (overwrite)
					{
						result = callback(presetName, true);
						
						if (result == PresetSaveResult.Success)
						{
							Close();
						}
						else if (result == PresetSaveResult.Default)
						{
							NotifyOverwritingDefault();
						}
					}
				}
			}
		}
		
		// 
		private void NotifyOverwritingDefault()
		{
			EditorUtility.DisplayDialog("Unable to save Preset", "Overwriting default Presets is not allowed! Please specify another name for your Preset.", "Close");
		}

		// 
		private void Quit()
		{
			callback(string.Empty, false);
			Close();
		}
	}
}