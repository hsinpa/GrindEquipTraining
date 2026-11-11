using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Hsinpa
{
    public class LanguageSelector: MonoBehaviour
    {
        const string sceneToLoad = "Scenes/防護罩";

        [SerializeField]
        private RectTransform btn_group;

        [SerializeField]
        private Button chinese_btn;

        [SerializeField]
        private Button en_btn;

        [SerializeField]
        private TextMeshProUGUI loading_text;

        private void Start()
        {
            chinese_btn.onClick.AddListener(() => {OnLangBtnClick("zhcht"); });
            en_btn.onClick.AddListener(() => {OnLangBtnClick("en"); });
        }

        private void OnLangBtnClick(string langcode)
        {
            btn_group.gameObject.SetActive(false);
            loading_text.gameObject.SetActive(true);
            Loc.SetLanguage(langcode);

            _ = Utility.UtilityFunc.DoDelayWork(1, () =>
            {
                var op = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);

                op.completed += (AsyncOperation obj) =>
                {
                    Scene loadedScene = SceneManager.GetSceneByPath(sceneToLoad);
                    Debug.Log($"{sceneToLoad} finished loading (build index: {loadedScene.buildIndex}).");
                    Debug.Log($"It has {loadedScene.rootCount} root(s).");
                    Debug.Log($"There are now {SceneManager.loadedSceneCount} Scenes open.");
                }; 
            });
        }
    }
}
