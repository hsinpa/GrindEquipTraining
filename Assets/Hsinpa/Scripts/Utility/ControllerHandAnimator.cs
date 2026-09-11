using UnityEngine;

namespace Hsinpa
{
    public class ControllerHandAnimator : MonoBehaviour
    {
        private Animator _animator;

        void Start()
        {
            this._animator = this.GetComponent<Animator>();
        }

        public void PlayAnimation(string animationName, bool triggerFlag)
        {
            this._animator.SetBool(animationName, triggerFlag);
        }
    }    
}
