using System;
using System.Threading.Tasks;
using Hsinpa.UI;
using Hsinpa.Utility;
using Hsinpa.Utility.UI;
using UnityEngine;

namespace Hsinpa
{
    public class GrinderManager : MonoBehaviour, ISceneManager
    {
        private Scene2Collider[] interactableColliders;

        [SerializeField]
        private GrinderSpin grinderSpinAnime;

        [SerializeField]
        private CanvasGroup uiCavnas;

        [SerializeField]
        private MessageHolder messageHolder;

        [SerializeField]
        private GameObject scene7Reminder;

        [SerializeField]
        private GameObject scene8Reminder;

        [Header("Scene 5")]
        [SerializeField]
        private ParticleSystemCtrl vfxSmokeEffect;

        [SerializeField]
        private ParticleSystemCtrl vfxFireFlameRightEffect;

        [SerializeField]
        private ParticleSystemCtrl vfxFireFlameLeftEffect;

        [SerializeField]
        private Scene5Collider scene5ColliderGrab;

        [SerializeField]
        private Transform scene5ColliderHolder;

        [Header("Scene 7")]
        [SerializeField]
        private SimpleSelectCollider scene7AirMachineCollider;

        [SerializeField]
        private Animator scene7GrinderButtonUI;

        [SerializeField]
        private Animator scene7HeadAnimator;

        [SerializeField]
        private SimpleSelectCollider scene7NoseCollider;

        [SerializeField]
        private SimpleSelectCollider scene7MouthCollider;

        [Header("Audio")]
        [SerializeField]
        private AudioSource grinderMachineAudio;
        [SerializeField]
        private AudioSource grinderingAudio;
        [SerializeField]
        private AudioSource airMachineAudio;

        private int colliderSelectedCount = 0;

        public Action completeScene2Callback;
        public Action completeScene5Callback;
        public Action completeScene7Callback;
        public Action completeScene8Callback;

        private bool isScene5Complete = false;
        private bool isScene8Complete = false;
        private bool isScene2Start = false;

        private bool scene7IsAirMachineClickFlag = false;
        private bool scene7IsNoseClickFlag = false;
        private CaptionCameraCtrl _captionCtrl;

        public void SetUp(CaptionCameraCtrl captionCtrl)
        {
            _captionCtrl = captionCtrl;
        }

        void Start()
        {
            interactableColliders = this.GetComponentsInChildren<Scene2Collider>();

            foreach (Scene2Collider c in interactableColliders)
            {
                c.isSelectedCallback += OnColliderSelected;
            }

            scene7NoseCollider.isSelectedCallback += OnNoseSelect;
            scene7MouthCollider.isSelectedCallback += OnMouthSelect;
            scene7AirMachineCollider.isSelectedCallback += OnAirMachineBtnSelect;

            grinderSpinAnime.enabled = false;

            scene5ColliderGrab.SetUp();
            scene5ColliderHolder.gameObject.SetActive(false);
        }

        void OnColliderSelected(bool isSelected)
        {
            colliderSelectedCount++;

            if (colliderSelectedCount >= interactableColliders.Length)
            {
                messageHolder.SetContent(StaticString.SCENE2COMPLETENOTICE, StaticString.SCENE2COMPLETETITLE, MessageHolder.IconState.Victory);


                _captionCtrl.Play(LocalizationManager.WorkSheet.Cut2_Complete, () =>
                {
                    completeScene2Callback?.Invoke();
                });
            }
        }

        public void Scene1Start()
        {
            messageHolder.SetContent(StaticString.SCENE1NOTICE, StaticString.SCENE1TITLE, MessageHolder.IconState.Info);

            foreach (Scene2Collider c in interactableColliders)
            {
                c.gameObject.SetActive(true);
            }
        }

        public void Scene2Start()
        {   
            Reset();
            isScene2Start = true;
            messageHolder.SetContent(StaticString.SCENE2NOTICE, StaticString.SCENE2NOTICETITLE, MessageHolder.IconState.Info);
            foreach (Scene2Collider c in interactableColliders)
            {
                c.gameObject.SetActive(true);
                c.Interaction(true);
            }
            _captionCtrl.Play(LocalizationManager.WorkSheet.Cust2_DustDetection);                
        }

        public void Scene5Start()
        {
            Reset();
            scene5ColliderHolder.gameObject.SetActive(true);
            grinderSpinAnime.enabled = true;
            grinderMachineAudio.Play();

            scene5ColliderGrab.StartSimulation(true, (x) =>
            {
                vfxSmokeEffect.EnableFlamethrower();

                PlayFlamethrower(x.name, true);
                grinderingAudio.Play();

                if (!isScene5Complete)
                {
                    isScene5Complete = true;

                    this._captionCtrl.Play(LocalizationManager.WorkSheet.Cut5, () =>
                    {
                        completeScene5Callback?.Invoke();
                    });
                }
            }, () =>
            {
                vfxSmokeEffect.DisableFlamethrower();
                PlayFlamethrower(null, false);

                grinderingAudio.Stop();
            });
        }

