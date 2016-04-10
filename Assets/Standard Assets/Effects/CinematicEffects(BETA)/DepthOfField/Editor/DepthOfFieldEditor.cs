using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    [CustomEditor(typeof(DepthOfField))]
    class DepthOfFieldEditor : Editor
    {
        [CustomPropertyDrawer(typeof(DepthOfField.GradientRangeAttribute))]
        internal sealed class GradientRangeDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                if (property.propertyType != SerializedPropertyType.Float)
                {
                    EditorGUI.LabelField(position, label.text, "Use GradientRange with float.");
                    return;
                }

                var savedbackgroundColor = GUI.backgroundColor;
                var gradientRange = (DepthOfField.GradientRangeAttribute)attribute;

                float range01 = (property.floatValue - gradientRange.min) / (gradientRange.max - gradientRange.min);
                GUI.backgroundColor = Color.Lerp(Color.green, Color.yellow, range01);

                EditorGUI.Slider(position, property, gradientRange.min, gradientRange.max, label);

                GUI.backgroundColor = savedbackgroundColor;
            }
        }

        SerializedProperty m_UIMode;
        SerializedProperty m_VisualizeBluriness;
        SerializedProperty m_FocusPlane;
        SerializedProperty m_FocusRange;

        SerializedProperty m_NearPlane;
        SerializedProperty m_NearRadius;
        SerializedProperty m_HighQualityUpsampling;
        SerializedProperty m_DilateNearBlur;
        SerializedProperty m_PrefilterBlur;
        SerializedProperty m_MedianFilter;
        SerializedProperty m_FarPlane;
        SerializedProperty m_FarRadius;
        SerializedProperty m_ApertureShape;
        SerializedProperty m_UseBokehTexture;
        SerializedProperty m_ApertureOrientation;

        SerializedProperty m_BoostPoint;
        SerializedProperty m_NearBoostAmount;
        SerializedProperty m_FarBoostAmount;
        SerializedProperty m_FStops;
        SerializedProperty m_Radius;
        SerializedProperty m_FocusTransform;

        SerializedProperty m_Dx11BokehThreshold;
        SerializedProperty m_Dx11SpawnHeuristic;
        SerializedProperty m_Dx11BokehTexture;
        SerializedProperty m_Dx11BokehScale;
        SerializedProperty m_Dx11BokehIntensity;

        SerializedObject m_TargetSerializedDOFObject;

        SerializedProperty m_Quality;
        SerializedProperty m_CustomizeQualitySettings;


        bool useBokehTexture { get { return m_UseBokehTexture.boolValue; } }
        bool useAdvancedOrExplicitView { get { return (m_UIMode.enumValueIndex != (int)DepthOfField.UIMode.Basic); } }
        bool useExplicitView { get { return (m_UIMode.enumValueIndex == (int)DepthOfField.UIMode.Explicit); } }
        bool useBlurOrientation
        {
            get
            {
                return ((m_ApertureShape.enumValueIndex == (int)DepthOfField.ApertureShape.Hexagonal) ||
                        (m_ApertureShape.enumValueIndex == (int)DepthOfField.ApertureShape.Octogonal));
            }
        }
        AnimBool showQualitySettings = new AnimBool();
        AnimBool showBoostSettings = new AnimBool();
        AnimBool showBlurOrientation = new AnimBool();
        AnimBool showDX11BlurSettings = new AnimBool();
        AnimBool showExplicitSettings = new AnimBool();

        void OnEnable()
        {
            m_TargetSerializedDOFObject = new SerializedObject(target);

            m_UIMode = m_TargetSerializedDOFObject.FindProperty("uiMode");
            m_VisualizeBluriness = m_TargetSerializedDOFObject.FindProperty("visualizeBluriness");
            m_FocusPlane = m_TargetSerializedDOFObject.FindProperty("focusPlane");
            m_FocusRange = m_TargetSerializedDOFObject.FindProperty("focusRange");
            m_NearPlane = m_TargetSerializedDOFObject.FindProperty("nearPlane");
            m_NearRadius = m_TargetSerializedDOFObject.FindProperty("nearRadius");
            m_FarPlane = m_TargetSerializedDOFObject.FindProperty("farPlane");
            m_FarRadius = m_TargetSerializedDOFObject.FindProperty("farRadius");
            m_ApertureShape  = m_TargetSerializedDOFObject.FindProperty("apertureShape");
            m_ApertureOrientation = m_TargetSerializedDOFObject.FindProperty("apertureOrientation");
            m_UseBokehTexture = m_TargetSerializedDOFObject.FindProperty("useBokehTexture");
            m_BoostPoint = m_TargetSerializedDOFObject.FindProperty("boostPoint");
            m_NearBoostAmount = m_TargetSerializedDOFObject.FindProperty("nearBoostAmount");
            m_FarBoostAmount = m_TargetSerializedDOFObject.FindProperty("farBoostAmount");
            m_FStops = m_TargetSerializedDOFObject.FindProperty("fStops");
            m_Radius = m_TargetSerializedDOFObject.FindProperty("radius");
            m_FocusTransform = m_TargetSerializedDOFObject.FindProperty("focusTransform");
            m_Dx11BokehThreshold = m_TargetSerializedDOFObject.FindProperty("textureBokehThreshold");
            m_Dx11SpawnHeuristic = m_TargetSerializedDOFObject.FindProperty("textureBokehSpawnHeuristic");
            m_Dx11BokehTexture = m_TargetSerializedDOFObject.FindProperty("bokehTexture");
            m_Dx11BokehScale = m_TargetSerializedDOFObject.FindProperty("textureBokehScale");
            m_Dx11BokehIntensity = m_TargetSerializedDOFObject.FindProperty("textureBokehIntensity");
            m_HighQualityUpsampling = m_TargetSerializedDOFObject.FindProperty("highQualityUpsampling");
            m_DilateNearBlur = m_TargetSerializedDOFObject.FindProperty("dilateNearBlur");
            m_PrefilterBlur = m_TargetSerializedDOFObject.FindProperty("prefilterBlur");
            m_MedianFilter = m_TargetSerializedDOFObject.FindProperty("medianFilter");
            m_Quality = m_TargetSerializedDOFObject.FindProperty("quality");
            m_CustomizeQualitySettings = m_TargetSerializedDOFObject.FindProperty("customizeQualitySettings");
            InitializedAnimBools();
        }

        void InitializedAnimBools()
        {
            showQualitySettings.valueChanged.AddListener(Repaint);
            showQualitySettings.value = useAdvancedOrExplicitView;

            showBlurOrientation.valueChanged.AddListener(Repaint);
            showBlurOrientation.value = useBlurOrientation;

            showDX11BlurSettings.valueChanged.AddListener(Repaint);
            showDX11BlurSettings.value = useBokehTexture;

            showBoostSettings.valueChanged.AddListener(Repaint);
            showBoostSettings.value = useAdvancedOrExplicitView;

            showExplicitSettings.valueChanged.AddListener(Repaint);
            showExplicitSettings.value = useExplicitView;
        }

        void UpdateAnimBoolTargets()
        {
            showQualitySettings.target = useAdvancedOrExplicitView;
            showBlurOrientation.target = useBlurOrientation;
            showDX11BlurSettings.target = useBokehTexture;
            showBoostSettings.target = useAdvancedOrExplicitView;
            showExplicitSettings.target = useExplicitView;
        }

        void SyncParametersAndQualityLevel(bool fromQualityLevelToParams)
        {
            const float prefilterBlurQuality    = 10.0f;
            const float medianFilterQuality     = 30.0f;
            const float dilateNearBlurQuality   = 50.0f;
            const float medianFilterQualityHigh = 70.0f;
            const float highUpsamplingQuality   = 90.0f;

            if (fromQualityLevelToParams)
            {
                int medianFilterEnumValue = 0;
                if (m_Quality.floatValue >= medianFilterQuality) ++medianFilterEnumValue;
                if (m_Quality.floatValue >= medianFilterQualityHigh) ++medianFilterEnumValue;
                m_MedianFilter.enumValueIndex = medianFilterEnumValue;

                m_DilateNearBlur.boolValue = (m_Quality.floatValue >= dilateNearBlurQuality);
                m_PrefilterBlur.boolValue = (m_Quality.floatValue >= prefilterBlurQuality);
                m_HighQualityUpsampling.boolValue = (m_Quality.floatValue >= highUpsamplingQuality);
            }
            else
            {
                const float stepBack = 5.0f;

                //trying to find a quality level matching the parameters (going down first and climbing back)
                if (!m_HighQualityUpsampling.boolValue) m_Quality.floatValue = Mathf.Min(highUpsamplingQuality - stepBack, m_Quality.floatValue);
                if (!m_PrefilterBlur.boolValue) m_Quality.floatValue = Mathf.Min(prefilterBlurQuality - stepBack, m_Quality.floatValue);
                if (!m_DilateNearBlur.boolValue) m_Quality.floatValue = Mathf.Min(dilateNearBlurQuality - stepBack, m_Quality.floatValue);
                if (m_MedianFilter.enumValueIndex == (int)DepthOfField.FilterQuality.Normal) m_Quality.floatValue = Mathf.Min(medianFilterQualityHigh - stepBack, m_Quality.floatValue);
                if (m_MedianFilter.enumValueIndex == (int)DepthOfField.FilterQuality.None) m_Quality.floatValue = Mathf.Min(medianFilterQuality - stepBack, m_Quality.floatValue);

                if (m_HighQualityUpsampling.boolValue) m_Quality.floatValue = Mathf.Max(highUpsamplingQuality, m_Quality.floatValue);
                if (m_PrefilterBlur.boolValue) m_Quality.floatValue = Mathf.Max(prefilterBlurQuality, m_Quality.floatValue);
                if (m_DilateNearBlur.boolValue) m_Quality.floatValue = Mathf.Max(dilateNearBlurQuality, m_Quality.floatValue);
                if (m_MedianFilter.enumValueIndex == (int)DepthOfField.FilterQuality.High) m_Quality.floatValue = Mathf.Max(medianFilterQualityHigh, m_Quality.floatValue);
                if (m_MedianFilter.enumValueIndex == (int)DepthOfField.FilterQuality.Normal) m_Quality.floatValue = Mathf.Max(medianFilterQuality - 1, m_Quality.floatValue);
            }
        }

        private void OnQualityLevelInspectorGUI()
        {
            SyncParametersAndQualityLevel(!m_CustomizeQualitySettings.boolValue);

            GUI.enabled = !m_CustomizeQualitySettings.boolValue;
            EditorGUILayout.PropertyField(m_Quality);
            GUI.enabled = true;

            if (m_UIMode.enumValueIndex == (int)DepthOfField.UIMode.Basic)
            {
                m_CustomizeQualitySettings.boolValue = false;
            }
            if (EditorGUILayout.BeginFadeGroup(showQualitySettings.faded))
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CustomizeQualitySettings);

                ++EditorGUI.indentLevel;
                GUI.enabled = m_CustomizeQualitySettings.boolValue;
                EditorGUILayout.PropertyField(m_PrefilterBlur, new GUIContent(" Smooth Bokeh"));
                EditorGUILayout.PropertyField(m_MedianFilter, new GUIContent(" More smooth Bokeh"));
                EditorGUILayout.PropertyField(m_DilateNearBlur, new GUIContent(" Dilate near blur"));
                EditorGUILayout.PropertyField(m_HighQualityUpsampling, new GUIContent(" Smooth Edges"));
                GUI.enabled = true;
                EditorGUI.indentLevel -= 2;
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void OnBokehTextureInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_UseBokehTexture);

            if (EditorGUILayout.BeginFadeGroup(showDX11BlurSettings.faded))
            {
                if (!ImageEffectHelper.supportsDX11)
                {
                    EditorGUILayout.HelpBox("Bokeh Texture not supported (need shader model 5)",
                        MessageType.Info);
                }
            }
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(showDX11BlurSettings.faded))
            {
                if (ImageEffectHelper.supportsDX11)
                {
                    EditorGUILayout.PropertyField(m_Dx11BokehTexture, new GUIContent(" Texture"));
                    EditorGUILayout.PropertyField(m_Dx11BokehScale, new GUIContent(" Scale"));
                    EditorGUILayout.PropertyField(m_Dx11BokehIntensity, new GUIContent(" Intensity"));
                    EditorGUILayout.PropertyField(m_Dx11BokehThreshold, new GUIContent(" Min Luminance"));
                    EditorGUILayout.PropertyField(m_Dx11SpawnHeuristic, new GUIContent(" Spawn Heuristic"));
                }
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.Separator();
        }

        public override void OnInspectorGUI()
        {
            m_TargetSerializedDOFObject.Update();

            UpdateAnimBoolTargets();

            EditorGUILayout.LabelField("Simulates camera lens defocus", EditorStyles.miniLabel);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_VisualizeBluriness);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_UIMode, new GUIContent("Mode"));
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_ApertureShape);

            if (EditorGUILayout.BeginFadeGroup(showBlurOrientation.faded))
            {
                EditorGUILayout.PropertyField(m_ApertureOrientation);
            }
            EditorGUILayout.EndFadeGroup();

            if ((m_ApertureShape.enumValueIndex == (int)DepthOfField.ApertureShape.Circular)  ||
                (m_ApertureShape.enumValueIndex == (int)DepthOfField.ApertureShape.Hexagonal) ||
                (m_ApertureShape.enumValueIndex == (int)DepthOfField.ApertureShape.Octogonal)
                )
            {
                EditorGUILayout.Separator();
                OnQualityLevelInspectorGUI();
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(m_FocusPlane);
            EditorGUILayout.PropertyField(m_FocusTransform, new GUIContent("Focus on Transform"));
            EditorGUILayout.Separator();

            if (m_UIMode.enumValueIndex == (int)DepthOfField.UIMode.Basic)
            {
                EditorGUILayout.PropertyField(m_FStops, new GUIContent("F-Stops"));
                EditorGUILayout.PropertyField(m_FocusRange);
                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(m_Radius, new GUIContent("Max Blur"));
                m_NearRadius.floatValue = m_Radius.floatValue;
                m_FarRadius.floatValue = m_Radius.floatValue;
                m_UseBokehTexture.boolValue = false;
            }
            else
            {
                if (EditorGUILayout.BeginFadeGroup(1 - showExplicitSettings.faded))
                {
                    EditorGUILayout.Slider(m_FStops, 1.0f, 128.0f, new GUIContent("F-Stops"));
                    EditorGUILayout.PropertyField(m_FocusRange);
                }
                EditorGUILayout.EndFadeGroup();
                if (EditorGUILayout.BeginFadeGroup(showExplicitSettings.faded))
                {
                    EditorGUILayout.PropertyField(m_FocusRange);
                    EditorGUILayout.PropertyField(m_NearPlane);
                    EditorGUILayout.PropertyField(m_FarPlane);
                }
                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.Separator();
                OnBokehTextureInspectorGUI();

                if (EditorGUILayout.BeginFadeGroup(showDX11BlurSettings.faded))
                {
                    EditorGUILayout.PropertyField(m_Radius, new GUIContent("Max Blur"));
                    m_NearRadius.floatValue = m_Radius.floatValue;
                    m_FarRadius.floatValue = m_Radius.floatValue;
                }
                EditorGUILayout.EndFadeGroup();
                if (EditorGUILayout.BeginFadeGroup(1 - showDX11BlurSettings.faded))
                {
                    EditorGUILayout.PropertyField(m_NearRadius, new GUIContent("Near Max Blur"));
                    EditorGUILayout.PropertyField(m_FarRadius, new GUIContent("Far Max Blur"));
                    m_Radius.floatValue = (m_FarRadius.floatValue + m_NearRadius.floatValue) * 0.5f;
                }
                EditorGUILayout.EndFadeGroup();
            }

            EditorGUILayout.Separator();
            if (EditorGUILayout.BeginFadeGroup(showBoostSettings.faded))
            {
                EditorGUILayout.PropertyField(m_BoostPoint);
                EditorGUILayout.PropertyField(m_NearBoostAmount);
                EditorGUILayout.PropertyField(m_FarBoostAmount);
            }
            EditorGUILayout.EndFadeGroup();
            m_TargetSerializedDOFObject.ApplyModifiedProperties();
        }
    }
}
