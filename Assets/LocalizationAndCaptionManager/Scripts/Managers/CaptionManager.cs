using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Timeline;

public class CaptionManager : MonoBehaviour
{

    private struct CaptionStruct
    {
        public string title;
        public string content;
        public readonly bool IsValid => !string.IsNullOrEmpty(title);
    }


    [System.Serializable]
    public struct Caption
    {
        public float Timestamp;
        public AudioClip[] AudioClip;//數量對應語言
    }

    [Header("Binding")]
    [SerializeField] private LocalizationManager localizationManager;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private TMPro.TextMeshProUGUI title;
    [SerializeField] private TMPro.TextMeshProUGUI caption;
    public AudioSource audioSource;


    private Coroutine showCoroutine;

    private List<string> captionStrs = new List<string>();
    private int currentCaptionNum;

    public SignalReceiver SignalReceiver;


    private void Awake()
    {
        canvasGroup.DOFade(0, 0);
    }

    public void Show(LocalizationManager.WorkSheet workSheet, string key, Caption[] captions, Action callback)
    {
        if (captions.Length == 0)
        {
            callback?.Invoke();
            return;
        }
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);

        Debug.Log($"Worksheet {workSheet.ToString()}, key {key}");

        showCoroutine = StartCoroutine(ShowIEnumerator(workSheet, key, captions, callback));
    }

    private CaptionStruct GetCaptionStruct(string content)
    {
        if (content.Contains("[system]"))
        {
            return new CaptionStruct { title="系統", content= content.Replace("[system]", "") };
        }
        if (content.Contains("[manager]"))
        {
            return new CaptionStruct { title="主管", content= content.Replace("[manager]", "") };
        }

        return default(CaptionStruct);
    }

    private IEnumerator ShowIEnumerator(LocalizationManager.WorkSheet workSheet, string key, Caption[] captions, Action callback)
    {
        canvasGroup.DOKill();
        float totleTime = 0;
        for (int i = 0; i < captions.Length; i++)
        {
            Debug.Log(captions[i].Timestamp - totleTime);
            float waitTime = captions[i].Timestamp - totleTime;
            yield return new WaitForSeconds(waitTime);
            totleTime += waitTime;
            if (i == 0) canvasGroup.DOFade(1, 0.5f);
            
            string captionStr = localizationManager.GetString(workSheet, key + "_" + i);
            SetCaption(captionStr);
            
            audioSource.PlayOneShot(captions[i].AudioClip[localizationManager.CurrentLanguageCodeNum]);
        }
        yield return new WaitForSeconds(captions[captions.Length - 1].AudioClip[localizationManager.CurrentLanguageCodeNum].length);
        float fadeOutTime = 0.5f;
        canvasGroup.DOFade(0, fadeOutTime);
        yield return new WaitForSeconds(fadeOutTime);
        callback?.Invoke();
    }

    private void SetCaption(string fullCaptionContent)
    {
        Debug.Log(fullCaptionContent);
        CaptionStruct captionStruct = GetCaptionStruct(fullCaptionContent);

        if (captionStruct.IsValid)
        {
            title.text = captionStruct.title;
            caption.text = captionStruct.content;
        }
    }

    public void SetCaptionStr(LocalizationManager.WorkSheet workSheet, string key, int count)
    {
        captionStrs.Clear();

        for (int i = 0; i < count; i++)
        {
            Debug.Log(key + "_" + i);
            captionStrs.Add(localizationManager.GetString(workSheet, key + "_" + i));
        }
    }

    public void ShowPanel()
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(1, 0.5f);
        currentCaptionNum = 0;
        
        SetCaption(captionStrs[currentCaptionNum]);
    }

    public void NextCaption()
    {
        currentCaptionNum++;
        SetCaption(captionStrs[currentCaptionNum]);
    }

    public void HidePanel()
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(0, 0.5f);
    }
}
