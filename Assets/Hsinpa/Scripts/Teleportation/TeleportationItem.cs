using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace Hsinpa
{
    public class TeleportationItem : MonoBehaviour
    {
        private TeleportationAnchor teleportationAnchor;
        public Action InPositionCallback;
        
        private Transform cameraTarget;
        private BoxCollider boxCollider;
        private bool collideFlag = false;

        public void SetTarget(Transform cameraTarget)
        {
            this.cameraTarget = cameraTarget;
        }

        private void Start()
        {
            this.teleportationAnchor = this.GetComponentInParent<TeleportationAnchor>();
            this.teleportationAnchor.teleporting.AddListener(OnTeleportEvent);

            this.boxCollider = GetComponent<BoxCollider>();
        }

        private void OnTeleportEvent(TeleportingEventArgs teleportingEventArgs)
        {
            InPositionCallback?.Invoke();
        }

        private void Update()
        {
            if (this.cameraTarget != null)
            {   
                float distance = Mathf.Sqrt(
                    Mathf.Pow( this.cameraTarget.position.x - this.transform.position.x, 2) + 
                    Mathf.Pow( this.cameraTarget.position.z - this.transform.position.z, 2)
                );

                if (distance <= this.boxCollider.size.x && !collideFlag)
                {
                    collideFlag = true;
                    InPositionCallback?.Invoke();
                } else
                {
                    collideFlag = false;
                }
            }
        }
    }
}
