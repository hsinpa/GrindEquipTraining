using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Hsinpa
{
    /// <summary>
    /// Static UI text localization.
    /// Each language is one JSON file at Resources/Localization/{languageCode}.json,
    /// containing a flat { "key": "text" } object. The file name is the language code.
    /// Lookup order: current language -> FallbackLanguage -> "#key#".
    /// </summary>
    public static class Loc
    {
        public const string FallbackLanguage = "en";
        public const string DefaultLanguage = "zhcht";   // used when no language was saved yet

        private const string ResourceFolder = "Localization";
        private const string PrefsKey = "Hsinpa.Loc.Language";

        private static readonly Dictionary<string, Dictionary<string, string>> _tables =
            new Dictionary<string, Dictionary<string, string>>();
        private static List<string> _availableLanguages;
        private static string _currentLanguage;

        public static event Action OnLanguageChanged;

        public static string CurrentLanguage
        {
            get { EnsureInit(); return _currentLanguage; }
        }

        /// <summary>Language codes found in Resources/Localization (file names without extension).</summary>
        public static IReadOnlyList<string> AvailableLanguages
        {
            get { EnsureInit(); return _availableLanguages; }
        }

        public static string Get(string key)
        {
            return TryGet(key, out string value) ? value : $"#{key}#";
        }

        public static string Get(string key, params object[] args)
        {
            string format = Get(key);
            if (args == null || args.Length == 0)
                return format;

            try
            {
                return string.Format(format, args);
            }
            catch (FormatException e)
            {
                Debug.LogWarning($"[Loc] Format error for key '{key}' in '{CurrentLanguage}': {e.Message}");
                return format;
            }
        }

        public static bool TryGet(string key, out string value)
        {
            EnsureInit();
            value = null;
            if (string.IsNullOrEmpty(key))
                return false;

            if (GetTable(_currentLanguage).TryGetValue(key, out value))
                return true;

            if (_currentLanguage != FallbackLanguage && GetTable(FallbackLanguage).TryGetValue(key, out value))
                return true;

            Debug.LogWarning($"[Loc] Missing key '{key}' (language '{_currentLanguage}', fallback '{FallbackLanguage}').");
            return false;
        }

        public static bool HasLanguage(string languageCode)
        {
            EnsureInit();
            return _availableLanguages.Contains(languageCode);
        }

        /// <summary>Switch language, save it to PlayerPrefs, and notify all LocalizedText.</summary>
        public static bool SetLanguage(string languageCode)
        {
            EnsureInit();
            if (!HasLanguage(languageCode))
            {
                Debug.LogWarning($"[Loc] Language '{languageCode}' not found. Available: {string.Join(", ", _availableLanguages)}");
                return false;
            }

            if (_currentLanguage == languageCode)
                return true;

            _currentLanguage = languageCode;
            PlayerPrefs.SetString(PrefsKey, languageCode);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
            return true;
        }

        private static void EnsureInit()
        {
            if (_availableLanguages != null)
                return;

            _availableLanguages = new List<string>();
            foreach (TextAsset asset in Resources.LoadAll<TextAsset>(ResourceFolder))
            {
                _availableLanguages.Add(asset.name);
                Resources.UnloadAsset(asset);
            }

            if (_availableLanguages.Count == 0)
                Debug.LogError($"[Loc] No language JSON found in Resources/{ResourceFolder}/.");

            string saved = PlayerPrefs.GetString(PrefsKey, DefaultLanguage);
            if (_availableLanguages.Contains(saved))
                _currentLanguage = saved;
            else if (_availableLanguages.Contains(DefaultLanguage))
                _currentLanguage = DefaultLanguage;
            else
                _currentLanguage = FallbackLanguage;
        }

        private static Dictionary<string, string> GetTable(string languageCode)
        {
            if (_tables.TryGetValue(languageCode, out var table))
                return table;

            table = new Dictionary<string, string>();
            TextAsset asset = Resources.Load<TextAsset>($"{ResourceFolder}/{languageCode}");
            if (asset != null)
            {
                try
                {
                    table = JsonConvert.DeserializeObject<Dictionary<string, string>>(asset.text)
                            ?? new Dictionary<string, string>();
                }
                catch (JsonException e)
                {
                    Debug.LogError($"[Loc] Failed to parse {languageCode}.json: {e.Message}");
                }
                Resources.UnloadAsset(asset);
            }

            _tables[languageCode] = table;
            return table;
        }

        // Reset static state when entering play mode with domain reload disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _tables.Clear();
            _availableLanguages = null;
            _currentLanguage = null;
            OnLanguageChanged = null;
        }
    }
}
