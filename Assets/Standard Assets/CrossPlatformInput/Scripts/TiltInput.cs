using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityStandardAssets.CrossPlatformInput
{
    // helps with managing tilt input on mobile devices
    public class TiltInput : MonoBehaviour
    {
        // options for the various orientations
        public enum AxisOptions
        {
            ForwardAxis,
            SidewaysAxis
        }

        public float centreAngleOffset = 0;
        public float fullTiltAngle = 25;


        private CrossPlatformInputManager.VirtualAxis m_SteerAxis;


        public AxisMapping mapping;
        public AxisOptions tiltAroundAxis = AxisOptions.ForwardAxis;


        private void OnEnable()
        {
            if (mapping.type == AxisMapping.MappingType.NamedAxis)
            {
                m_SteerAxis = new CrossPlatformInputManager.VirtualAxis(mapping.axisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_SteerAxis);
            }
        }


        private void Update()
        {
            float angle = 0;
            if (Input.acceleration != Vector3.zero)
                switch (tiltAroundAxis)
                {
                    case AxisOptions.ForwardAxis:
                        angle = Mathf.Atan2(Input.acceleration.x, -Input.acceleration.y) * Mathf.Rad2Deg +
                                centreAngleOffset;
                        break;
                    case AxisOptions.SidewaysAxis:
                        angle = Mathf.Atan2(Input.acceleration.z, -Input.acceleration.y) * Mathf.Rad2Deg +
                                centreAngleOffset;
                        break;
                }

            var axisValue = Mathf.InverseLerp(-fullTiltAngle, fullTiltAngle, angle) * 2 - 1;
            switch (mapping.type)
            {
                case AxisMapping.MappingType.NamedAxis:
                    m_SteerAxis.Update(axisValue);
                    break;
                case AxisMapping.MappingType.MousePositionX:
                    CrossPlatformInputManager.SetVirtualMousePositionX(axisValue * Screen.width);
                    break;
                case AxisMapping.MappingType.MousePositionY:
                    CrossPlatformInputManager.SetVirtualMousePositionY(axisValue * Screen.width);
                    break;
                case AxisMapping.MappingType.MousePositionZ:
                    CrossPlatformInputManager.SetVirtualMousePositionZ(axisValue * Screen.width);
                    break;
            }
        }


        private void OnDisable()
        {
            m_SteerAxis.Remove();
        }


        [Serializable]
        public class AxisMapping
        {
            public enum MappingType
            {
                NamedAxis,
                MousePositionX,
                MousePositionY,
                MousePositionZ
            }

            public string axisName;


            public MappingType type;
        }
    }
}


namespace UnityStandardAssets.CrossPlatformInput.Inspector
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TiltInput.AxisMapping))]
    public class TiltInputAxisStylePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var x = position.x;
            var y = position.y;
            var inspectorWidth = position.width;

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var props = new[] { "type", "axisName" };
            var widths = new[] { .4f, .6f };
            if (property.FindPropertyRelative("type").enumValueIndex > 0)
            {
                // hide name if not a named axis
                props = new[] { "type" };
                widths = new[] { 1f };
            }

            const float lineHeight = 18;
            for (var n = 0; n < props.Length; ++n)
            {
                var w = widths[n] * inspectorWidth;

                // Calculate rects
                var rect = new Rect(x, y, w, lineHeight);
                x += w;

                EditorGUI.PropertyField(rect, property.FindPropertyRelative(props[n]), GUIContent.none);
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
#endif
}