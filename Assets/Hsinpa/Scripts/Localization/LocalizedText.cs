using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hsinpa
{
    /// <summary>
    /// Put on a GameObject with TMP_Text (or legacy Text) and set the key.
    /// The text updates on enable and whenever Loc.SetLanguage is called.
    /// </summary>
    [DisallowMultipleComponent]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField]
        private string key;

        private TMP_Text _tmpText;
        private Text _legacyText;
        private object[] _formatArgs;

        public string Key => key;

        private void Awake()
        {
            CacheTarget();
        }

        private void OnEnable()
        {
            Loc.OnLanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Loc.OnLanguageChanged -= Refresh;
        }

        /// <summary>Change the key at runtime. Optional args are used with string.Format ({0}, {1}...).</summary>
        public void SetKey(string newKey, params object[] args)
        {
            key = newKey;
            _formatArgs = args;
            Refresh();
        }

        /// <summary>Update format args only, keeping the current key.</summary>
        public void SetArgs(params object[] args)
        {
            _formatArgs = args;
            Refresh();
        }

        public void Refresh()
        {
            if (string.IsNullOrEmpty(key))
                return;

            CacheTarget();
            string value = (_formatArgs != null && _formatArgs.Length > 0)
                ? Loc.Get(key, _formatArgs)
                : Loc.Get(key);

            if (_tmpText != null)
                _tmpText.text = value;
            else if (_legacyText != null)
                _legacyText.text = value;
            else
                Debug.LogWarning($"[LocalizedText] No TMP_Text or Text on '{name}'.", this);
        }

        private void CacheTarget()
        {
            if (_tmpText == null && _legacyText == null)
            {
                _tmpText = GetComponent<TMP_Text>();
                if (_tmpText == null)
                    _legacyText = GetComponent<Text>();
            }
        }
    }
}
