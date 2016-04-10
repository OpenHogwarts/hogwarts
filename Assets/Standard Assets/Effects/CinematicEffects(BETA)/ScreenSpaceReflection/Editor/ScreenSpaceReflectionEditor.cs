using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    [CustomPropertyDrawer(typeof(ScreenSpaceReflection.SSRSettings.LayoutAttribute))]
    public class LayoutDrawer : PropertyDrawer
    {
        private const float kHeadingSpace = 22.0f;

        static Styles m_Styles;

        private class Styles
        {
            public readonly GUIStyle header = "ShurikenModuleTitle";

            internal Styles()
            {
                header.font = (new GUIStyle("Label")).font;
                header.border = new RectOffset(15, 7, 4, 4);
                header.fixedHeight = kHeadingSpace;
                header.contentOffset = new Vector2(20f, -2f);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return kHeadingSpace;

            var count = property.CountInProperty();
            return EditorGUIUtility.singleLineHeight * count  + 15;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (m_Styles == null)
                m_Styles = new Styles();

            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = Header(position, property.displayName, property.isExpanded);
            position.y += kHeadingSpace;

            if (!property.isExpanded)
                return;

            foreach (SerializedProperty child in property)
            {
                EditorGUI.PropertyField(position, child);
                position.y += EditorGUIUtility.singleLineHeight;
            }
        }

        private bool Header(Rect position, String title, bool display)
        {
            Rect rect = position;
            position.height = EditorGUIUtility.singleLineHeight;
            GUI.Box(rect, title, m_Styles.header);

            Rect toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (Event.current.type == EventType.Repaint)
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);

            Event e = Event.current;
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }
            return display;
        }
    }

    [CustomEditor(typeof(ScreenSpaceReflection))]
    internal class ScreenSpaceReflectionEditor : Editor
    {
        private static class StaticFieldFinder<T>
        {
            public static FieldInfo GetField<TValue>(Expression<Func<T, TValue>> selector)
            {
                Expression body = selector;
                if (body is LambdaExpression)
                {
                    body = ((LambdaExpression)body).Body;
                }
                switch (body.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        return (FieldInfo)((MemberExpression)body).Member;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private enum SettingsMode
        {
            HighQuality,
            Default,
            Performance,
            Custom,
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            var currentState = ((ScreenSpaceReflection)target).settings;

            var settingsMode = SettingsMode.Custom;
            if (currentState.Equals(ScreenSpaceReflection.SSRSettings.performanceSettings))
                settingsMode = SettingsMode.Performance;
            else if (currentState.Equals(ScreenSpaceReflection.SSRSettings.defaultSettings))
                settingsMode = SettingsMode.Default;
            else if (currentState.Equals(ScreenSpaceReflection.SSRSettings.highQualitySettings))
                settingsMode = SettingsMode.HighQuality;

            EditorGUI.BeginChangeCheck();
            settingsMode = (SettingsMode)EditorGUILayout.EnumPopup("Preset", settingsMode);
            if (EditorGUI.EndChangeCheck())
                Apply(settingsMode);

            // move into the settings fields...
            var settings = StaticFieldFinder<ScreenSpaceReflection>.GetField(x => x.settings);

            foreach (var setting in settings.FieldType.GetFields())
            {
                var prop = settings.Name + "." + setting.Name;
                var settingsProperty = serializedObject.FindProperty(prop);
                EditorGUILayout.PropertyField(settingsProperty);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void Apply(SettingsMode settingsMode)
        {
            switch (settingsMode)
            {
                case SettingsMode.Default:
                    Apply(ScreenSpaceReflection.SSRSettings.defaultSettings);
                    break;
                case SettingsMode.HighQuality:
                    Apply(ScreenSpaceReflection.SSRSettings.highQualitySettings);
                    break;
                case SettingsMode.Performance:
                    Apply(ScreenSpaceReflection.SSRSettings.performanceSettings);
                    break;
            }
        }

        private void Apply(ScreenSpaceReflection.SSRSettings settings)
        {
            var validTargets = targets.Where(x => x is ScreenSpaceReflection).Cast<ScreenSpaceReflection>().ToArray();

            Undo.RecordObjects(validTargets, "Apply SSR Settings");
            foreach (var validTarget in validTargets)
                validTarget.settings = settings;
        }
    }
}
