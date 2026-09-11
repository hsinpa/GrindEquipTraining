using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static UnityEngine.InputSystem.InputAction;


namespace Hsinpa
{
    public class GrabCollider : MonoBehaviour
    {
        private bool _isSelected;
        private bool _isHovered;
        private bool interactable = false;
        private XRIDefaultInputActions _xrInputActions;
        private BoxCollider collider;
        private Transform _parent;
        public Action<bool> isSelectedCallback;


        [SerializeField]
        private XRSimpleInteractable xrInteractable;

        [SerializeField]
        private Transform anchorPoint;

        void Awake()
        {
            collider =  xrInteractable.GetComponent<BoxCollider>();

            this._xrInputActions = new XRIDefaultInputActions();
            this._xrInputActions.Enable();
            this._parent = this.transform.parent;
        }

        public void Interaction(bool enable)
        {
            interactable = enable;
            collider.enabled = enable;
        }

        void Start()
        {
            _xrInputActions.XRIRightInteraction.Activate.started += OnColliderSelected;
            _xrInputActions.XRILeftInteraction.Activate.started += OnColliderSelected;

            _xrInputActions.XRIRightInteraction.Activate.canceled += OnColliderDeselected;
            _xrInputActions.XRILeftInteraction.Activate.canceled += OnColliderDeselected;

            xrInteractable.hoverEntered.AddListener(x => _isHovered = true);
            xrInteractable.hoverExited.AddListener(x => {
                if (!_isSelected)
                {
                }
                _isHovered = false;                    
            });
        }

        private void OnColliderSelected(CallbackContext a)
        {
            if (_isHovered)
            {
                _isSelected = true;
                if (isSelectedCallback != null && interactable)
                {
                    isSelectedCallback?.Invoke(true);
                }                
            }
        }

        private void OnColliderDeselected(CallbackContext a)
        {
            if (_isSelected)
            {
                isSelectedCallback?.Invoke(false);
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