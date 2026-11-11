using System;
using Hsinpa.Outline;
using Hsinpa.Utility;
using Hsinpa.Utility.UI;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Hsinpa
{
    public class Scene4Collider : MonoBehaviour
    {
        public bool isCorrectAnswer;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private TextMeshProUGUI hintText;
        
        [SerializeField]
        private XRGrabInteractable xrGrabInteractable;
        
        [SerializeField]
        private MaskOutlineTarget maskOutlineTarget;

        private Rigidbody rigidbody;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        private bool _isSelected;
        private bool _isInteractable = false;

        public Action<Scene4Collider, bool> onColliderHoldEventCallback;
        public Action<Scene4Collider> onColliderPairedEventCallback;

        public XRGrabInteractable GetInteractable() { return xrGrabInteractable; }

        private bool countDownStart;

        public void StartCount(int beginCount, bool forceStart)
        {
            if (forceStart) countDownStart = true;
            
            if (beginCount < 0 || !countDownStart)
            {
                if (hintText != null) hintText.text = ""; 
                countDownStart = false;
                return;
            }

            ShowItemText(this.gameObject.name);     
        }

        private void Start()
        {   
            this.rigidbody = this.GetComponent<Rigidbody>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            xrGrabInteractable.enabled = false;

            xrGrabInteractable.selectEntered.AddListener(OnColliderSelected);
            xrGrabInteractable.selectExited.AddListener(OnColliderUnselected);

            xrGrabInteractable.hoverEntered.AddListener((x) =>
            {
                maskOutlineTarget?.SetOutlined(true);  
            });
            xrGrabInteractable.hoverExited.AddListener(
                (x) =>
                {
                    maskOutlineTarget?.SetOutlined(false);  
                } 
            );
        }


        private void ShowItemText(string itemName)
        {
            if (hintText == null) return;

            if (itemName == "n95")
            {
                hintText.text= Loc.Get("N95_NOTICE");
            }

            if (itemName == "dustMask")
            {
                hintText.text= Loc.Get("NORMAL_MASK_NOTICE");
            }

            if (itemName == "gasMask")
            {
                hintText.text= Loc.Get("HALF_MASK_NOTICE");
            }
        }

        public void StartSimulation(bool simulate)
        {
            _isInteractable = simulate;
            xrGrabInteractable.enabled = true;
        }

        public void ShowHint(bool show)
        {
            if (canvasGroup != null) canvasGroup.SetVisible(show);
        }

        public void Reset()
        {
            ShowHint(false);
            countDownStart = false;
            _isSelected = false;
            if (hintText != null) hintText.text = ""; 
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            this.rigidbody.angularVelocity = Vector3.zero;
            this.rigidbody.linearVelocity = Vector3.zero;

            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            
            xrGrabInteractable.enabled = false;
            _ = Utility.UtilityFunc.DoDelayWork(1, () =>
            {
                if (_isInteractable) xrGrabInteractable.enabled = true;
            });
        }

        private static string GetHand(IXRSelectInteractor interactor)
        {
            string interactorName = interactor.transform.gameObject.name;

            if (interactorName.Contains("Left", StringComparison.OrdinalIgnoreCase)) return HandState.LEFTHAND;
            if (interactorName.Contains("Right", StringComparison.OrdinalIgnoreCase)) return HandState.RIGHTHAND;

            return "Unknown";
        }

        private void OnColliderSelected(SelectEnterEventArgs a)
        {
            if (!_isInteractable) return;

            string handStr = GetHand(a.interactorObject);
            SimpleEventSystem.Send(StaticEvent.TriggerEvent, handStr, GestureState.GRAB_MASK_EVENT);
            
            _isSelected = true;
            ShowHint(true);
            
            rigidbody.constraints = RigidbodyConstraints.None;
            onColliderHoldEventCallback?.Invoke(this, true);
        }

        private void OnColliderUnselected(SelectExitEventArgs a)
        {
            _isSelected = false;
            if (_isInteractable) onColliderHoldEventCallback?.Invoke(this, false);
            ShowHint(false);

            _ = Utility.UtilityFunc.DoDelayWork(5, () => Reset());
        }
    }
}