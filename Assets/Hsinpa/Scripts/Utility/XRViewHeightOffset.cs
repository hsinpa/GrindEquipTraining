using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

namespace Hsinpa
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(XROrigin))]
    public class XRViewHeightOffset : MonoBehaviour
    {
        [Tooltip("Additional tracked view height in meters. Negative values lower the camera and hands.")]
        [SerializeField] private float heightOffset = -0.2f;

        private XROrigin origin;

        private void Awake()
        {
            origin = GetComponent<XROrigin>();
        }

        private void LateUpdate()
        {
            ApplyOffset(heightOffset);
        }

        private void OnDisable()
        {
            ApplyOffset(0f);
        }

        private void ApplyOffset(float offset)
        {
            if (origin == null || origin.CameraFloorOffsetObject == null)
                return;

            var mode = origin.CurrentTrackingOriginMode;
            if (mode == TrackingOriginModeFlags.Unknown)
                return;

            // XROrigin resets this height when tracking initializes or recenters.
            // Apply an absolute value so the adjustment never accumulates.
            float baseHeight = mode == TrackingOriginModeFlags.Floor ? 0f : origin.CameraYOffset;
            var target = origin.CameraFloorOffsetObject.transform;
            var position = target.localPosition;
            position.y = baseHeight + offset;
            target.localPosition = position;
        }
    }
}
