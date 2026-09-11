using System.Collections;
using UnityEngine;

namespace  Hsinpa
{
    public class ParticleSystemCtrl : MonoBehaviour
    {
        [SerializeField] private bool deactivateAfterStopping = true;

        private ParticleSystem rootParticleSystem;
        private Coroutine shutdownRoutine;

        private void Awake()
        {
            // Finds the ParticleSystem on Flamethrower itself.
            rootParticleSystem = GetComponent<ParticleSystem>();
        }

        public void EnableFlamethrower()
        {
            if (shutdownRoutine != null)
            {
                StopCoroutine(shutdownRoutine);
                shutdownRoutine = null;
            }

            gameObject.SetActive(true);

            // true means include all child ParticleSystems.
            rootParticleSystem.Play(true);
        }

        public void DisableFlamethrower()
        {
            if (!gameObject.activeInHierarchy)
                return;

            // Stops new emission but lets existing particles fade naturally.
            rootParticleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );

            if (deactivateAfterStopping)
                shutdownRoutine = StartCoroutine(DeactivateWhenFinished());
        }

        private IEnumerator DeactivateWhenFinished()
        {
            // true checks this ParticleSystem and all children.
            while (rootParticleSystem.IsAlive(true))
                yield return null;

            gameObject.SetActive(false);
            shutdownRoutine = null;
        }
    }    
}
