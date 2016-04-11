using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    public partial class AmbientOcclusion : MonoBehaviour
    {
        /// Values for Settings.sampleCount, determining the number of sample points.
        public enum SampleCount
        {
            Lowest, Low, Medium, High, Variable
        }

        /// Class used for storing settings of AmbientOcclusion.
        [Serializable]
        public class Settings
        {
            /// Degree of darkness produced by the effect.
            [SerializeField, Range(0, 4)]
            [Tooltip("Degree of darkness produced by the effect.")]
            public float intensity;

            /// Radius of sample points, which affects extent of darkened areas.
            [SerializeField]
            [Tooltip("Radius of sample points, which affects extent of darkened areas.")]
            public float radius;

            /// Number of sample points, which affects quality and performance.
            [SerializeField]
            [Tooltip("Number of sample points, which affects quality and performance.")]
            public SampleCount sampleCount;

            /// Determines the sample count when SampleCount.Variable is used.
            [SerializeField]
            [Tooltip("Determines the sample count when SampleCount.Variable is used.")]
            public int sampleCountValue;

            /// Number of iterations of blur filter.
            [SerializeField, Range(0, 4)]
            [Tooltip("Number of iterations of the blur filter.")]
            public int blurIterations;

            /// Halves the resolution of the effect to increase performance.
            [SerializeField]
            [Tooltip("Halves the resolution of the effect to increase performance.")]
            public bool downsampling;

            /// Enables the ambient-only mode in that the effect only affects
            /// ambient lighting. This mode is only available with deferred
            /// shading and HDR rendering.
            [SerializeField]
            [Tooltip("If checked, the effect only affects ambient lighting.")]
            public bool ambientOnly;

            [SerializeField]
            public bool debug;

            /// Returns the default settings.
            public static Settings defaultSettings
            {
                get
                {
                    return new Settings
                    {
                        intensity = 1,
                        radius = 0.3f,
                        sampleCount = SampleCount.Medium,
                        sampleCountValue = 24,
                        blurIterations = 2,
                        downsampling = false,
                        ambientOnly = false
                    };
                }
            }
        }
    }
}
