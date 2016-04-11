using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace UnityStandardAssets.CinematicEffects
{
    [CustomEditor(typeof(LensAberrations))]
    public class LensAberrationsEditor : Editor
    {
        private Dictionary<FieldInfo, List<SerializedProperty>> m_GroupFields = new Dictionary<FieldInfo, List<SerializedProperty>>();
        private List<SerializedProperty> m_SimpleProperties = new List<SerializedProperty>();
        private List<SerializedProperty> m_AdvancedProperties = new List<SerializedProperty>();

        private LensAberrations concreteTarget
        {
            get { return target as LensAberrations; }
        }

        private void PopulateMap(FieldInfo group)
        {
            var searchPath = group.Name + ".";
            foreach (var setting in group.FieldType.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                List<SerializedProperty> settingsGroup;
                if (!m_GroupFields.TryGetValue(group, out settingsGroup))
                {
                    settingsGroup = new List<SerializedProperty>();
                    m_GroupFields[group] = settingsGroup;
                }

                var property = serializedObject.FindProperty(searchPath + setting.Name);
                if (property != null)
                {
                    settingsGroup.Add(property);

                    if (setting.GetCustomAttributes(typeof(LensAberrations.SimpleSetting), false).Length > 0)
                        m_SimpleProperties.Add(property);
                    else if (setting.GetCustomAttributes(typeof(LensAberrations.AdvancedSetting), false).Length > 0)
                        m_AdvancedProperties.Add(property);
                }
            }
        }

        private void OnEnable()
        {
            var settingsGroups = typeof(LensAberrations).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(LensAberrations.SettingsGroup), false).Any());

            foreach (var settingGroup in settingsGroups)
                PopulateMap(settingGroup);
        }

        private void DrawFields()
        {
            foreach (var group in m_GroupFields)
            {
                var enabledField = group.Value.FirstOrDefault(x => x.propertyPath == group.Key.Name + ".enabled");
                var groupProperty = serializedObject.FindProperty(group.Key.Name);

                GUILayout.Space(5);
                bool display = EditorGUIHelper.Header(groupProperty, enabledField);
                if (!display)
                    continue;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(10);
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(3);
                        foreach (var field in group.Value.Where(x => x.propertyPath != group.Key.Name + ".enabled"))
                        {
                            if (group.Key.FieldType == typeof(LensAberrations.VignetteSettings))
                            {
                                if (m_SimpleProperties.Contains(field) && concreteTarget.vignette.mode != LensAberrations.SettingsMode.Simple ||
                                    m_AdvancedProperties.Contains(field) && concreteTarget.vignette.mode != LensAberrations.SettingsMode.Advanced)
                                    continue;
                            }
                            else
                            {
                                if (m_AdvancedProperties.Contains(field) && concreteTarget.chromaticAberration.mode != LensAberrations.SettingsMode.Advanced)
                                    continue;
                            }

                            EditorGUILayout.PropertyField(field);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawFields();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
