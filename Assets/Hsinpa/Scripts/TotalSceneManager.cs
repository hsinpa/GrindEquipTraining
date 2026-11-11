using Hsinpa.Utility;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Video;
using static UnityEngine.InputSystem.InputAction;

namespace Hsinpa
{
    public class TotalSceneManager : MonoBehaviour
    {
        [SerializeField]
        private CameraMaskManager cameraMaskManager;

        [SerializeField]
        private GrinderManager scene2Manager;

        [SerializeField]
        private MaskManager maskManager;

        [SerializeField]
        private Transform playeTransform;

        [SerializeField]
        private LocalizationManager localizationManager;

        [SerializeField]
        private CaptionCameraCtrl captionCtrl;

        [SerializeField]
        private TeleportationManager teleportManager;

        [Header("Video source")]
        [SerializeField]
        private VideoClip scene3VideoClip;

        [Header("Position Config")]
        [SerializeField]
        private Transform scene4TeleportPoint;

        private StaticEnum.SimulationStage currentStage = StaticEnum.SimulationStage.Stage_0;
        private InputSystem_Actions inputActions;
        private XRIDefaultInputActions _xrInputActions;

        void Start()
        {   
            Transform cameraTransform = playeTransform.GetComponentInChildren<Camera>().transform;
            inputActions = new InputSystem_Actions();
            inputActions.Enable();
            this._xrInputActions = new XRIDefaultInputActions();
            this._xrInputActions.Enable();

            inputActions.UI.Submit.performed += OnSubmitEvent;
            scene2Manager.completeScene2Callback += RunStage3;
            scene2Manager.completeScene5Callback += RunStage6;
            scene2Manager.completeScene7Callback += RunStage8;
            scene2Manager.completeScene8Callback += RunStage9;
            scene2Manager.SetUp(captionCtrl);

            // Not to block the raycast on UI
            scene2Manager.ShowGrinderMachine(false);

            _xrInputActions.XRIRightInteraction.Activate.started += OnColliderSelected;
            _xrInputActions.XRILeftInteraction.Activate.started += OnColliderSelected;

            maskManager.completeCallback += () => { 
                maskManager.End();
            };

            teleportManager.grinderTeleportAnchor.InPositionCallback += () =>
            {
                if (maskManager.completeFlag) RunStage5();
            };
            teleportManager.grinderTeleportAnchor.SetTarget(cameraTransform);
            maskManager.Setup(cameraTransform, captionCtrl);

            Debug.Log(Loc.CurrentLanguage);
            RunStage0();
        }

        void RunStage0()
        {
            currentStage = StaticEnum.SimulationStage.Stage_0;
            cameraMaskManager.Enter();
            cameraMaskManager.ShowStartMenu(() =>
            {
                if (currentStage == StaticEnum.SimulationStage.Stage_0) RunStage1(); 
            });
        
            // captionCtrl.Play(LocalizationManager.WorkSheet.Cut0_Start);
        }

        void RunStage1()
        {
            localizationManager.SetLanguage((Loc.CurrentLanguage == "zhcht") ? 0 : 1); 
            scene2Manager.ShowGrinderMachine(true);
            currentStage = StaticEnum.SimulationStage.Stage_1;
            cameraMaskManager.Dispose();

            scene2Manager.Enter();

            scene2Manager.Scene1Start();

            captionCtrl.Play(LocalizationManager.WorkSheet.Cut1, () =>
            {
                RunStage2();
            });
        }

        void RunStage2()
        {
            if (currentStage == StaticEnum.SimulationStage.Stage_1)
            {
                currentStage = StaticEnum.SimulationStage.Stage_2;
                captionCtrl.Stop();
                scene2Manager.Scene2Start();
            }
        }

        // Play education video
        void RunStage3()
        {
            currentStage = StaticEnum.SimulationStage.Stage_3;
            scene2Manager.Dispose();
            cameraMaskManager.Enter();

            captionCtrl.Play(LocalizationManager.WorkSheet.Cut3);
            cameraMaskManager.PlayVideo(scene3VideoClip, () =>
            {
                if (currentStage == StaticEnum.SimulationStage.Stage_3) 
                    RunStage4();
            });
            Debug.Log("RunStage3");
        }

        void RunStage4()
        {
            playeTransform.position = new Vector3(scene4TeleportPoint.position.x, scene4TeleportPoint.position.y, scene4TeleportPoint.position.z);
            playeTransform.rotation = scene4TeleportPoint.rotation;

            cameraMaskManager.Dispose();
            maskManager.Enter();

            currentStage = StaticEnum.SimulationStage.Stage_4;
        }

        // Play smoke animation on grinder scene
        void RunStage5()
        {
            if (currentStage == StaticEnum.SimulationStage.Stage_4)
            {
                Debug.Log("Stage 5");
                currentStage = StaticEnum.SimulationStage.Stage_5;

                scene2Manager.Scene5Start();
            }
        }

        void RunStage6()
        {
            if (currentStage != StaticEnum.SimulationStage.Stage_5) return;
            
            scene2Manager.Dispose();
            currentStage = StaticEnum.SimulationStage.Stage_6;

            cameraMaskManager.Enter();
            cameraMaskManager.SetText("", Loc.Get("SCENE6NOTICE"));

            captionCtrl.Play(LocalizationManager.WorkSheet.Cut6);

            _ = UtilityFunc.DoDelayWork(20, () =>
            {
                if (currentStage == StaticEnum.SimulationStage.Stage_6) RunStage7();
            });
        }

        void RunStage7()
        {
            cameraMaskManager.Dispose();
            scene2Manager.Dispose();
            currentStage = StaticEnum.SimulationStage.Stage_7;

            scene2Manager.Scene7Start();
        }

        void RunStage8()
        {
            cameraMaskManager.Dispose();
            scene2Manager.Dispose();
            currentStage = StaticEnum.SimulationStage.Stage_8;
            scene2Manager.Scene8Start();

        }

        void RunStage9()
        {
            cameraMaskManager.Dispose();
            scene2Manager.Dispose();
            currentStage = StaticEnum.SimulationStage.Stage_9;
            scene2Manager.Scene9Start();

            UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.COMPLETE); 

            _ = UtilityFunc.DoDelayWork(1, () =>
            {
                captionCtrl.Play(LocalizationManager.WorkSheet.Cut10); 
            });
        }

        void OnSubmitEvent(UnityEngine.InputSystem.InputAction.CallbackContext callbackContext)
        {
            if (currentStage == StaticEnum.SimulationStage.Stage_0)
            {
               RunStage1();
               return;
            }

            if (currentStage == StaticEnum.SimulationStage.Stage_1)
            {
                RunStage2(); 
                return;
            }

            if (currentStage == StaticEnum.SimulationStage.Stage_3)
            {
                RunStage4(); 
                return;
            }

            if (currentStage == StaticEnum.SimulationStage.Stage_6)
            {
                RunStage7();
                return;
            }
        }

        private void OnColliderSelected(CallbackContext a)
        {
            if (currentStage == StaticEnum.SimulationStage.Stage_9)
            {
                UniversalAudioSolution.instance.PlayAudio(UniversalAudioSolution.AudioType.UI, Audio.TAG, Audio.UI_CLICK); 

                Application.Quit();
            }
        }

    }
}