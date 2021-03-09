using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.IO;
#endif

public class SceneLoader : MonoBehaviour
{
	public static readonly string[] sceneNames = new string[] {"Welcome", "Colors", "Transparency", "Occlusion", "OccluderModes", "Scripting", "Compound", "Mobile"};

	private int ox = 20;
	private int oy = 100;
	private int h = 30;

	#if UNITY_EDITOR
	private const string scenesPath = "Assets/HighlightingSystemDemo/Scenes/";
	private const string extension = ".unity";

	private List<string> missingSceneNames;
	private List<string> missingScenePaths;

	// 
	void Start()
	{
		// Create list of missing demo scenes
		CheckMissingScenes();

		// In case we have missing demo scenes
		if (missingSceneNames != null && missingSceneNames.Count > 0)
		{
			// Ask user to add missing scenes to the editor build settings
			string message = "Add these demo scenes to the editor build settings?\n";
			int l = missingSceneNames.Count;
			for (int i = 0; i < l; i++)
			{
				message += string.Format(i != l-1 ? "'{0}', " : "'{0}'.", missingSceneNames[i]);
			}
			bool answer = EditorUtility.DisplayDialog("Highlighting System", message, "Yes", "No");
			if (answer)
			{
				AddMissingScenes();

				// Stop Playing to allow changes to take effect
				StartCoroutine(StopNextFrame());
			}
		}
	}

	// 
	void CheckMissingScenes()
	{
		int l = sceneNames.Length;
		missingSceneNames = new List<string>(l);
		missingScenePaths = new List<string>(l);

		// Build list with full scene paths
		for (int i = 0; i < l; i++)
		{
			string sceneName = sceneNames[i];
			missingSceneNames.Add(sceneName);
			missingScenePaths.Add(scenesPath + sceneName + extension);
		}
		
		// Remove existing scenes from the list
		EditorBuildSettingsScene[] existingScenes = EditorBuildSettings.scenes;
		for (int i = missingScenePaths.Count - 1; i >= 0; i--)
		{
			string scenePath = missingScenePaths[i];
			for (int j = 0; j < existingScenes.Length; j++)
			{
				EditorBuildSettingsScene scene = existingScenes[j];
				if (Path.Equals(scene.path, scenePath))
				{
					missingSceneNames.RemoveAt(i);
					missingScenePaths.RemoveAt(i);
					break;
				}
			}
		}
	}

	// 
	void AddMissingScenes()
	{
		EditorBuildSettingsScene[] existingScenes = EditorBuildSettings.scenes;
		int l = existingScenes.Length;

		// Create new extended list of scenes and copy existing ones to it
		EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[l + missingScenePaths.Count];
		existingScenes.CopyTo(newScenes, 0);

		// Add missing scenes
		for (int i = 0; i < missingScenePaths.Count; i++)
		{
			newScenes[l + i] = new EditorBuildSettingsScene(missingScenePaths[i], true);
		}

		// Assign new scene list
		EditorBuildSettings.scenes = newScenes;
	}

	// 
	IEnumerator StopNextFrame()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		
		EditorApplication.isPlaying = false;
	}
	#endif

	// 
	void OnGUI()
	{
		GUI.Label(new Rect(ox, oy + 10, 500, 100), "Load demo scene:");

		for (int i = 0; i < sceneNames.Length; i++)
		{
			string scene = sceneNames[i];

			if (GUI.Button(new Rect(ox, oy + 30 + i * h, 120, 20), scene))
			{
                UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
			}
		}
	}
}