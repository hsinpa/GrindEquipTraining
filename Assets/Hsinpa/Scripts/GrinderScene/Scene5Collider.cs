using System;
using Hsinpa.Outline;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Hsinpa
{
    public class Scene5Collider : MonoBehaviour
    {
        public bool isCorrectAnswer;
        
        [SerializeField]
        private XRGrabInteractable xrGrabInteractable;
        
        [SerializeField]
        private MaskOutlineTarget maskOutlineTarget;

        private Rigidbody rigidbody;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private bool _isInteractable;

        public bool isInteractable
        {
            get {return this._isInteractable;}
            set
            {
                // if (maskOutlineTarget != null)
                // {   
                //     maskOutlineTarget.SetOutlined(value);
                // }
                this._isInteractable = value;
            }
        }

        public Action<Scene5Collider, bool> onColliderHoldEventCallback;
        public Action<Scene5Collider> onColliderPairedEventCallback;

        private Action<GameObject> onTouchGrinderEvent;
        private Action onLeaveGrinderEvent;

        public XRGrabInteractable GetInteractable() { return xrGrabInteractable; }

        public void SetUp()
        {   
            this.rigidbody = this.GetComponent<Rigidbody>();

            spawnPosition = transform.position;
            spawnRotation = transform.rotation;

            xrGrabInteractable.selectEntered.AddListener(OnColliderSelected);
            xrGrabInteractable.selectExited.AddListener(OnColliderUnselected);

            xrGrabInteractable.hoverEntered.AddListener((x) =>
            {
                if (isInteractable)  maskOutlineTarget?.SetOutlined(true);  
            });
            xrGrabInteractable.hoverExited.AddListener(
                (x) =>
                {
                    maskOutlineTarget?.SetOutlined(false);  
                } 
            );

        }

        public void StartSimulation(bool simulate, Action<GameObject> touchAction, Action leaveAction)
        {
            isInteractable = simulate;
            xrGrabInteractable.enabled = true;
            onTouchGrinderEvent = touchAction;
            onLeaveGrinderEvent = leaveAction;
        }

        public void Reset(bool hardReset = true)
        {
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            this.rigidbody.angularVelocity = Vector3.zero;
            this.rigidbody.linearVelocity = Vector3.zero;

            rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            if (hardReset || !isInteractable)
            {
                xrGrabInteractable.enabled = false;
                onTouchGrinderEvent = null;
                onLeaveGrinderEvent = null;
            }
        }

        private void OnColliderSelected(SelectEnterEventArgs a)
        {            
            rigidbody.constraints = RigidbodyConstraints.None;
            if (isInteractable) onColliderHoldEventCallback?.Invoke(this, true);
        }

        private void OnColliderUnselected(SelectExitEventArgs a)
        {
            if (isInteractable) onColliderHoldEventCallback?.Invoke(this, false);

            _ = Utility.UtilityFunc.DoDelayWork(5, () => Reset(false));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "GrindMachine")
            {
                onTouchGrinderEvent?.Invoke(collision.gameObject);
            }

            Debug.Log(collision.gameObject.name);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.tag == "GrindMachine")
            {
                onLeaveGrinderEvent?.Invoke();
            }
        }
    }
}