using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace Hsinpa
{
    public class ControllerHandManager : MonoBehaviour
    {
        [SerializeField]
        private ControllerHandAnimator rightAnimator;

        [SerializeField]
        private ControllerHandAnimator leftAnimator;

        private XRIDefaultInputActions _xrInputActions;

        void Start()
        {
            this._xrInputActions = new XRIDefaultInputActions();
            this._xrInputActions.Enable();
            
            _xrInputActions.XRIRightInteraction.Activate.canceled += OnRightTriggerCancel;
            _xrInputActions.XRILeftInteraction.Activate.canceled += OnLeftTriggerCancel;

            Utility.SimpleEventSystem.CustomEventListener += OnCustomEvent;
        }

        void OnCustomEvent(string eventName, object[] parameters)
        {
            if (eventName != StaticEvent.TriggerEvent)
            {
                return;
            }

            string handState = (string) parameters[0];
            string gestureAnimation = (string) parameters[1];

            ControllerHandAnimator controllerHandAnimator = (handState == HandState.LEFTHAND) ? leftAnimator : rightAnimator;
            PlayEventAnim(controllerHandAnimator, gestureAnimation, true);
        }

        void PlayEventAnim(ControllerHandAnimator animator, string animationName, bool animationState)
        {
            animator.PlayAnimation(animationName, animationState);
        }

        void OnRightTriggerCancel(CallbackContext context)
        {
            rightAnimator.PlayAnimation("bComeHere", false);
            rightAnimator.PlayAnimation("bThumbsy", false);
        }

        void OnLeftTriggerCancel(CallbackContext context)
        {
            leftAnimator.PlayAnimation("bComeHere", false);
            leftAnimator.PlayAnimation("bThumbsy", false);
        }
    }    
}
