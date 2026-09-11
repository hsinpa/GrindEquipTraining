using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Text))]
public class LocalizationUpdater : MonoBehaviour
{
    [SerializeField] private LocalizationManager localizationManager;

    [ReadOnly] public Text UpdateText;
    public LocalizationManager.WorkSheet workSheet;
    public string key;

    private Coroutine updateStringCoroutine;

    private void Awake()
    {
        UpdateText = transform.GetComponent<Text>();
    }

    private void OnEnable()
    {
        updateStringCoroutine = StartCoroutine(UpdateTextIEnumerator());
    }

    private void OnDisable()
    {
        localizationManager?.LocalizationUpdaters.Remove(this);
        if (updateStringCoroutine != null)
        {
            StopCoroutine(updateStringCoroutine);
            updateStringCoroutine = null;
        }
    }

    private IEnumerator UpdateTextIEnumerator()
    {
        while (localizationManager == null)
            yield return null;
        while (!localizationManager.isInit)
            yield return null;

        localizationManager.LocalizationUpdaters.Add(this);
        UpdateText.text = localizationManager.GetString(workSheet, key);
    }
}
