using CsvHelper.Configuration.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


namespace Hsinpa.UI
{
    public class MessageHolder : MonoBehaviour
    {
        private Image msg_border;

        [SerializeField]
        private Image msg_icon;

        [SerializeField]
        private TextMeshProUGUI title;

        [SerializeField]
        private TextMeshProUGUI content;

        [Header("Icon sprites")]
        [SerializeField]
        private Sprite info_icon;

        [SerializeField]
        private Sprite victory_icon;

        [SerializeField]
        private Sprite error_icon;

        [Header("Border sprites")]
        [SerializeField]
        private Sprite info_border;

        [SerializeField]
        private Sprite victory_border;

        [SerializeField]
        private Sprite error_border;

        public enum IconState
        {
            Info = 0, Victory, Error, None
        }

        private RectTransform rectTransform;
        private void Start()
        {
            msg_border = this.GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Reset()
        {
            msg_icon.gameObject.SetActive(false);
            title.gameObject.SetActive(false);
            content.text = "";
        }

        public void SetContent(string content_str, string title_str = null, IconState icon_state = IconState.None)
        {
            Reset();

            content.text = content_str;

            if (title_str != null)
            {
                title.gameObject.SetActive(true);
                title.text = title_str;                
            }

            SetStyleTheme(icon_state);
            
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        private void SetStyleTheme(IconState icon_state)
        {
            if (icon_state == IconState.None)
            {
                msg_icon.gameObject.SetActive(false);
                return;
            }

            msg_icon.gameObject.SetActive(true);

            switch(icon_state) {
                case IconState.Info:
                    {
                        msg_icon.sprite = info_icon;
                        msg_border.sprite = info_border;
                        break;
                    }
                case IconState.Error:
                    {
                        msg_icon.sprite = error_icon;
                        msg_border.sprite = error_border;
                        break;
                    }
                case IconState.Victory:
                    {
                        msg_icon.sprite = victory_icon;
                        msg_border.sprite = victory_border;
                        break;
                    }
                default:
                    return;
            }
        }
    }
}