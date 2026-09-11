
using UnityEngine;

namespace Hsinpa.Utility.UI
{
    public static class UnityUIExtension
    {
        /// <summary>
        /// Show hide the CanvasGroup, depending on the given true or false/ value.
        /// </summary>
        public static void SetVisible(this CanvasGroup cg, bool visible)
        {
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }
    }
}