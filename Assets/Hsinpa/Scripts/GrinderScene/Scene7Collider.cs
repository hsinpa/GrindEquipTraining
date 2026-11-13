using UnityEngine;
using UnityEngine.UI;
using System;
using Hsinpa.Outline;
using Hsinpa.Utility;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static UnityEngine.InputSystem.InputAction;

namespace Hsinpa
{
    public class Scene7Collider : MonoBehaviour
    {
        [SerializeField]
        private Sprite checkSprite;

        [SerializeField]
        private Sprite crossSprite;

        [SerializeField]
        private Image indicatorImage;

        [SerializeField]
        private XRSimpleInteractable xrInteractable;
        
        [SerializeField]
        private MaskOutlineTarget maskOutlineTarget;

        private bool _isSelected;
        private bool _isHovered;
        private bool interactable = false;
        private XRIDefaultInputActions _xrInputActions;
        private BoxCollider collider;
        public Action isSelectedCallback;

        void Awake()
        {
            collider =  xrInteractable.GetComponent<BoxCollider>();

            this._xrInputActions = new XRIDefaultInputActions();
            this._xrInputActions.Enable();
        }

        public Transform GetParentObject()
        {
            return maskOutlineTarget?.transform;
        }

        public void Interaction(bool enable)
        {
            interactable = enable;
            collider.enabled = enable;
        }

        public void ShowIndicator(bool is_show, bool is_correct = false)
        {
            indicatorImage.enabled = is_show;
            indicatorImage.sprite = is_correct ? checkSprite : crossSprite;
        }

        void Start()
        {
            // xrInteractable.hoverEntered.AddListener(a => Debug.Log("Ray hovering"));
            _xrInputActions.XRIRightInteraction.Activate.started += (x) => { OnColliderSelected(x, HandState.RIGHTHAND); };
            _xrInputActions.XRILeftInteraction.Activate.started += (x) => { OnColliderSelected(x, HandState.LEFTHAND); };

            xrInteractable.hoverEntered.AddListener(x =>
            {
                if (maskOutlineTarget != null && interactable) maskOutlineTarget.SetOutlined(true);
                _isHovered = true;
            });
            xrInteractable.hoverExited.AddListener(x => {
                if (maskOutlineTarget != null) maskOutlineTarget.SetOutlined(false);
                _isHovered = false;                    
            });
        }

        private void OnColliderSelected(CallbackContext a, string hand_state)
        {
            if (_isHovered)
            {
                _isSelected = true;
                if (isSelectedCallback != null && interactable)
                {
                    isSelectedCallback?.Invoke();

                    SimpleEventSystem.Send(StaticEvent.TriggerEvent, hand_state, GestureState.POINT_EVENT);
                }                
            }
        }

        public void Reset()
        {
            _isHovered = false;
            _isSelected = false;
            indicatorImage.enabled = false;
            Interaction(false);
        }
    }
}