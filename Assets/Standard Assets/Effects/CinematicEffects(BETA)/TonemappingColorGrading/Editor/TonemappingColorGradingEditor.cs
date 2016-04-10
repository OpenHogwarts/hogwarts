using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    [CustomPropertyDrawer(typeof(TonemappingColorGrading.ColorWheelGroup))]
    class ColorWheelGroupDrawer : PropertyDrawer
    {
        private int m_RenderSizePerWheel;
        private int m_NumberOfWheels;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var wheelAttribute = (TonemappingColorGrading.ColorWheelGroup)attribute;
            property.isExpanded = true;

            m_NumberOfWheels = property.CountInProperty() - 1;
            if (m_NumberOfWheels == 0)
                return 0;

            m_RenderSizePerWheel = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth) / m_NumberOfWheels) - 30;
            m_RenderSizePerWheel = Mathf.Clamp(m_RenderSizePerWheel, wheelAttribute.minSizePerWheel, wheelAttribute.maxSizePerWheel);
            return TonemappingColorGradingEditor.ColorWheel.GetColorWheelHeight(m_RenderSizePerWheel);
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (m_NumberOfWheels == 0)
                return;

            var width = position.width;
            Rect newPosition = new Rect(position.x, position.y, width / m_NumberOfWheels, position.height);
            foreach (SerializedProperty prop in property)
            {
                if (prop.propertyType == SerializedPropertyType.Color)
                    prop.colorValue = TonemappingColorGradingEditor.ColorWheel.DoGUI(newPosition, prop.displayName, prop.colorValue, m_RenderSizePerWheel);
                newPosition.x += width / m_NumberOfWheels;
            }
        }
    }

    [CustomEditor(typeof(TonemappingColorGrading))]
    [CanEditMultipleObjects]
    class TonemappingColorGradingEditor : Editor
    {
        static Styles s_Styles;
        class Styles
        {
            public GUIStyle thumb2D = "ColorPicker2DThumb";
            public GUIStyle pickerBox = "ColorPickerBox";
            public GUIStyle thumbHoriz = "ColorPickerHorizThumb";
            public GUIStyle header = "ShurikenModuleTitle";
            public GUIStyle headerCheckbox = "ShurikenCheckMark";
            public Vector2 thumb2DSize;

            internal Styles()
            {
                thumb2DSize = new Vector2(
                        !Mathf.Approximately(thumb2D.fixedWidth, 0f) ? thumb2D.fixedWidth : thumb2D.padding.horizontal,
                        !Mathf.Approximately(thumb2D.fixedHeight, 0f) ? thumb2D.fixedHeight : thumb2D.padding.vertical
                        );

                header.font = (new GUIStyle("Label")).font;
                header.border = new RectOffset(15, 7, 4, 4);
                header.fixedHeight = 22;
                header.contentOffset = new Vector2(20f, -2f);
            }
        }

        // settings group <setting, property reference>
        Dictionary<FieldInfo, List<SerializedProperty>> m_GroupFields = new Dictionary<FieldInfo, List<SerializedProperty>>();

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
                    settingsGroup.Add(property);
            }
        }

        void OnEnable()
        {
            var settingsGroups = typeof(TonemappingColorGrading).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(TonemappingColorGrading.SettingsGroup), false).Any());
            foreach (var settingGroup in settingsGroups)
                PopulateMap(settingGroup);

            // Histogram
            concreteTarget.onFrameEndEditorOnly = OnFrameEnd;
            m_CPUHistogram = new CPUHistogram();
            m_GPUHistogram = new GPUHistogram(concreteTarget.histogramShader, concreteTarget.histogramComputeShader);
        }

        // Cleanup
        void OnDisable()
        {
            concreteTarget.onFrameEndEditorOnly = null;
            if (m_CPUHistogram != null)
                m_CPUHistogram.Destroy();
            if (m_GPUHistogram != null)
                m_GPUHistogram.Destroy();
        }

        private bool Header(SerializedProperty group, SerializedProperty enabledField)
        {
            var display = group == null || group.isExpanded;
            var enabled = enabledField != null && enabledField.boolValue;
            var title = group == null ? "Unknown Group" : ObjectNames.NicifyVariableName(group.displayName);

            Rect rect = GUILayoutUtility.GetRect(16f, 22f, s_Styles.header);
            GUI.Box(rect, title, s_Styles.header);

            Rect toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            if (Event.current.type == EventType.Repaint)
                s_Styles.headerCheckbox.Draw(toggleRect, false, false, enabled, false);

            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if (toggleRect.Contains(e.mousePosition) && enabledField != null)
                {
                    enabledField.boolValue = !enabledField.boolValue;
                    e.Use();
                }
                else if (rect.Contains(e.mousePosition) && group != null)
                {
                    display = !display;
                    group.isExpanded = !group.isExpanded;
                    e.Use();
                }
            }
            return display;
        }

        private void DrawFields()
        {
            foreach (var group in m_GroupFields)
            {
                var enabledField = group.Value.FirstOrDefault(x => x.propertyPath == group.Key.Name + ".enabled");
                var groupProperty = serializedObject.FindProperty(group.Key.Name);

                GUILayout.Space(5);
                bool display = Header(groupProperty, enabledField);
                if (!display)
                    continue;

                //Special case for the filmic curve
                if (group.Key.GetCustomAttributes(false).Any(x => x.GetType() == typeof(TonemappingColorGrading.DrawFilmicCurveAttribute)))
                    DrawFilmicCurve(concreteTarget);

                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginVertical();
                GUILayout.Space(3);
                foreach (var field in group.Value.Where(x => x.propertyPath != group.Key.Name + ".enabled"))
                    EditorGUILayout.PropertyField(field);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        public override void OnInspectorGUI()
        {
            if (s_Styles == null)
                s_Styles = new Styles();

            serializedObject.Update();
            GUILayout.Label("All following effects will use LDR color buffers", EditorStyles.miniBoldLabel);

            var texture = serializedObject.FindProperty("m_UserLutTexture");

            //if (TonemappingColorGrading.Mode.GenerateLUT != (TonemappingColorGrading.Mode) mode.enumValueIndex)
            EditorGUILayout.PropertyField(texture);
            //else
            DrawFields();

            if (concreteTarget.filmicCurve.enabled)
            {
                var camera = concreteTarget.GetComponent<Camera>();
                if (camera != null && !camera.hdr)
                    EditorGUILayout.HelpBox("The camera is not HDR enabled. This will likely break the Tonemapper.", MessageType.Warning);
                else if (!concreteTarget.validRenderTextureFormat)
                    EditorGUILayout.HelpBox("The input to Tonemapper is not in HDR. Make sure that all effects prior to this are executed in HDR.", MessageType.Warning);
            }
            serializedObject.ApplyModifiedProperties();
        }

        public static void DrawFilmicCurve(TonemappingColorGrading target)
        {
            const int h = 128;
            const int h1 = h - 1;
            Rect rect;

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                rect = GUILayoutUtility.GetRect(Mathf.Min(EditorGUIUtility.currentViewWidth - 50f, 512f), h);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            // Background
            GUI.Box(rect, GUIContent.none);

            // Curve points
            int w = Mathf.FloorToInt(rect.width);
            Vector3[] c = new Vector3[w];

            float lutA = TonemappingColorGrading.GetLutA();

            TonemappingColorGrading.SimplePolyFunc polyToe;
            TonemappingColorGrading.SimplePolyFunc polyLinear;
            TonemappingColorGrading.SimplePolyFunc polyShoulder;

            var curveData = target.filmicCurve;

            const float gammaSpace = 2.2f;

            float x0 = Mathf.Pow(1.0f / 3.0f, gammaSpace);
            const float shoulderBase = .7f;
            float x1 = Mathf.Pow(shoulderBase, gammaSpace);
            float gammaHighY = Mathf.Pow(shoulderBase, 1.0f + (curveData.lutShoulder) * 1.0f);
            float y1 = Mathf.Pow(gammaHighY, gammaSpace);

            float t = x0 / x1;
            float lin = t * y1;
            float low = lin * (1.0f - curveData.toe * .5f);
            var y0 = low;

            float dx = x1 - x0;
            float dy = y1 - y0;

            float m = 0.0f;
            if (dx > 0 && dy > 0)
                m = dy / dx;

            // Linear section, power is 1, slope is m
            polyLinear.x0 = x0;
            polyLinear.y0 = y0;
            polyLinear.A = m;
            polyLinear.B = 1.0f;
            polyLinear.signX = 1.0f;
            polyLinear.signY = 1.0f;
            polyLinear.logA = Mathf.Log(m);

            // Toe
            polyToe = polyLinear;
            polyToe.Initialize(x0, y0, m);

            float linearW = target.GetWhitePoint();

            // Shoulder, first think about it "backwards"
            float offsetX = linearW - x1;
            float offsetY = 1.0f - y1;

            polyShoulder = polyLinear;
            polyShoulder.Initialize(offsetX, offsetY, m);

            // Flip horizontal
            polyShoulder.signX = -1.0f;
            polyShoulder.x0 = -linearW;

            // Flip vertical
            polyShoulder.signY = -1.0f;
            polyShoulder.y0 = 1.0f;

            float oneOverDim = 1.0f / (1.0f * w - 1.0f);

            for (int i = 0; i < w; i++)
            {
                float src = (i * 1.0f) * oneOverDim;
                float dst = target.EvalFilmicHelper(src, lutA,
                        polyToe,
                        polyLinear,
                        polyShoulder,
                        x0, x1, linearW);

                dst = Mathf.LinearToGammaSpace(dst);
                dst = Mathf.Clamp01(dst);
                c[i] = new Vector3(rect.x + i, rect.y + (h - dst * h1), 0f);
            }

            // Curve drawing
            Handles.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            Handles.DrawAAPolyLine(2f, c);
        }

        CPUHistogram m_CPUHistogram;
        GPUHistogram m_GPUHistogram;

        private IHistogram histogram
        {
            get
            {
                if (SupportsGPUHistogram())
                    return m_GPUHistogram;
                return m_CPUHistogram;
            }
        }

        public enum HistogramMode
        {
            Red = 0,
            Green = 1,
            Blue = 2,
            Luminance = 3,
            RGB,
        }

        private bool m_LogHistogram;
        private bool m_RefreshHistogramOnPlay;
        private HistogramMode m_HistogramMode = HistogramMode.RGB;
        private bool SupportsGPUHistogram()
        {
            return concreteTarget.histogramComputeShader != null
                   && SystemInfo.supportsComputeShaders
                   && concreteTarget.histogramShader != null
                   && concreteTarget.histogramShader.isSupported;
        }

        TonemappingColorGrading concreteTarget
        {
            get { return target as TonemappingColorGrading; }
        }

        public override bool HasPreviewGUI()
        {
            return targets.Length == 1 && concreteTarget != null && concreteTarget.enabled;
        }

        Rect m_HistogramRect;
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            serializedObject.Update();

            if (Event.current.type == EventType.Repaint)
            {
                // If m_HistogramRect isn't set the preview was just opened so refresh the render to get the histogram data
                if (m_HistogramRect.width == 0 && m_HistogramRect.height == 0)
                    InternalEditorUtility.RepaintAllViews();

                // Sizing
                float width = Mathf.Min(512f, r.width);
                float height = Mathf.Min(128f, r.height);
                m_HistogramRect = new Rect(
                        Mathf.Floor(r.x + r.width / 2f - width / 2f),
                        Mathf.Floor(r.y + r.height / 2f - height / 2f),
                        width, height
                        );

                histogram.DoGUI(m_HistogramRect, m_LogHistogram, m_HistogramMode);
            }

            // Toolbar
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            {
                concreteTarget.debugClamp = GUILayout.Toggle(concreteTarget.debugClamp, new GUIContent("Clipping", "Turns all overexposed pixels pink in the game view"), EditorStyles.miniButtonLeft);
                m_LogHistogram = GUILayout.Toggle(m_LogHistogram, new GUIContent("Log", "Logarithmic histogram"), EditorStyles.miniButtonMid);
                m_RefreshHistogramOnPlay = GUILayout.Toggle(m_RefreshHistogramOnPlay, new GUIContent("Refresh on Play", "Keep refreshing the histogram in play mode; this will impact performances"), EditorStyles.miniButtonRight);
                GUILayout.FlexibleSpace();
                m_HistogramMode = (HistogramMode)EditorGUILayout.EnumPopup(m_HistogramMode);
            }
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
                InternalEditorUtility.RepaintAllViews();
        }

        private void OnFrameEnd(RenderTexture source, Material tonemapMaterial)
        {
            if (Application.isPlaying && !m_RefreshHistogramOnPlay)
                return;

            if (Mathf.Approximately(m_HistogramRect.width, 0) || Mathf.Approximately(m_HistogramRect.height, 0))
                return;

            int pass = (int)(concreteTarget.fastMode ? TonemappingColorGrading.Passes.OneD : TonemappingColorGrading.Passes.ThreeD);

            int rw = SupportsGPUHistogram() ? 512 : 160;
            RenderTexture rtt = RenderTexture.GetTemporary(rw, rw, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rtt, tonemapMaterial, pass);

            histogram.Update(source, m_HistogramRect, m_LogHistogram, m_HistogramMode);

            Repaint();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rtt);
        }

        public static class ColorWheel
        {
            // Constants
            const float PI_2 = Mathf.PI / 2f;
            const float PI2 = Mathf.PI * 2f;

            // Hue Wheel
            static Texture2D s_WheelTexture;
            static float s_LastDiameter;
            private static GUIStyle s_centeredStyle;

            public static Color DoGUI(Rect area, string title, Color color, float diameter)
            {
                var labelrect = area;
                labelrect.height = EditorGUIUtility.singleLineHeight;

                if (s_centeredStyle == null)
                {
                    s_centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"))
                    {
                        alignment = TextAnchor.UpperCenter
                    };
                }
                GUI.Label(labelrect, title, s_centeredStyle);

                // figure out the wheel draw area
                var wheelDrawArea = area;
                wheelDrawArea.y += EditorGUIUtility.singleLineHeight;
                wheelDrawArea.height = diameter;
                if (wheelDrawArea.width > wheelDrawArea.height)
                {
                    wheelDrawArea.x += (wheelDrawArea.width - wheelDrawArea.height) / 2.0f;
                    wheelDrawArea.width = area.height;
                }
                wheelDrawArea.width = wheelDrawArea.height;

                var radius = diameter / 2.0f;
                Vector3 hsv;
                Color.RGBToHSV(color, out hsv.x, out hsv.y, out hsv.z);

                if (Event.current.type == EventType.Repaint)
                {
                    if (!Mathf.Approximately(diameter, s_LastDiameter))
                    {
                        s_LastDiameter = diameter;
                        UpdateHueWheel((int)diameter);
                    }

                    // Wheel
                    GUI.DrawTexture(wheelDrawArea, s_WheelTexture);

                    // Thumb
                    Vector2 thumbPos = Vector2.zero;
                    float theta = hsv.x * PI2;
                    float len = hsv.y * radius;
                    thumbPos.x = Mathf.Cos(theta + PI_2);
                    thumbPos.y = Mathf.Sin(theta - PI_2);
                    thumbPos *= len;
                    Vector2 thumbSize = s_Styles.thumb2DSize;
                    Color oldColor = GUI.color;
                    GUI.color = Color.black;
                    s_Styles.thumb2D.Draw(new Rect(wheelDrawArea.x + radius + thumbPos.x - thumbSize.x / 2f, wheelDrawArea.y + radius + thumbPos.y - thumbSize.y / 2f, thumbSize.x, thumbSize.y), false, false, false, false);
                    GUI.color = oldColor;
                }
                hsv = GetInput(wheelDrawArea, hsv, radius);

                var sliderDrawArea = wheelDrawArea;
                sliderDrawArea.y = sliderDrawArea.yMax;
                sliderDrawArea.height = EditorGUIUtility.singleLineHeight;

                hsv.y = GUI.HorizontalSlider(sliderDrawArea, hsv.y, 1e-04f, 1f);
                color = Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
                return color;
            }

            static readonly int thumbHash = "colorWheelThumb".GetHashCode();
            static Vector3 GetInput(Rect bounds, Vector3 hsv, float radius)
            {
                Event e = Event.current;

                var id = GUIUtility.GetControlID(thumbHash, FocusType.Passive, bounds);

                Vector2 mousePos = e.mousePosition;
                Vector2 relativePos = mousePos - new Vector2(bounds.x, bounds.y);
                if (e.type == EventType.MouseDown && e.button == 0 && GUIUtility.hotControl == 0)
                {
                    if (bounds.Contains(mousePos))
                    {
                        Vector2 center = new Vector2(bounds.x + radius, bounds.y + radius);
                        float dist = Vector2.Distance(center, mousePos);

                        if (dist <= radius)
                        {
                            e.Use();
                            GetWheelHueSaturation(relativePos.x, relativePos.y, radius, out hsv.x, out hsv.y);
                            GUIUtility.hotControl = id;
                        }
                    }
                }
                else if (e.type == EventType.MouseDrag && e.button == 0 && GUIUtility.hotControl == id)
                {
                    Vector2 center = new Vector2(bounds.x + radius, bounds.y + radius);
                    float dist = Vector2.Distance(center, mousePos);
                    if (dist <= radius)
                    {
                        e.Use();
                        GetWheelHueSaturation(relativePos.x, relativePos.y, radius, out hsv.x, out hsv.y);
                    }
                }
                else if (e.type == EventType.MouseUp && e.button == 0 && GUIUtility.hotControl == id)
                {
                    e.Use();
                    GUIUtility.hotControl = 0;
                }

                return hsv;
            }

            static void GetWheelHueSaturation(float x, float y, float radius, out float hue, out float saturation)
            {
                float dx = (x - radius) / radius;
                float dy = (y - radius) / radius;
                float d = Mathf.Sqrt((dx * dx + dy * dy));
                hue = Mathf.Atan2(dx, -dy);
                hue = 1f - ((hue > 0) ? hue : PI2 + hue) / PI2;
                saturation = Mathf.Clamp01(d);
            }

            static void UpdateHueWheel(int diameter)
            {
                CleanTexture(s_WheelTexture);
                s_WheelTexture = MakeTexture(diameter);

                var radius = diameter / 2.0f;

                Color[] pixels = s_WheelTexture.GetPixels();

                for (int y = 0; y < diameter; y++)
                {
                    for (int x = 0; x < diameter; x++)
                    {
                        int index = y * diameter + x;
                        float dx = (x - radius) / radius;
                        float dy = (y - radius) / radius;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);

                        // Out of the wheel, early exit
                        if (d >= 1f)
                        {
                            pixels[index] = new Color(0f, 0f, 0f, 0f);
                            continue;
                        }

                        // Red (0) on top, counter-clockwise (industry standard)
                        float saturation = d;
                        float hue = Mathf.Atan2(dx, dy);
                        hue = 1f - ((hue > 0) ? hue : PI2 + hue) / PI2;
                        Color color = Color.HSVToRGB(hue, saturation, 1f);

                        // Quick & dirty antialiasing
                        color.a = (saturation > 0.99) ? (1f - saturation) * 100f : 1f;

                        pixels[index] = color;
                    }
                }

                s_WheelTexture.SetPixels(pixels);
                s_WheelTexture.Apply();
            }

            static Texture2D MakeTexture(int dimension)
            {
                Texture2D tex = new Texture2D(dimension, dimension, TextureFormat.ARGB32, false, true)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave,
                    alphaIsTransparency = true
                };
                return tex;
            }

            static void CleanTexture(Texture2D texture)
            {
                if (texture != null)
                    DestroyImmediate(texture);
            }

            public static float GetColorWheelHeight(int renderSizePerWheel)
            {
                // wheel height + title label + alpha slider
                return renderSizePerWheel + 2 * EditorGUIUtility.singleLineHeight;
            }
        }

        interface IHistogram
        {
            void DoGUI(Rect rect, bool log, HistogramMode mode);
            void Update(RenderTexture source, Rect rect, bool log, HistogramMode mode);
            void Destroy();
        }

        private static readonly Color s_MasterCurveColor = new Color(1f, 1f, 1f, 2f);
        private static readonly Color s_RedCurveColor = new Color(1f, 0f, 0f, 2f);
        private static readonly Color s_GreenCurveColor = new Color(0f, 1f, 0f, 2f);
        private static readonly Color s_BlueCurveColor = new Color(0f, 1f, 1f, 2f);

        class CPUHistogram : IHistogram
        {
            int[] m_Histogram = new int[256];
            int[] m_HistogramRGB = new int[256 * 3];
            Texture2D m_TempTexture;

            public void DoGUI(Rect rect, bool log, HistogramMode mode)
            {
                if (mode == HistogramMode.RGB)
                    DoGUIrgb(rect, log);
                else
                    DoGUImono(rect, log, mode);
            }

            void DoGUImono(Rect rect, bool log, HistogramMode mode)
            {
                // Scale histogram values
                int[] scaledHistogram = new int[256];

                int max = 0;
                for (int i = 0; i < 256; i++)
                    max = (max < m_Histogram[i]) ? m_Histogram[i] : max;

                scaledHistogram = new int[256];

                if (log)
                {
                    float factor = rect.height / Mathf.Log10(max);

                    for (int i = 0; i < 256; i++)
                        scaledHistogram[i] = (m_Histogram[i] == 0) ? 0 : Mathf.Max(Mathf.RoundToInt(Mathf.Log10(m_Histogram[i]) * factor), 1);
                }
                else
                {
                    float factor = rect.height / max;

                    for (int i = 0; i < 256; i++)
                        scaledHistogram[i] = Mathf.Max(Mathf.RoundToInt(m_Histogram[i] * factor), 1);
                }

                // Color
                if (mode == HistogramMode.Red)
                    Handles.color = s_RedCurveColor;
                else if (mode == HistogramMode.Green)
                    Handles.color = s_GreenCurveColor;
                else if (mode == HistogramMode.Blue)
                    Handles.color = s_BlueCurveColor;
                else
                    Handles.color = s_MasterCurveColor;

                // Base line
                Vector2 p1 = new Vector2(rect.x - 1, rect.yMax);
                Vector2 p2 = new Vector2(rect.xMax - 1, rect.yMax);
                Handles.DrawLine(p1, p2);

                // Histogram
                for (int i = 0; i < (int)rect.width; i++)
                {
                    float remapI = (float)i / rect.width * 255f;
                    int index = Mathf.FloorToInt(remapI);
                    float fract = remapI - (float)index;
                    float v1 = scaledHistogram[index];
                    float v2 = scaledHistogram[Mathf.Min(index + 1, 255)];
                    float h = v1 * (1.0f - fract) + v2 * fract;
                    Handles.DrawLine(
                        new Vector2(rect.x + i, rect.yMax),
                        new Vector2(rect.x + i, rect.yMin + (rect.height - h))
                        );
                }
            }

            void DoGUIrgb(Rect rect, bool log)
            {
                // Scale histogram values
                Vector3 max = Vector3.zero;
                for (int i = 0; i < 256; i++)
                {
                    max.x = (max.x < m_HistogramRGB[i]) ? m_HistogramRGB[i] : max.x;
                    max.y = (max.y < m_HistogramRGB[i + 256]) ? m_HistogramRGB[i + 256] : max.y;
                    max.z = (max.z < m_HistogramRGB[i + 512]) ? m_HistogramRGB[i + 512] : max.z;
                }

                Vector3[] scaledHistogramRGB = new Vector3[256];

                if (log)
                {
                    Vector3 factor = new Vector3(
                            rect.height / Mathf.Log10(max.x),
                            rect.height / Mathf.Log10(max.y),
                            rect.height / Mathf.Log10(max.z)
                            );

                    for (int i = 0; i < 256; i++)
                    {
                        scaledHistogramRGB[i] = new Vector3(
                                (m_HistogramRGB[i] == 0) ? 0 : Mathf.Max(Mathf.RoundToInt(Mathf.Log10(m_HistogramRGB[i]) * factor.x), 1),
                                (m_HistogramRGB[i + 256] == 0) ? 0 : Mathf.Max(Mathf.RoundToInt(Mathf.Log10(m_HistogramRGB[i + 256]) * factor.y), 1),
                                (m_HistogramRGB[i + 512] == 0) ? 0 : Mathf.Max(Mathf.RoundToInt(Mathf.Log10(m_HistogramRGB[i + 512]) * factor.z), 1)
                                );
                    }
                }
                else
                {
                    Vector3 factor = new Vector3(rect.height / max.x, rect.height / max.y, rect.height / max.z);

                    for (int i = 0; i < 256; i++)
                    {
                        scaledHistogramRGB[i] = new Vector3(
                                Mathf.Max(Mathf.RoundToInt(m_HistogramRGB[i] * factor.x), 1),
                                Mathf.Max(Mathf.RoundToInt(m_HistogramRGB[i + 256] * factor.y), 1),
                                Mathf.Max(Mathf.RoundToInt(m_HistogramRGB[i + 512] * factor.z), 1)
                                );
                    }
                }

                // Base line
                Handles.color = s_MasterCurveColor;
                Vector2 p1 = new Vector2(rect.x - 1, rect.yMax);
                Vector2 p2 = new Vector2(rect.xMax - 1, rect.yMax);
                Handles.DrawLine(p1, p2);
                Color[] colors = { s_RedCurveColor, s_GreenCurveColor, s_BlueCurveColor };

                // Histogram
                for (int i = 0; i < (int)rect.width; i++)
                {
                    int[] heights = new int[3];

                    for (int j = 0; j < 3; j++)
                    {
                        float remapI = (float)i / rect.width * 255f;
                        int index = Mathf.FloorToInt(remapI);
                        float fract = remapI - (float)index;
                        float v1 = scaledHistogramRGB[index][j];
                        float v2 = scaledHistogramRGB[Mathf.Min(index + 1, 255)][j];
                        heights[j] = (int)(v1 * (1.0f - fract) + v2 * fract);
                    }

                    int[] indices = { 0, 1, 2 };
                    Array.Sort<int>(indices, (a, b) => heights[a].CompareTo(heights[b]));

                    Handles.color = s_MasterCurveColor;
                    Handles.DrawLine(
                        new Vector2(rect.x + i, rect.yMax),
                        new Vector2(rect.x + i, rect.yMin + (rect.height - heights[indices[0]]))
                        );

                    Handles.color = colors[indices[2]] + colors[indices[1]];
                    Handles.DrawLine(
                        new Vector2(rect.x + i, rect.yMin + (rect.height - heights[indices[0]])),
                        new Vector2(rect.x + i, rect.yMin + (rect.height - heights[indices[1]]))
                        );

                    Handles.color = colors[indices[2]];
                    Handles.DrawLine(
                        new Vector2(rect.x + i, rect.yMin + (rect.height - heights[indices[1]])),
                        new Vector2(rect.x + i, rect.yMin + (rect.height - heights[indices[2]]))
                        );
                }
            }

            public void Update(RenderTexture source, Rect rect, bool log, HistogramMode mode)
            {
                if (m_TempTexture == null || m_TempTexture.height != source.height || m_TempTexture.width != source.width)
                {
                    DestroyImmediate(m_TempTexture);
                    m_TempTexture = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
                    m_TempTexture.anisoLevel = 0;
                    m_TempTexture.wrapMode = TextureWrapMode.Clamp;
                    m_TempTexture.filterMode = FilterMode.Bilinear;
                    m_TempTexture.hideFlags = HideFlags.HideAndDontSave;
                }

                // Grab the screen content for the camera
                RenderTexture.active = source;
                m_TempTexture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
                m_TempTexture.Apply();
                RenderTexture.active = null;

                // Raw histogram
                Color[] pixels = m_TempTexture.GetPixels();

                switch (mode)
                {
                    case HistogramMode.Luminance:
                        Array.Clear(m_Histogram, 0, 256);
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            Color c = pixels[i];
                            m_Histogram[(int)((c.r * 0.2125f + c.g * 0.7154f + c.b * 0.0721f) * 255)]++;
                        }
                        break;
                    case HistogramMode.RGB:
                        Array.Clear(m_HistogramRGB, 0, 256 * 3);
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            Color c = pixels[i];
                            m_HistogramRGB[(int)(c.r * 255)]++;
                            m_HistogramRGB[(int)(c.g * 255) + 256]++;
                            m_HistogramRGB[(int)(c.b * 255) + 512]++;
                        }
                        break;
                    case HistogramMode.Red:
                        Array.Clear(m_Histogram, 0, 256);
                        for (int i = 0; i < pixels.Length; i++)
                            m_Histogram[(int)(pixels[i].r * 255)]++;
                        break;
                    case HistogramMode.Green:
                        Array.Clear(m_Histogram, 0, 256);
                        for (int i = 0; i < pixels.Length; i++)
                            m_Histogram[(int)(pixels[i].g * 255)]++;
                        break;
                    case HistogramMode.Blue:
                        Array.Clear(m_Histogram, 0, 256);
                        for (int i = 0; i < pixels.Length; i++)
                            m_Histogram[(int)(pixels[i].b * 255)]++;
                        break;
                }
            }

            public void Destroy()
            {
                if (m_TempTexture != null)
                    DestroyImmediate(m_TempTexture);
            }
        }

        class GPUHistogram : IHistogram
        {
            RenderTexture m_RT;
            Material m_Material;
            Shader m_Shader;
            ComputeShader m_ComputeShader;
            ComputeBuffer m_Buffer;

            public GPUHistogram(Shader shader, ComputeShader computeShader)
            {
                m_Shader = shader;
                m_ComputeShader = computeShader;
            }

            public void DoGUI(Rect rect, bool log, HistogramMode mode)
            {
                if (m_RT != null)
                    GUI.DrawTexture(rect, m_RT);
            }

            public void Update(RenderTexture source, Rect rect, bool log, HistogramMode mode)
            {
                if (m_Material == null)
                {
                    m_Material = new Material(m_Shader);
                    m_Material.hideFlags = HideFlags.HideAndDontSave;
                }

                if (m_Buffer == null)
                    m_Buffer = new ComputeBuffer(256, sizeof(uint) << 2);

                m_Buffer.SetData(new uint[256 << 2]);

                ComputeShader cs = m_ComputeShader;

                int kernel = cs.FindKernel("KHistogram");
                cs.SetBuffer(kernel, "_Histogram", m_Buffer);
                cs.SetTexture(kernel, "_Source", source);
                cs.SetInt("_IsLinear", (QualitySettings.activeColorSpace == ColorSpace.Linear) ? 1 : 0);
                cs.Dispatch(kernel, source.width >> 4, source.height >> 4, 1);

                kernel = cs.FindKernel(log ? "KScale_Log" : "KScale");
                cs.SetBuffer(kernel, "_Histogram", m_Buffer);
                cs.SetFloat("_Height", rect.height);
                cs.Dispatch(kernel, 1, 1, 1);

                if (m_RT == null || m_RT.height != rect.height || m_RT.width != rect.width)
                {
                    DestroyImmediate(m_RT);
                    m_RT = new RenderTexture((int)rect.width, (int)rect.height, 0, RenderTextureFormat.ARGB32);
                    m_RT.hideFlags = HideFlags.HideAndDontSave;
                }

                m_Material.SetBuffer("_Histogram", m_Buffer);
                m_Material.SetVector("_Size", new Vector2(m_RT.width, m_RT.height));
                m_Material.SetColor("_ColorR", s_RedCurveColor);
                m_Material.SetColor("_ColorG", s_GreenCurveColor);
                m_Material.SetColor("_ColorB", s_BlueCurveColor);
                m_Material.SetColor("_ColorL", s_MasterCurveColor);
                m_Material.SetInt("_Channel", (int)mode);
                Graphics.Blit(m_RT, m_RT, m_Material, (mode == HistogramMode.RGB) ? 1 : 0);
            }

            public void Destroy()
            {
                if (m_RT != null)
                    DestroyImmediate(m_RT);

                if (m_Material != null)
                    DestroyImmediate(m_Material);

                if (m_Buffer != null)
                    m_Buffer.Release();
            }
        }
    }
}
