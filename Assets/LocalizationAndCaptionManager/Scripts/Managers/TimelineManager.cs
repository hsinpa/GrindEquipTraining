using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineManager : MonoBehaviour
{
    [SerializeField] private LocalizationManager localizationManager;
    [SerializeField] private CaptionManager captionManager;

    private Coroutine showCoroutine;

    [SerializeField] private PlayableDirector playableDirector;

    private string VoiceTrackName = "VoiceTrack";
    private List<SignalEmitter> captionSignals = new List<SignalEmitter>();

    private LocalizationManager.WorkSheet currentWorkSheet;

    public void SetWorkSheet(LocalizationManager.WorkSheet workSheet)
    {
        currentWorkSheet = workSheet;
    }

    public void Show(TimelineAsset timelineAsset, Action callback = null)
    {
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);
        showCoroutine = StartCoroutine(ShowIEnumerator(timelineAsset, callback));
    }

    public void Stop()
    {
        playableDirector.Stop();
        captionSignals.Clear();
        captionManager.HidePanel();
    }

    private IEnumerator ShowIEnumerator(TimelineAsset timelineAsset, Action callback)
    {
        playableDirector.playableAsset = timelineAsset;

        captionSignals.Clear();

        foreach (var track in timelineAsset.GetOutputTracks())
        {
            //Set CaptionTrack
            for (int i = 0; i < localizationManager.languageCodes.Count; i++)
            {

                Debug.Log($"{VoiceTrackName}_{localizationManager.languageCodes[i]}");
                if (track.name == $"{VoiceTrackName}_{localizationManager.languageCodes[i]}")
                {
                    track.muted = localizationManager.languageCodes[i] != localizationManager.languageCodes[localizationManager.CurrentLanguageCodeNum];
                    if (!track.muted)
                    {
                        playableDirector.SetGenericBinding(track, captionManager.audioSource);
                        Debug.Log("Here" + "_" + $"{VoiceTrackName}_{localizationManager.languageCodes[i]}");
                        foreach (var marker in track.GetMarkers())
                        {
                            if (marker is SignalEmitter)
                            {
                                SignalEmitter signalEmitter = (SignalEmitter)marker;
                                captionSignals.Add(signalEmitter);
                            }
                        }
                    }
                }
            }
        }

        captionManager.SetCaptionStr(currentWorkSheet, timelineAsset.name + "_Caption", captionSignals.Count - 1);

        playableDirector.Play();

        while (playableDirector.state != PlayState.Playing)
        {
            yield return null;
        }

        playableDirector.stopped += Stopped_playableDirector;

        void Stopped_playableDirector(PlayableDirector aDirector)
        {
            playableDirector.stopped -= Stopped_playableDirector;
            callback?.Invoke();
        }
    }
}