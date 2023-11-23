using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UnityStandardAssets.CrossPlatformInput
{
    [ExecuteInEditMode]
    public class MobileControlRig : MonoBehaviour
    {
        // this script enables or disables the child objects of a control rig
        // depending on whether the USE_MOBILE_INPUT define is declared.

        // This define is set or unset by a menu item that is included with
        // the Cross Platform Input package.

#if !UNITY_EDITOR
	void OnEnable()
	{
		CheckEnableControlRig();
	}
#endif

        private void Start()
        {
#if UNITY_EDITOR
            if (Application
                .isPlaying) //if in the editor, need to check if we are playing, as start is also called just after exiting play
#endif
            {
                var system = FindObjectOfType<EventSystem>();

                if (system == null)
                {
                    //the scene have no event system, spawn one
                    var o = new GameObject("EventSystem");

                    o.AddComponent<EventSystem>();
                    o.AddComponent<StandaloneInputModule>();
                }
            }
        }

#if UNITY_EDITOR

        private void OnEnable()
        {
            EditorUserBuildSettings.activeBuildTargetChanged += Update;
            EditorApplication.update += Update;
        }


        private void OnDisable()
        {
            EditorUserBuildSettings.activeBuildTargetChanged -= Update;
            EditorApplication.update -= Update;
        }


        private void Update()
        {
            CheckEnableControlRig();
        }
#endif


        private void CheckEnableControlRig()
        {
#if MOBILE_INPUT
		EnableControlRig(true);
#else
            EnableControlRig(false);
#endif
        }


        private void EnableControlRig(bool enabled)
        {
            foreach (Transform t in transform) t.gameObject.SetActive(enabled);
        }
    }
}