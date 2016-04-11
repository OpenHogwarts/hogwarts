using UnityEngine;
using UnityEditor;

namespace UnityStandardAssets.CinematicEffects
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AmbientOcclusion))]
    public class AmbientOcclusionEditor : Editor
    {
        SerializedProperty _intensity;
        SerializedProperty _radius;
        SerializedProperty _sampleCount;
        SerializedProperty _sampleCountValue;
        SerializedProperty _blurIterations;
        SerializedProperty _downsampling;
        SerializedProperty _ambientOnly;
        SerializedProperty _debug;

        static GUIContent _textValue = new GUIContent("Value");

        static string _textNoAmbientOnly =
            "The ambient-only mode is currently disabled; " +
            "it needs deferred shading and HDR rendering.";

        void OnEnable()
        {
            _intensity = serializedObject.FindProperty("settings.intensity");
            _radius = serializedObject.FindProperty("settings.radius");
            _sampleCount = serializedObject.FindProperty("settings.sampleCount");
            _sampleCountValue = serializedObject.FindProperty("settings.sampleCountValue");
            _blurIterations = serializedObject.FindProperty("settings.blurIterations");
            _downsampling = serializedObject.FindProperty("settings.downsampling");
            _ambientOnly = serializedObject.FindProperty("settings.ambientOnly");
            _debug = serializedObject.FindProperty("settings.debug");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_intensity);
            EditorGUILayout.PropertyField(_radius);
            EditorGUILayout.PropertyField(_sampleCount);

            if (_sampleCount.hasMultipleDifferentValues ||
                _sampleCount.enumValueIndex == (int)AmbientOcclusion.SampleCount.Variable)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_sampleCountValue, _textValue);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_blurIterations);
            EditorGUILayout.PropertyField(_downsampling);
            EditorGUILayout.PropertyField(_ambientOnly);
            EditorGUILayout.PropertyField(_debug);

            // Show a warning if the ambient-only mode is not supported.
            if (!_ambientOnly.hasMultipleDifferentValues && _ambientOnly.boolValue)
                if (!((AmbientOcclusion)target).isAmbientOnlySupported)
                    EditorGUILayout.HelpBox(_textNoAmbientOnly, MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
