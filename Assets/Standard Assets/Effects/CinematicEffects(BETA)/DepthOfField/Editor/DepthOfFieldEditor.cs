using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityStandardAssets.CinematicEffects
{
    [CustomEditor(typeof(DepthOfField))]
    class DepthOfFieldEditor : Editor
    {
        private List<SerializedProperty> m_TopLevelFields = new List<SerializedProperty>();
        private Dictionary<FieldInfo, List<SerializedProperty>> m_GroupFields = new Dictionary<FieldInfo, List<SerializedProperty>>();
        private Dictionary<DepthOfField.TweakMode, List<SerializedProperty>> m_AccessFields = new Dictionary<DepthOfField.TweakMode, List<SerializedProperty>>();

        private DepthOfField.TweakMode tweakMode
        {
            get { return ((DepthOfField)target).settings.tweakMode; }
        }

        private void OnEnable()
        {
            var topLevelSettings = typeof(DepthOfField).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(DepthOfField.TopLevelSettings), false).Any());
            var settingsGroups = typeof(DepthOfField).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(DepthOfField.SettingsGroup), false).Any());

            foreach (var group in topLevelSettings)
            {
                var searchPath = group.Name + ".";

                foreach (var setting in group.FieldType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    var property = serializedObject.FindProperty(searchPath + setting.Name);
                    if (property != null)
                        m_TopLevelFields.Add(property);
                }
            }

            var basicFields = new List<SerializedProperty>();
            var advancedFields = new List<SerializedProperty>();
            var explicitFields = new List<SerializedProperty>();

            foreach (var group in settingsGroups)
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
                        if (setting.GetCustomAttributes(typeof(DepthOfField.Basic), false).Length > 0)
                            basicFields.Add(property);
                        if (setting.GetCustomAttributes(typeof(DepthOfField.Advanced), false).Length > 0)
                            advancedFields.Add(property);
                        if (setting.GetCustomAttributes(typeof(DepthOfField.Explicit), false).Length > 0)
                            explicitFields.Add(property);
                    }
                }
            }

            m_AccessFields[DepthOfField.TweakMode.Basic] = basicFields;
            m_AccessFields[DepthOfField.TweakMode.Advanced] = advancedFields;
            m_AccessFields[DepthOfField.TweakMode.Explicit] = explicitFields;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            foreach (var setting in m_TopLevelFields)
                EditorGUILayout.PropertyField(setting);

            List<SerializedProperty> accessList = m_AccessFields[tweakMode];

            foreach (var group in m_GroupFields)
            {
                if (group.Key.FieldType == typeof(DepthOfField.QualitySettings) && ((DepthOfField)target).settings.quality != DepthOfField.QualityPreset.Custom)
                    continue;

                bool forceInclude = group.Key.GetCustomAttributes(typeof(DepthOfField.AllTweakModes), false).Length > 0;

                int count = group.Value.Count(x => accessList.Contains(x));
                if (!forceInclude && count == 0)
                    continue;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(group.Key.Name), EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                foreach (var field in group.Value)
                {
                    if (forceInclude || accessList.Contains(field))
                        EditorGUILayout.PropertyField(field);
                }

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
