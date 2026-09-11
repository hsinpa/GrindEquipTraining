using UnityEngine;

namespace Hsinpa.Outline
{
    /// <summary>
    /// Marks this GameObject's renderers for the mask outline pass.
    /// Rendering Layer 31 is reserved by default; keep this value in sync with MaskOutlineRendererFeature.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaskOutlineTarget : MonoBehaviour
    {
        [SerializeField] private bool outlined = true;
        [SerializeField] private bool includeChildren = true;
        [SerializeField, Range(0, 31)] private int renderingLayer = 31;

        private Renderer[] targetRenderers;

        public bool IsOutlined => outlined;

        private uint RenderingLayerBit => 1u << renderingLayer;

        private void OnEnable()
        {
            RefreshRenderers();
            ApplyState();
        }

        private void OnDisable()
        {
            SetLayerBit(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            renderingLayer = Mathf.Clamp(renderingLayer, 0, 31);

            if (isActiveAndEnabled)
            {
                RefreshRenderers();
                ApplyState();
            }
        }
#endif

        /// <summary>Enables or disables the outline without disabling the object.</summary>
        public void SetOutlined(bool value)
        {
            outlined = value;

            if (isActiveAndEnabled)
                ApplyState();
        }

        /// <summary>Rebuilds the cached renderer list after children are added or removed.</summary>
        public void RefreshRenderers()
        {
            targetRenderers = includeChildren
                ? GetComponentsInChildren<Renderer>(true)
                : GetComponents<Renderer>();
        }

        private void ApplyState()
        {
            SetLayerBit(outlined);
        }

        private void SetLayerBit(bool value)
        {
            if (targetRenderers == null)
                return;

            uint bit = RenderingLayerBit;
            uint inverseBit = ~bit;

            foreach (Renderer targetRenderer in targetRenderers)
            {
                if (targetRenderer == null)
                    continue;

                targetRenderer.renderingLayerMask = value
                    ? targetRenderer.renderingLayerMask | bit
                    : targetRenderer.renderingLayerMask & inverseBit;
            }
        }
    }
}
