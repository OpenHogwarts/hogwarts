using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityStandardAssets.CinematicEffects
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Bloom))]
    public class BloomEditor : Editor
    {
        [NonSerialized]
        private List<SerializedProperty> m_Properties = new List<SerializedProperty>();

        void OnEnable()
        {
            var settings = FieldFinder<Bloom>.GetField(x => x.settings);
            foreach (var setting in settings.FieldType.GetFields())
            {
                var prop = settings.Name + "." + setting.Name;
                m_Properties.Add(serializedObject.FindProperty(prop));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            foreach (var property in m_Properties)
                EditorGUILayout.PropertyField(property);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