        public async Task Scene7Start()
        {
            Reset();
            uiCavnas.SetVisible(true);
            scene5ColliderHolder.gameObject.SetActive(false);

            scene7GrinderButtonUI.SetBool("flash", true);
            scene7HeadAnimator.gameObject.SetActive(true);
            scene7NoseCollider.Interaction(false);
            scene7MouthCollider.Interaction(false);

            scene7AirMachineCollider.Interaction(true);

            messageHolder.gameObject.SetActive(false);
            scene7Reminder.gameObject.SetActive(true);

            await _captionCtrl.PlayAsync(LocalizationManager.WorkSheet.Cut7);

            messageHolder.gameObject.SetActive(true);
            scene7Reminder.gameObject.SetActive(false);

            messageHolder.SetContent(StaticString.SCENE7NOSENOTICE, StaticString.SCENE7NOSETITLE, MessageHolder.IconState.Info);

            _captionCtrl.Play(LocalizationManager.WorkSheet.Cut8_Air);
        }

        public async void Scene8Start()
        {
            Reset();
            scene5ColliderHolder.gameObject.SetActive(true);

            grinderSpinAnime.enabled = true;
            scene5ColliderGrab.StartSimulation(true, (x) =>
            {
                PlayFlamethrower(x.name, true);

                grinderingAudio.Play();


                if (!isScene8Complete)
                {
                    isScene8Complete = true;

                    _  =  Utility.UtilityFunc.DoDelayWork(6,  () =>
                    {
                        completeScene8Callback?.Invoke();
                    });

                }
            }, () =>
            {
                grinderingAudio.Stop();
                PlayFlamethrower(null, false);
            });
        }

        private void PlayFlamethrower(string gameobjectName, bool play)
        {
            if (play)
            {
                if (gameobjectName.Contains("item_L"))
                {
                    vfxFireFlameLeftEffect.EnableFlamethrower();
                    vfxFireFlameRightEffect.DisableFlamethrower();
                }

                if (gameobjectName.Contains("item_R"))
                {
                    vfxFireFlameLeftEffect.DisableFlamethrower();
                    vfxFireFlameRightEffect.EnableFlamethrower();
                }
            } else
            {
                vfxFireFlameLeftEffect.DisableFlamethrower();
                vfxFireFlameRightEffect.DisableFlamethrower();
            }
        }

        // Show END UI
        public async void Scene9Start()
        {
            Reset();
            uiCavnas.SetVisible(true);
            messageHolder.gameObject.SetActive(false);
            scene7Reminder.gameObject.SetActive(false);
            scene8Reminder.gameObject.SetActive(true);
        }

        private async void OnAirMachineBtnSelect()
        {
            await _captionCtrl.PlayAsync(LocalizationManager.WorkSheet.Cut8_AirComplete);
            await _captionCtrl.PlayAsync(LocalizationManager.WorkSheet.Cut8_Mask);
            this.scene7IsAirMachineClickFlag = true;
            scene7NoseCollider.Interaction(true);   
            airMachineAudio.Play();             
        }

        private void OnNoseSelect()
        {
            if (this.scene7IsAirMachineClickFlag)
            {
                this.scene7IsNoseClickFlag = true;
                scene7HeadAnimator.SetTrigger("Nose");
                messageHolder.SetContent(StaticString.SCENE7MOUTHNOTICE, StaticString.SCENE7MOUTHTITLE, MessageHolder.IconState.Info);
                scene7NoseCollider.Interaction(false);
                scene7MouthCollider.Interaction(true);
                UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.UI_CLICK); 

                _ = UtilityFunc.DoDelayWork(4, () =>
                {
                    OnMouthSelect();
                });        
            }
        }

        private void OnMouthSelect()
        {
            if (this.scene7IsAirMachineClickFlag && this.scene7IsNoseClickFlag)
            {
                scene7NoseCollider.Interaction(false);
                scene7NoseCollider.Interaction(false);
                scene7HeadAnimator.SetTrigger("Mouth");
                UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.UI_CLICK);            

                _captionCtrl.Play(LocalizationManager.WorkSheet.Cut8_MaskComplete, () =>
                {
                    this.completeScene7Callback?.Invoke(); 
                });
            }
        }

        private void Reset()
        {
            colliderSelectedCount = 0;
            isScene5Complete = false;
            isScene8Complete = false;
            isScene2Start = false;
            this.scene7IsAirMachineClickFlag = false;
            this.scene7IsNoseClickFlag = false;

            foreach (Scene2Collider c in interactableColliders)
            {
                c.Reset();
                c.gameObject.SetActive(false);
            }

            scene5ColliderGrab.Reset();
            scene7HeadAnimator.gameObject.SetActive(false);
            scene7GrinderButtonUI.SetBool("flash", false);
            grinderSpinAnime.enabled = false;
            scene7AirMachineCollider.Interaction(false);
            grinderMachineAudio.Stop();
            // airMachineAudio.Stop();
            grinderingAudio.Stop();
        }

        public void Enter()
        {
            Reset();

            uiCavnas.SetVisible(true);
        }

        public void Dispose()
        {
            uiCavnas.SetVisible(false);
            messageHolder.SetContent("");

            vfxSmokeEffect.DisableFlamethrower();
            PlayFlamethrower(null, false);

            Reset();
        }
    }
}
