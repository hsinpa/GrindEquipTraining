using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SamplePlay : MonoBehaviour
{
    [SerializeField] private TimelineManager timelineManager;
    [SerializeField] private TimelineAsset samplePlayableAsset;

    private void Start()
    {
        timelineManager.SetWorkSheet(LocalizationManager.WorkSheet.Cut1);
    }


    [InspectorButton]
    private void Play()
    {
        timelineManager.Show(samplePlayableAsset);
    }
}
