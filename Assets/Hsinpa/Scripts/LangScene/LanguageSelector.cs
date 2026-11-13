using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

namespace Hsinpa
{
    public class LanguageSelector: MonoBehaviour
    {
        [SerializeField]
        private Button chinese_btn;

        [SerializeField]
        private Button en_btn;

        [SerializeField]
        private Button id_btn;

        [SerializeField]
        private Button th_btn;

        [SerializeField]
        private Button vi_btn;

        [SerializeField]
        private Button confirm_btn;

        public Action onConfirmEvent;

        private void Start()
        {
            chinese_btn.onClick.AddListener(() => {OnLangBtnClick("zhcht"); });
            en_btn.onClick.AddListener(() => {OnLangBtnClick("en"); });
            id_btn.onClick.AddListener(() => {OnLangBtnClick("id"); });
            th_btn.onClick.AddListener(() => {OnLangBtnClick("th"); });
            vi_btn.onClick.AddListener(() => {OnLangBtnClick("vi"); });

            confirm_btn.onClick.AddListener(OnConfirm);
        }

        public void Show(bool is_show)
        {
            gameObject.SetActive(is_show);
        }

        private void OnLangBtnClick(string langcode)
        {
            Loc.SetLanguage(langcode);
        }

        private void OnConfirm()
        {
            onConfirmEvent?.Invoke();
            Show(false);
        }
    }
}
