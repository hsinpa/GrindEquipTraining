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
    private const string FallbackLanguageCode = "en";
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

        //Collect all VoiceTrack_{lang} tracks
        List<TrackAsset> voiceTracks = new List<TrackAsset>();
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            if (track.name.StartsWith(VoiceTrackName + "_"))
                voiceTracks.Add(track);
        }

        //Pick current language track, fallback to en when missing
        string currentLang = localizationManager.languageCodes[localizationManager.CurrentLanguageCodeNum];
        string currentTrackName = $"{VoiceTrackName}_{currentLang}";
        string fallbackTrackName = $"{VoiceTrackName}_{FallbackLanguageCode}";

        TrackAsset selectedTrack = voiceTracks.Find(t => t.name == currentTrackName);
        if (selectedTrack == null)
        {
            selectedTrack = voiceTracks.Find(t => t.name == fallbackTrackName);
            Debug.LogWarning($"[TimelineManager] '{currentTrackName}' not found in '{timelineAsset.name}', fallback to '{fallbackTrackName}'" +
                             (selectedTrack == null ? " (also not found)." : "."));
        }

        //Mute all other voice tracks, bind the selected one
        foreach (var track in voiceTracks)
        {
            track.muted = track != selectedTrack;
        }

        if (selectedTrack != null)
        {
            playableDirector.SetGenericBinding(selectedTrack, captionManager.audioSource);
            foreach (var marker in selectedTrack.GetMarkers())
            {
                if (marker is SignalEmitter signalEmitter)
                    captionSignals.Add(signalEmitter);
            }
        }

        Debug.Log($"[TimelineManager] '{timelineAsset.name}' lang='{currentLang}' (index {localizationManager.CurrentLanguageCodeNum}), " +
                  $"voiceTracks=[{string.Join(", ", voiceTracks.ConvertAll(t => $"{t.name}{(t.muted ? "(muted)" : "")}"))}], " +
                  $"selected='{(selectedTrack != null ? selectedTrack.name : "none")}', " +
                  $"clips={(selectedTrack != null ? System.Linq.Enumerable.Count(selectedTrack.GetClips()) : 0)}, " +
                  $"audioSource={(captionManager.audioSource != null ? captionManager.audioSource.name : "NULL")}");

        captionManager.SetCaptionStr(currentWorkSheet, timelineAsset.name + "_Caption", captionSignals.Count - 1);

        // Mute state is only read when the graph is built, so force a rebuild
        // in case the director already holds a graph built with the old mute state.
        playableDirector.RebuildGraph();
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
