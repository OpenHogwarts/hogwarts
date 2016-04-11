using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Reflection;
using UnityEngine.Serialization;

namespace UnityStandardAssets.CinematicEffects
{
    public class SMAAEditor : IAntiAliasingEditor
    {
        private List<SerializedProperty> m_TopLevelFields = new List<SerializedProperty>();

        [Serializable]
        class InfoMap
        {
            public string name;
            public bool experimental;
            public bool quality;
            public List<SerializedProperty> properties;
        }
        private List<InfoMap> m_GroupFields = new List<InfoMap>();

        public void OnEnable(SerializedObject serializedObject, string path)
        {
            var topLevelSettings = typeof(SMAA).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(SMAA.TopLevelSettings), false).Any());
            var settingsGroups = typeof(SMAA).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(SMAA.SettingsGroup), false).Any());

            foreach (var group in topLevelSettings)
            {
                var searchPath = path + "." + group.Name + ".";

                foreach (var setting in group.FieldType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    var property = serializedObject.FindProperty(searchPath + setting.Name);
                    if (property != null)
                        m_TopLevelFields.Add(property);
                }
            }

            foreach (var group in settingsGroups)
            {
                var searchPath = path + "." + group.Name + ".";

                foreach (var setting in group.FieldType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    var infoGroup = m_GroupFields.FirstOrDefault(x => x.name == group.Name);
                    if (infoGroup == null)
                    {
                        infoGroup = new InfoMap();
                        infoGroup.properties = new List<SerializedProperty>();
                        infoGroup.name = group.Name;
                        infoGroup.quality = group.FieldType == typeof(SMAA.QualitySettings);
                        infoGroup.experimental = group.GetCustomAttributes(typeof(SMAA.ExperimentalGroup), false).Length > 0;
                        m_GroupFields.Add(infoGroup);
                    }

                    var property = serializedObject.FindProperty(searchPath + setting.Name);
                    if (property != null)
                    {
                        infoGroup.properties.Add(property);
                    }
                }
            }
        }

        public bool OnInspectorGUI(IAntiAliasing target)
        {
            EditorGUI.BeginChangeCheck();

            foreach (var setting in m_TopLevelFields)
                EditorGUILayout.PropertyField(setting);

            foreach (var group in m_GroupFields)
            {
                if (group.quality && (target as SMAA).settings.quality != SMAA.QualityPreset.Custom)
                {
                    continue;
                }

                string title = ObjectNames.NicifyVariableName(group.name);
                if (group.experimental)
                    title += " (Experimental)";

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                var enabledField = group.properties.FirstOrDefault(x => x.propertyPath == "m_SMAA." + group.name + ".enabled");
                if (enabledField != null && !enabledField.boolValue)
                {
                    EditorGUILayout.PropertyField(enabledField);
                    EditorGUI.indentLevel--;
                    continue;
                }

                foreach (var field in group.properties)
                    EditorGUILayout.PropertyField(field);

                EditorGUI.indentLevel--;
            }
            return EditorGUI.EndChangeCheck();
        }
    }
}
