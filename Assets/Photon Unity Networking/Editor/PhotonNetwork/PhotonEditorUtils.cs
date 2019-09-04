// ----------------------------------------------------------------------------
// <copyright file="PhotonEditorUtils.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Unity Editor Utils
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace ExitGames.Client.Photon
{
    [InitializeOnLoad]
	public class PhotonEditorUtils
	{
        /// <summary>True if the ChatClient of the Photon Chat API is available. If so, the editor may (e.g.) show additional options in settings.</summary>
        public static bool HasChat;
        /// <summary>True if the VoiceClient of the Photon Voice API is available. If so, the editor may (e.g.) show additional options in settings.</summary>
        public static bool HasVoice;
        /// <summary>True if the PhotonEditorUtils checked the available products / APIs. If so, the editor may (e.g.) show additional options in settings.</summary>
        public static bool HasCheckedProducts;

	    static PhotonEditorUtils()
	    {
            HasVoice = Type.GetType("ExitGames.Client.Photon.Voice.VoiceClient, Assembly-CSharp") != null || Type.GetType("ExitGames.Client.Photon.Voice.VoiceClient, Assembly-CSharp-firstpass") != null;
            HasChat = Type.GetType("Photon.Chat.ChatClient, Assembly-CSharp") != null || Type.GetType("Photon.Chat.ChatClient, Assembly-CSharp-firstpass") != null;
            PhotonEditorUtils.HasCheckedProducts = true;
	    }


		public static void MountScriptingDefineSymbolToAllTargets(string defineSymbol)
		{
			foreach (BuildTargetGroup _group in Enum.GetValues(typeof(BuildTargetGroup)))
			{
				if (_group == BuildTargetGroup.Unknown) continue;

				List<string> _defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(_group).Split(';').Select(d => d.Trim()).ToList();

				if (!_defineSymbols.Contains(defineSymbol))
				{
					_defineSymbols.Add(defineSymbol);
					PlayerSettings.SetScriptingDefineSymbolsForGroup(_group, string.Join(";", _defineSymbols.ToArray()));
				}
			}
		}

		public static void UnMountScriptingDefineSymbolToAllTargets(string defineSymbol)
		{
			foreach (BuildTargetGroup _group in Enum.GetValues(typeof(BuildTargetGroup)))
			{
				if (_group == BuildTargetGroup.Unknown) continue;

				List<string> _defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(_group).Split(';').Select(d => d.Trim()).ToList();

				if (_defineSymbols.Contains(defineSymbol))
				{
					_defineSymbols.Remove(defineSymbol);
					PlayerSettings.SetScriptingDefineSymbolsForGroup(_group, string.Join(";", _defineSymbols.ToArray()));
				}
			}
		}


		/// <summary>
		/// Gets the parent directory of a path. Recursive Function, will return null if parentName not found
		/// </summary>
		/// <returns>The parent directory</returns>
		/// <param name="path">Path.</param>
		/// <param name="parentName">Parent name.</param>
		public static string GetParent(string path, string parentName)
		{
			var dir = new DirectoryInfo(path);

			if (dir.Parent == null)
			{
				return null;
			}

			if (string.IsNullOrEmpty(parentName))
			{
				return  dir.Parent.FullName;
			}

			if (dir.Parent.Name == parentName)
			{
				return dir.Parent.FullName;
			}

			return GetParent(dir.Parent.FullName, parentName);
		}

		/// <summary>
		/// Check if a GameObject is a prefab asset or part of a prefab asset, as opposed to an instance in the scene hierarchy
		/// </summary>
		/// <returns><c>true</c>, if a prefab asset or part of it, <c>false</c> otherwise.</returns>
		/// <param name="go">The GameObject to check</param>
		public static bool IsPrefab(GameObject go)
		{
            #if UNITY_2018_3_OR_NEWER
            	return UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(go) != null || EditorUtility.IsPersistent(go);
            #else
                return EditorUtility.IsPersistent(go);
			#endif
		}

        //https://forum.unity.com/threads/using-unitywebrequest-in-editor-tools.397466/#post-4485181
        public static void StartCoroutine(System.Collections.IEnumerator update)
        {
            EditorApplication.CallbackFunction closureCallback = null;

            closureCallback = () =>
            {
                try
                {
                    if (update.MoveNext() == false)
                    {
                        EditorApplication.update -= closureCallback;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    EditorApplication.update -= closureCallback;
                }
            };

            EditorApplication.update += closureCallback;
        }

        public static System.Collections.IEnumerator HttpGet(string url, Action<string> successCallback, Action<string> errorCallback)
        {
            using (UnityEngine.Networking.UnityWebRequest w = UnityEngine.Networking.UnityWebRequest.Get(url))
            {
                #if UNITY_2017_2_OR_NEWER
                yield return w.SendWebRequest();
                #else
                yield return w.Send();
                #endif

                while (w.isDone == false)
                    yield return null;

                #if UNITY_2017_1_OR_NEWER
                if (w.isNetworkError || w.isHttpError)
                #else
                if (w.isError)
                #endif
                {
                    if (errorCallback != null)
                    {
                        errorCallback(w.error);
                    }
                }
                else
                {
                    if (successCallback != null)
                    {
                        successCallback(w.downloadHandler.text);
                    }
                }
            }
        }
    }
}
