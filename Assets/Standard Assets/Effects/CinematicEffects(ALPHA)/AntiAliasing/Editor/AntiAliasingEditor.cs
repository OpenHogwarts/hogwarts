using System;
using UnityEditor;

namespace UnityStandardAssets.CinematicEffects
{
    [CustomEditor(typeof(AntiAliasing))]
    public class AntiAliasingEditor : Editor
    {
        private string[] methodNames =
        {
            "Subpixel Morphological Anti-aliasing",
            "Fast Approximate Anti-aliasing"
        };

        private int m_SelectedMethod;

        private SMAAEditor m_SMAAEditor = new SMAAEditor();
        private FXAAEditor m_FXAAEditor = new FXAAEditor();

        IAntiAliasingEditor m_AntiAliasingEditor;

        private void OnEnable()
        {
            m_SMAAEditor.OnEnable(serializedObject, "m_SMAA");
            m_FXAAEditor.OnEnable(serializedObject, "m_FXAA");
        }

        public override void OnInspectorGUI()
        {
            var antiAliasingTarget = (AntiAliasing)target;

            m_SelectedMethod = antiAliasingTarget.method;

            EditorGUI.BeginChangeCheck();
            m_SelectedMethod = EditorGUILayout.Popup("Method", m_SelectedMethod, methodNames);

            bool dirty = false;

            if (EditorGUI.EndChangeCheck())
            {
                if (m_SelectedMethod < 0)
                    m_SelectedMethod = 0;
                else if (m_SelectedMethod > 1)
                    m_SelectedMethod = 1;

                antiAliasingTarget.method = m_SelectedMethod;
                dirty = true;
            }

            if (m_SelectedMethod == 0)
                m_AntiAliasingEditor = m_SMAAEditor;
            else
                m_AntiAliasingEditor = m_FXAAEditor;

            dirty |= m_AntiAliasingEditor.OnInspectorGUI(antiAliasingTarget.current);

            if (dirty)
            {
                EditorUtility.SetDirty(antiAliasingTarget);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
