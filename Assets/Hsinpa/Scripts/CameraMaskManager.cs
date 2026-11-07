using UnityEngine;
using DG.Tweening;
using TMPro;
using Hsinpa.Utility.UI;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using static UnityEngine.InputSystem.InputAction;
using Hsinpa.Utility;

namespace Hsinpa
{
    public class CameraMaskManager :  MonoBehaviour, ISceneManager
    {
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private TextMeshProUGUI textUI;

        [SerializeField]
        private TextMeshProUGUI titleUI;

        [SerializeField]
        private VideoPlayer videoPlayer;

        [SerializeField]
        private RawImage videoRawImage;

        [SerializeField]
        private GameObject countDownObj;

        [SerializeField]
        private TextMeshProUGUI countDownText;

        private Action OnVideoEndEvent;
        private Action nextPageEvent;
        private XRIDefaultInputActions _xrInputActions;

        private void Start()
        {
            this._xrInputActions = new XRIDefaultInputActions();
            this._xrInputActions.Enable();

            _xrInputActions.XRIRightInteraction.Activate.started += OnCtrlTriggerActivate;
            _xrInputActions.XRILeftInteraction.Activate.started += OnCtrlTriggerActivate;
        }


        private void OnEnable()
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        private void OnDisable()
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }

        public void Dispose()
        {
            Display(false);

            videoPlayer.Stop();
            videoRawImage.enabled = false;
        }

        public void Enter()
        {
            Display(true);
        }

        public void SetText(string title, string content)
        {
            titleUI.gameObject.SetActive(!string.IsNullOrEmpty(title));

            if (!string.IsNullOrEmpty(title)) titleUI.text = title;

            textUI.text = content;
        }

        public async void SetCountDown(int duration, Action callback)
        {
            countDownObj.SetActive(true);
            nextPageEvent = callback;

            // await TextCountdown.Countdown(countDownText,  "({0}秒)", duration);

            // callback?.Invoke();
        }

        public void PlayVideo(VideoClip videoClip, Action endCallback)
        {
            videoPlayer.clip = videoClip;
            videoPlayer.Play();
            videoRawImage.enabled = true;
            
            OnVideoEndEvent = endCallback;
        }

        private void OnCtrlTriggerActivate(CallbackContext e)
        {
            if (textUI.enabled) UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.UI_CLICK); 

            nextPageEvent?.Invoke();
            nextPageEvent = null;
        }

        private void OnVideoFinished(VideoPlayer source)
        {
            // Confirm this is the expected player.
            if (source != videoPlayer)
                return;

            OnVideoEndEvent?.Invoke();
        }

        private void Display(bool is_show)
        {
            int AlphaStrengthID = Shader.PropertyToID("_AlphaStrength");
            meshRenderer.material.DOFloat(is_show ? 1 : 0, AlphaStrengthID, 0.5f);
            canvasGroup.SetVisible(is_show);

            // Empty text content
            SetText("", "");
            textUI.enabled = is_show;

            countDownObj.SetActive(false);
        }
    }
}