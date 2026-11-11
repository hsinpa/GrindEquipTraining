using System;
using System.Threading.Tasks;
using Hsinpa.UI;
using Hsinpa.Utility;
using Hsinpa.Utility.UI;
using TMPro;
using UnityEngine;

namespace Hsinpa
{
    public class GrinderManager : MonoBehaviour, ISceneManager
    {
        private Scene2Collider[] interactableColliders;

        [SerializeField]
        private CanvasGroup uiCavnas;

        [SerializeField]
        private MessageHolder messageHolder;

        [SerializeField]
        private GameObject scene7Reminder;

        [SerializeField]
        private LastSceneUI scene8Reminder;

        [SerializeField]
        private Transform grinderMachine1;

        [SerializeField]
        private Transform grinderMachine2;

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
        private Animator grindToolTextCanvas;

        [SerializeField]
        private TextMeshProUGUI grindToolText;

        [Header("Scene 7")]
        [SerializeField]
        private SimpleSelectCollider scene7AirMachineCollider;

        [SerializeField]
        private Animator scene7GrinderButtonUI;

        [SerializeField]
        private Animator scene7MaskButtonUI;

        [SerializeField]
        private SimpleSelectCollider scene7CorrectHeadCollider;

        [SerializeField]
        private SimpleSelectCollider scene7Wrong1HeadCollider;

        [SerializeField]
        private SimpleSelectCollider scene7Wrong2HeadCollider;

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
            scene8Reminder.SetUp();
        }

        void Start()
        {
            interactableColliders = this.GetComponentsInChildren<Scene2Collider>();

            foreach (Scene2Collider c in interactableColliders)
            {
                c.isSelectedCallback += OnColliderSelected;
            }

            scene7CorrectHeadCollider.isSelectedCallback += OnScene7CorrectHeadSelect;
            scene7Wrong1HeadCollider.isSelectedCallback += OnScene7WrongHeadSelect;
            scene7Wrong2HeadCollider.isSelectedCallback += OnScene7WrongHeadSelect;

            scene7AirMachineCollider.isSelectedCallback += OnAirMachineBtnSelect;

            scene5ColliderGrab.SetUp(grindToolTextCanvas);
            scene5ColliderGrab.gameObject.SetActive(false);
        }

        void OnColliderSelected(bool isSelected)
        {
            colliderSelectedCount++;

            if (isSelected)
            {
                messageHolder.SetContent(Loc.Get("SCENE2COMPLETENOTICE"), Loc.Get("SCENE2COMPLETETITLE"), MessageHolder.IconState.Victory);

                _captionCtrl.Play(LocalizationManager.WorkSheet.Cut2_Complete, () =>
                {
                    completeScene2Callback?.Invoke();
                });
            }
        }

        public void ShowGrinderMachine(bool is_show)
        {
            grinderMachine1.gameObject.SetActive(is_show);
            grinderMachine2.gameObject.SetActive(is_show);
        }

        public void Scene1Start()
        {
            messageHolder.SetContent(Loc.Get("SCENE1NOTICE"), Loc.Get("SCENE1TITLE"), MessageHolder.IconState.Info);

            foreach (Scene2Collider c in interactableColliders)
            {
                c.gameObject.SetActive(true);
            }
        }

        public void Scene2Start()
        {   
            Reset();
            isScene2Start = true;
            messageHolder.SetContent(Loc.Get("SCENE2NOTICE"), Loc.Get("SCENE2NOTICETITLE"), MessageHolder.IconState.Info);
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
            scene5ColliderGrab.gameObject.SetActive(true);
            grinderMachineAudio.Play();

            grindToolText.text = Loc.Get("GRIND_TOOL_NOTICE");

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
            scene5ColliderGrab.gameObject.SetActive(false);

            scene7GrinderButtonUI.gameObject.SetActive(true);
            scene7GrinderButtonUI.SetBool("flash", true);

            scene7CorrectHeadCollider.GetParentObject()?.gameObject.SetActive(true);
            scene7Wrong1HeadCollider.GetParentObject()?.gameObject.SetActive(true);
            scene7Wrong2HeadCollider.GetParentObject()?.gameObject.SetActive(true);

            scene7AirMachineCollider.Interaction(true);

            messageHolder.gameObject.SetActive(false);
            scene7Reminder.gameObject.SetActive(true);

            await _captionCtrl.PlayAsync(LocalizationManager.WorkSheet.Cut7);

            messageHolder.gameObject.SetActive(true);
            scene7Reminder.gameObject.SetActive(false);

            messageHolder.SetContent(Loc.Get("SCENE7NOSENOTICE"), Loc.Get("SCENE7NOSETITLE"), MessageHolder.IconState.Info);

            _captionCtrl.Play(LocalizationManager.WorkSheet.Cut8_Air);
        }

        public async void Scene8Start()
        {
            Reset();
            scene5ColliderGrab.gameObject.SetActive(true);
            grindToolText.text = Loc.Get("GRIND_TOOL_RENOTICE");

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

            scene8Reminder.ShowResult();
        }

        private async void OnAirMachineBtnSelect()
        {
            airMachineAudio.Play(); 
            await _captionCtrl.PlayAsync(LocalizationManager.WorkSheet.Cut8_AirComplete);
            await _captionCtrl.PlayAsync(LocalizationManager.WorkSheet.Cut8_Mask);

            scene7MaskButtonUI.gameObject.SetActive(true);
            this.scene7IsAirMachineClickFlag = true;
            scene7GrinderButtonUI.gameObject.SetActive(false);

            scene7CorrectHeadCollider.Interaction(true);
            scene7Wrong1HeadCollider.Interaction(true);
            scene7Wrong2HeadCollider.Interaction(true);
        }

        private async void OnScene7CorrectHeadSelect()
        {
            if (this.scene7IsAirMachineClickFlag)
            {
                UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.UI_CLICK); 
                scene7MaskButtonUI.gameObject.SetActive(false);
                SimpleEventSystem.Send(StaticEvent.TaskStatusEvent, TaskStatus.Step, true);     
                messageHolder.SetContent(Loc.Get("SCENE7MOUTHNOTICE"),Loc.Get("SCENE7MOUTHTITLE"), MessageHolder.IconState.Info);

                _captionCtrl.Play(LocalizationManager.WorkSheet.Cut8_MaskComplete, () =>
                {
                    this.completeScene7Callback?.Invoke(); 
                });
            }
        }

        private async void OnScene7WrongHeadSelect()
        {
            if (this.scene7IsAirMachineClickFlag)
            {
                SimpleEventSystem.Send(StaticEvent.TaskStatusEvent, TaskStatus.Step, false);     
                UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.WRONG_CHOICE); 
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
            scene7GrinderButtonUI.gameObject.SetActive(false);
            scene7MaskButtonUI.gameObject.SetActive(false);
            scene7GrinderButtonUI.SetBool("flash", false);
            scene7AirMachineCollider.Interaction(false);

            grinderMachineAudio.Stop();
            // airMachineAudio.Stop();
            grinderingAudio.Stop();
            grindToolTextCanvas.gameObject.SetActive(false);

            scene7CorrectHeadCollider.GetParentObject()?.gameObject.SetActive(false);
            scene7Wrong1HeadCollider.GetParentObject()?.gameObject.SetActive(false);
            scene7Wrong2HeadCollider.GetParentObject()?.gameObject.SetActive(false);
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
