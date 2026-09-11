using System;
using UnityEngine;


namespace Hsinpa
{
    public class CameraCollider : MonoBehaviour
    {
        private SphereCollider collider;
        public Action<Scene4Collider> onCollideEvent;

        void Start()
        {
            collider = this.GetComponent<SphereCollider>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == "MouthMask" || other.tag == "EyeMask")
            {
                onCollideEvent?.Invoke(other.GetComponent<Scene4Collider>());
            }
        }

        void OnTriggerExit(Collider other)
        {
            
        }
    }
}