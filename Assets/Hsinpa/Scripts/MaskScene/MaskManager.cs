using System;
using Hsinpa.UI;
using Hsinpa.Utility;
using Hsinpa.Utility.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace Hsinpa
{
    public class MaskManager : MonoBehaviour, ISceneManager
    {
        [SerializeField]
        private CanvasGroup uiCavnas;

        [SerializeField]
        private MessageHolder messaegeHolder;
        
        [SerializeField]
        private Scene4Collider[] mouthColliders;

        [SerializeField]
        private Scene4Collider[] eyeMaskColliders;


        private Scene4Collider selectedScene4Collider;
        private CameraCollider _cameraTransform;

        public Action completeCallback;
        private CaptionCameraCtrl _captionCtrl;
        public bool completeFlag = false;
        private bool eyeMaskFlag = false;
        private bool mouthMaskFlag = false;
        public void Setup(Transform cameraTransform, CaptionCameraCtrl captionCtrl)
        {
            foreach(var collider in mouthColliders)
            {
                collider.onColliderHoldEventCallback += OnMouthMaskHoldColliderEvent;
                collider.onColliderPairedEventCallback += OnMouthMaskPairedColliderEvent;
            }

            foreach(var collider in eyeMaskColliders)
            {
                collider.onColliderHoldEventCallback += OnEyeMaskHoldColliderEvent;
            }

            this._cameraTransform = cameraTransform.GetComponent<CameraCollider>();
            this._cameraTransform.onCollideEvent += OnScene4HoldOnTriggerEvent;
            this._captionCtrl = captionCtrl;
        }

        public void Dispose()
        {
            Reset();
        }

        public void Enter()
        {
            Reset();
            completeFlag = false;
            eyeMaskFlag = false;
            mouthMaskFlag = false;

            uiCavnas.SetVisible(true);
            messaegeHolder.SetContent(StaticString.SCENE4NOTICE, StaticString.SCENE4TITLE, MessageHolder.IconState.Info);
            this._captionCtrl.Play(LocalizationManager.WorkSheet.Cut4_Begin, () =>
            {
                uiCavnas.SetVisible(false);
            });

            foreach(var collider in mouthColliders)
            {
                collider.StartSimulation(true);
            }

            foreach(var collider in eyeMaskColliders)
            {
                collider.StartSimulation(true);
            }
        }

        public void End()
        {
            Reset(false);
            completeFlag = true;

            UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.COMPLETE); 
        }

        private void OnMouthMaskHoldColliderEvent(Scene4Collider collider, bool isSelected)
        {
            selectedScene4Collider = (isSelected) ? collider : null;

            if (isSelected)
            {
                if (!collider.isCorrectAnswer)
                {
                    uiCavnas.SetVisible(true);

                    if (collider.name == "dustMask")
                    {
                        messaegeHolder.SetContent(StaticString.NORMAL_MASK_NOTICE, StaticString.NORMAL_MASK_TITLE, MessageHolder.IconState.Error);
                    } else if (collider.name == "gasMask")
                    {
                        messaegeHolder.SetContent(StaticString.HALF_MASK_NOTICE, StaticString.HALF_MASK_TITLE, MessageHolder.IconState.Error);                    
                    }

                    this._captionCtrl.Play(LocalizationManager.WorkSheet.Cut4_Mistake, () => { uiCavnas.SetVisible(false);});

                } else if (collider.gameObject.activeSelf && collider.isCorrectAnswer)
                {
                    uiCavnas.SetVisible(true);
                    messaegeHolder.SetContent(StaticString.N95_NOTICE, StaticString.N95_TITLE, MessageHolder.IconState.Info);
                    this._captionCtrl.Play(LocalizationManager.WorkSheet.Cut4_Complete,  () => { uiCavnas.SetVisible(false);});
                }
            } else
            {
                collider.StartCount( beginCount: 5, forceStart: true);                
            }
        }


        private void OnEyeMaskHoldColliderEvent(Scene4Collider collider, bool isSelected)
        {
            if (isSelected)
            {
                messaegeHolder.SetContent(StaticString.SAFE_GLASS_MASK_NOTICE, StaticString.SAFE_GLASS_MASK_TITLE, MessageHolder.IconState.Victory);
                UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.COMPLETE);                
            } else
            {
                messaegeHolder.SetContent(StaticString.SCENE4NOTICE, StaticString.SCENE4TITLE, MessageHolder.IconState.Info);
            }
        }

        private void OnMouthMaskPairedColliderEvent(Scene4Collider collider)
        {
            if (collider.isCorrectAnswer)
            {
                completeCallback?.Invoke();
            }            
        }

        private void OnScene4HoldOnTriggerEvent(Scene4Collider collider)
        {
            UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.WEAR);

            if (collider.tag == "EyeMask")
            {
                collider.gameObject.SetActive(false);  
                eyeMaskFlag = true;
                
                if (eyeMaskFlag && mouthMaskFlag)
                {
                    uiCavnas.SetVisible(true);
                    messaegeHolder.SetContent(StaticString.SCENE4COMPLETE, StaticString.SCENE4TITLE, MessageHolder.IconState.Victory);
                    completeCallback?.Invoke();
                }

                return;
            }

            if (collider.isCorrectAnswer)
            {
                collider.gameObject.SetActive(false); // Hide the mask from user  
                mouthMaskFlag = true;

                if (eyeMaskFlag && mouthMaskFlag)
                {
                    uiCavnas.SetVisible(true);
                    messaegeHolder.SetContent(StaticString.SCENE4COMPLETE, StaticString.SCENE4TITLE, MessageHolder.IconState.Victory);
                    completeCallback?.Invoke();
                } 
            } else
            {
                collider.Reset();
            }
        }

        private void Reset(bool fullreset = true)
        {
            if (fullreset)
            {
                uiCavnas.SetVisible(false);
                messaegeHolder.SetContent("");             
            }

            foreach(var collider in mouthColliders)
            {
                collider.Reset();
            }
            
            selectedScene4Collider = null;
        }

    }
}