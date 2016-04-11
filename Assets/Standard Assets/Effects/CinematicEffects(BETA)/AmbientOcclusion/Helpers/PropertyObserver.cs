using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    public partial class AmbientOcclusion : MonoBehaviour
    {
        // Observer class that detects changes on properties
        struct PropertyObserver
        {
            // AO properties
            int _blurIterations;
            bool _downsampling;
            bool _ambientOnly;

            // Camera properties
            int _pixelWidth;
            int _pixelHeight;

            // Check if it has to reset itself for property changes.
            public bool CheckNeedsReset(Settings setting, Camera camera)
            {
                return
                    _blurIterations != setting.blurIterations ||
                    _downsampling != setting.downsampling ||
                    _ambientOnly != setting.ambientOnly ||
                    _pixelWidth != camera.pixelWidth ||
                    _pixelHeight != camera.pixelHeight;
            }

            // Update the internal state.
            public void Update(Settings setting, Camera camera)
            {
                _blurIterations = setting.blurIterations;
                _downsampling = setting.downsampling;
                _ambientOnly = setting.ambientOnly;
                _pixelWidth = camera.pixelWidth;
                _pixelHeight = camera.pixelHeight;
            }
        }
    }
}
