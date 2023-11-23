#if UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 || UNITY_5_4_OR_NEWER
#define UNITY_MIN_5_3
#endif

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;
using UnityEditor.SceneManagement;
using ExitGames.Client.Photon;

[InitializeOnLoad]
public class PhotonViewHandler : EditorWindow
{
    private static bool CheckSceneForStuckHandlers = true;

    static PhotonViewHandler()
    {
        // hierarchyWindowChanged is called on hierarchy changed and on save. It's even called when hierarchy-window is closed and if a prefab with instances is changed.
        // this is not called when you edit a instance's value but: on save
		#if (UNITY_2018 || UNITY_2018_1_OR_NEWER)
		EditorApplication.hierarchyChanged += HierarchyChange;
		#else
		EditorApplication.hierarchyWindowChanged += HierarchyChange;
		#endif
    }

    // this method corrects the IDs for photonviews in the scene and in prefabs
    // make sure prefabs always use viewID 0
    // make sure instances never use a owner
    // this is a editor class that should only run if not playing
    internal static void HierarchyChange()
    {
        if (Application.isPlaying)
        {
            //Debug.Log("HierarchyChange ignored, while running.");
            CheckSceneForStuckHandlers = true;  // done once AFTER play mode.
            return;
        }

        if (CheckSceneForStuckHandlers)
        {
            CheckSceneForStuckHandlers = false;
            PhotonNetwork.InternalCleanPhotonMonoFromSceneIfStuck();
        }

        HashSet<PhotonView> pvInstances = new HashSet<PhotonView>();
        HashSet<int> usedInstanceViewNumbers = new HashSet<int>();
        bool fixedSomeId = false;

        //// the following code would be an option if we only checked scene objects (but we can check all PVs)
        //PhotonView[] pvObjects = GameObject.FindSceneObjectsOfType(typeof(PhotonView)) as PhotonView[];
        //Debug.Log("HierarchyChange. PV Count: " + pvObjects.Length);

        string levelName = SceneManagerHelper.ActiveSceneName;
        #if UNITY_EDITOR
        levelName = SceneManagerHelper.EditorActiveSceneName;
        #endif
        int minViewIdInThisScene = PunSceneSettings.MinViewIdForScene(levelName);
        //Debug.Log("Level '" + Application.loadedLevelName + "' has a minimum ViewId of: " + minViewIdInThisScene);

        PhotonView[] pvObjects = Resources.FindObjectsOfTypeAll(typeof(PhotonView)) as PhotonView[];

        foreach (PhotonView view in pvObjects)
        {
            // first pass: fix prefabs to viewID 0 if they got a view number assigned (cause they should not have one!)
			if (PhotonEditorUtils.IsPrefab(view.gameObject))
            {
                if (view.viewID != 0 || view.prefixBackup != -1 || view.instantiationId != -1)
                {
					#if !UNITY_2018_3_OR_NEWER
                    	Debug.LogWarning("PhotonView on persistent object being fixed (id and prefix must be 0). Was: " + view);
					#endif
                    view.viewID = 0;
                    view.prefixBackup = -1;
                    view.instantiationId = -1;
                    EditorUtility.SetDirty(view);   // even in Unity 5.3+ it's OK to SetDirty() for non-scene objects. 
                    fixedSomeId = true;
                }
            }
            else
            {
                // keep all scene-instanced PVs for later re-check
                pvInstances.Add(view);
            }
        }

        Dictionary<GameObject, int> idPerObject = new Dictionary<GameObject, int>();

        // second pass: check all used-in-scene viewIDs for duplicate viewIDs (only checking anything non-prefab)
        // scene-PVs must have user == 0 (scene/room) and a subId != 0
        foreach (PhotonView view in pvInstances)
        {
            if (view.ownerId > 0)
            {
                Debug.Log("Re-Setting Owner ID of: " + view);
            }
            view.ownerId = 0;   // simply make sure no owner is set (cause room always uses 0)
            view.prefix = -1;   // TODO: prefix could be settable via inspector per scene?!

            if (view.viewID != 0)
            {
                if (view.viewID < minViewIdInThisScene || usedInstanceViewNumbers.Contains(view.viewID))
                {
                    view.viewID = 0; // avoid duplicates and negative values by assigning 0 as (temporary) number to be fixed in next pass
                }
                else
                {
                    usedInstanceViewNumbers.Add(view.viewID); // builds a list of currently used viewIDs

                    int instId = 0;
                    if (idPerObject.TryGetValue(view.gameObject, out instId))
                    {
                        view.instantiationId = instId;
                    }
                    else
                    {
                        view.instantiationId = view.viewID;
                        idPerObject[view.gameObject] = view.instantiationId;
                    }
                }
            }

        }

        // third pass: anything that's now 0 must get a new (not yet used) ID (starting at 0)
        int lastUsedId = (minViewIdInThisScene > 0) ? minViewIdInThisScene - 1 : 0;

        foreach (PhotonView view in pvInstances)
        {
            if (view.viewID == 0)
            {
                Undo.RecordObject(view, "Automatic viewID change for: "+view.gameObject.name);

                // Debug.LogWarning("setting scene ID: " + view.gameObject.name + " ID: " + view.subId.ID + " scene ID: " + view.GetSceneID() + " IsPersistent: " + EditorUtility.IsPersistent(view.gameObject) + " IsSceneViewIDFree: " + IsSceneViewIDFree(view.subId.ID, view));
                int nextViewId = PhotonViewHandler.GetID(lastUsedId, usedInstanceViewNumbers);

                view.viewID = nextViewId;

                int instId = 0;
                if (idPerObject.TryGetValue(view.gameObject, out instId))
                {
                    view.instantiationId = instId;
                }
                else
                {
                    view.instantiationId = view.viewID;
                    idPerObject[view.gameObject] = nextViewId;
                }

                lastUsedId = nextViewId;
                fixedSomeId = true;

                #if !UNITY_MIN_5_3
                EditorUtility.SetDirty(view);
                #endif
            }
        }


        if (fixedSomeId)
        {
            //Debug.LogWarning("Some subId was adjusted."); // this log is only interesting for Exit Games
        }
    }

    // TODO fail if no ID was available anymore
    // TODO look up lower numbers if offset hits max?!
    public static int GetID(int idOffset, HashSet<int> usedInstanceViewNumbers)
    {
        while (idOffset < PhotonNetwork.MAX_VIEW_IDS)
        {
            idOffset++;
            if (!usedInstanceViewNumbers.Contains(idOffset))
            {
                break;
            }
        }

        return idOffset;
    }

    //TODO: check if this can be internal protected (as source in editor AND as dll)
    public static void LoadAllScenesToFix()
    {
        string[] scenes = System.IO.Directory.GetFiles(".", "*.unity", SearchOption.AllDirectories);

        foreach (string scene in scenes)
        {
            EditorSceneManager.OpenScene(scene);
            PhotonViewHandler.HierarchyChange();//NOTE: most likely on load also triggers a hierarchy change
            EditorSceneManager.SaveOpenScenes();
        }

        Debug.Log("Corrected scene views where needed.");
    }
}
