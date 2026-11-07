using System;
using Hsinpa.Outline;
using Hsinpa.Utility;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using static UnityEngine.InputSystem.InputAction;


namespace Hsinpa
{
    public class Scene2Collider : MonoBehaviour
    {
        static readonly int ColliderAlphaStrengthID = Shader.PropertyToID("_Strength");
        public Action<bool> isSelectedCallback;

        [SerializeField]
        MeshRenderer meshRenderer;

        [SerializeField]
        private XRSimpleInteractable xrInteractable;

        [SerializeField]
        private MaskOutlineTarget maskOutlineTarget;

        [SerializeField]
        private bool isCorrect;

        private BoxCollider collider;

        private MaterialPropertyBlock _propertyBlock;

        private bool _isSelected;
        private bool _isHovered;
        private bool interactable = false;
        private XRIDefaultInputActions _xrInputActions;

        void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            collider =  xrInteractable.GetComponent<BoxCollider>();

            this._xrInputActions = new XRIDefaultInputActions();
            this._xrInputActions.Enable();
        }

        public void Interaction(bool enable)
        {
            interactable = enable;
            collider.enabled = enable;
        }

        private static string GetHand(IXRInteractor interactor)
        {
            string interactorName = interactor.transform.parent.gameObject.name;

            Debug.Log(interactorName);

            if (interactorName.Contains("Left", StringComparison.OrdinalIgnoreCase)) return HandState.LEFTHAND;
            if (interactorName.Contains("Right", StringComparison.OrdinalIgnoreCase)) return HandState.RIGHTHAND;

            return "Unknown";
        }

        void Start()
        {
            _xrInputActions.XRIRightInteraction.Activate.started += (x) => { OnColliderSelected(x, HandState.RIGHTHAND); };
            _xrInputActions.XRIRightInteraction.Activate.canceled += OnColliderUnselected;
            _xrInputActions.XRILeftInteraction.Activate.started += (x) => { OnColliderSelected(x, HandState.LEFTHAND); };
            _xrInputActions.XRILeftInteraction.Activate.canceled += OnColliderUnselected;

            xrInteractable.hoverEntered.AddListener(x =>
            {
                _isHovered = true;
                SimpleEventSystem.Send(StaticEvent.TriggerEvent, GetHand(x.interactorObject), GestureState.POINT_EVENT);    
            });

            xrInteractable.hoverExited.AddListener(x => {
                if (!_isSelected)
                {
                    SetMaterialFloat(ColliderAlphaStrengthID, 0);
                }
                _isHovered = false;                    
                SimpleEventSystem.Send(StaticEvent.TriggerEvent, GetHand(x.interactorObject), GestureState.DEFAULT);
            });

            Reset();
        }

        void Update()
        {
            if (_isSelected && interactable)
            {
                _isHovered = false;
                float pulse = Mathf.Lerp(0.2f, 0.5f, (Mathf.Sin(Time.time * 2f) + 1f) * 0.5f);
                SetMaterialFloat(ColliderAlphaStrengthID, pulse);

                return;
            }

            if (_isHovered && interactable)
            {
                float pulse = Mathf.Lerp(0f, 0.1f, (Mathf.Sin(Time.time * 2f) + 1f) * 0.5f);
                SetMaterialFloat(ColliderAlphaStrengthID, pulse);
                maskOutlineTarget?.SetOutlined(true);
                return;
            }

            maskOutlineTarget?.SetOutlined(false);
        }

        private void OnColliderSelected(CallbackContext a, string hand_state)
        {
            if (_isHovered)
            {
                _isSelected = true;
                if (isSelectedCallback != null && interactable)
                {
                    Interaction(false);
                    isSelectedCallback(isCorrect);
                    SetMaterialFloat(ColliderAlphaStrengthID, 0.5f);

                    string audioID = (isCorrect) ? Audio.UI_CLICK : Audio.WRONG_CHOICE;
                    UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, audioID);
                }                
            }
        }

        private void OnColliderUnselected(CallbackContext a)
        {
        }

        public void SetMaterialColor(int propertyID, Color color)
        {
            meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(propertyID, color);
            meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetMaterialFloat(int propertyID, float value)
        {
            meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(propertyID, value);
            meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void Reset()
        {
            _isHovered = false;
            _isSelected = false;
            Interaction(false);
            SetMaterialFloat(ColliderAlphaStrengthID, 0f);
        }
    }
}
