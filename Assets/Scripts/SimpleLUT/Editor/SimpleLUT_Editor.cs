using System;
using System.Collections;

using UnityEngine;
using UnityEditor;

namespace DigitalRuby.SimpleLUT
{
    [CustomEditor(typeof(SimpleLUT))]
    public class SimpleLUT_Editor : Editor
    {
        private Texture2D logo;

        public override void OnInspectorGUI()
        {
            if (logo == null)
            {
                logo = AssetDatabase.LoadMainAssetAtPath("Assets/SimpleLUT/Editor/SimpleLUTLogo.png") as Texture2D;
            }
            if (logo != null)
            {
                const float maxLogoWidth = 430.0f;
                EditorGUILayout.Separator();
                float w = EditorGUIUtility.currentViewWidth;
                Rect r = new Rect();
                r.width = Math.Min(w - 40.0f, maxLogoWidth);
                r.height = r.width / 4.886f;
                Rect r2 = GUILayoutUtility.GetRect(r.width, r.height);
                r.x = r2.x;
                r.y = r2.y;
                GUI.DrawTexture(r, logo, ScaleMode.ScaleToFit);
                if (GUI.Button(r, "", new GUIStyle()))
                {
                    Application.OpenURL("http://www.digitalruby.com/unity-plugins/");
                }
                EditorGUILayout.Separator();
            }
            
            DrawDefaultInspector();
        }
    }
}
