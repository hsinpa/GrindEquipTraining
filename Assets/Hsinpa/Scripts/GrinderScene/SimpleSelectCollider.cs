using System;
using Hsinpa.Outline;
using Hsinpa.Utility;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static UnityEngine.InputSystem.InputAction;


namespace Hsinpa
{
    public class SimpleSelectCollider : MonoBehaviour
    {
        private bool _isSelected;
        private bool _isHovered;
        private bool interactable = false;
        private XRIDefaultInputActions _xrInputActions;
        private BoxCollider collider;
        public Action isSelectedCallback;

        [SerializeField]
        private XRSimpleInteractable xrInteractable;
        
        [SerializeField]
        private MaskOutlineTarget maskOutlineTarget;

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
                    Interaction(false);
                    isSelectedCallback?.Invoke();

                    SimpleEventSystem.Send(StaticEvent.TriggerEvent, hand_state, GestureState.POINT_EVENT);
                    UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.UI_CLICK);
                }                
            }
        }

        public void Reset()
        {
            _isHovered = false;
            _isSelected = false;
            Interaction(false);
        }
    }
}