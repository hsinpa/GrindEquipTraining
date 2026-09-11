using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace Hsinpa
{
    public class CaptionCameraCtrl : MonoBehaviour
    {
        [SerializeField] private float z_offset = 1;
        [SerializeField] private float y_offset = -1;
        [SerializeField] Canvas targetUiCanvas;

        [SerializeField] TimelineManager timelineManager;

        [SerializeField] List<TimelineAsset> timelineList;

        private Transform cameraTransform;
        private Action timelineStopEvent;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            cameraTransform = Camera.main.transform;     
            timelineManager.SetWorkSheet(LocalizationManager.WorkSheet.Cut1);     
        }

        void Update()
        {
            if (cameraTransform != null && targetUiCanvas != null)
            {
                
                Vector3 canvasPosition = cameraTransform.position;
                canvasPosition += (cameraTransform.forward * z_offset);
                canvasPosition += (cameraTransform.up * y_offset);

                targetUiCanvas.transform.position = Vector3.Lerp(targetUiCanvas.transform.position, canvasPosition, 0.1f);
                targetUiCanvas.transform.LookAt(cameraTransform, cameraTransform.up);                                                                                                                                                                                                                                                          }
        }

        public void Play(LocalizationManager.WorkSheet worksheet, Action p_timelineStopEvent = null )
        {
            timelineStopEvent = p_timelineStopEvent;
            timelineManager.SetWorkSheet(worksheet);     

            var sampleAsset = GetTimeasset(worksheet);
            timelineManager.Show(sampleAsset, () => {
                timelineStopEvent?.Invoke();
            });
        }

        public Task PlayAsync(LocalizationManager.WorkSheet worksheet)
        {
            timelineManager.SetWorkSheet(worksheet);     

            var completion = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

            var sampleAsset = GetTimeasset(worksheet);
            timelineManager.Show(
                sampleAsset,
                () => completion.TrySetResult(true));

            return completion.Task;
        }

        public void Stop()
        {
            timelineManager.Stop();
        }

        private TimelineAsset GetTimeasset(LocalizationManager.WorkSheet worksheet)
        {
            foreach (TimelineAsset asset in timelineList)
            {
                if (asset.name == worksheet.ToString())
                {
                    return asset;
                }
            }

            return null;

        }
    }    
}
