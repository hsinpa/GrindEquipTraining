using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Globalization;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocalizationManager : MonoBehaviour
{
    //Setting--
    private string baseUrl = "https://docs.google.com/spreadsheets/d/1Y-MfAkIWgXuM8ONWooW6rIMc3caOnuEY3WM6r9cBB-M/export?format=csv&gid=";
    public enum WorkSheet {Version, Cut0_Start, Cut1, Cust2_DustDetection, Cut2_Complete, Cut3, Cut4_Begin, Cut4_Mistake, Cut4_Complete, Cut5, Cut6, Cut7, Cut8_Air, Cut8_AirComplete, Cut8_Mask, Cut8_MaskComplete, Cut10 };
    private int[] gid = {2089288298, 794692782, 0, 1214422446, 810627338, 1305103595, 496206170, 373358810, 73108086, 1816233372, 174195983, 407541686, 1664310869, 1161585711, 1672976812, 1175951804, 1109326520  };
    //--

    private Dictionary<WorkSheet, Dictionary<string, string>> dic = new Dictionary<WorkSheet, Dictionary<string, string>>();


    private string currentLanguageCodeNumPlayerprefsKey = "currentLanguageCodeNum";

    [ReadOnly] public List<string> languageCodes;
    [ReadOnly] public int CurrentLanguageCodeNum;
    [ReadOnly] public List<LocalizationUpdater> LocalizationUpdaters;

    public bool isInit;

    public Action OnLocalizrtionInit;

    private Coroutine updateLocalizationUpdaterCoroutine;

    private void Awake()
    {
        isInit = false;
        languageCodes.Clear();
        if (PlayerPrefs.HasKey(currentLanguageCodeNumPlayerprefsKey))
            CurrentLanguageCodeNum = PlayerPrefs.GetInt(currentLanguageCodeNumPlayerprefsKey);
        else
        {
            CurrentLanguageCodeNum = 0;
            PlayerPrefs.SetInt(currentLanguageCodeNumPlayerprefsKey, CurrentLanguageCodeNum);
        }

        var workSheets = Enum.GetValues(typeof(WorkSheet));
        foreach (WorkSheet sheet in workSheets)
        {
            dic[sheet] = new Dictionary<string, string>();
        }

        StartCoroutine(Init());
    }

    public void SetLanguage(int languageNum)
    {
        if (languageNum < 0 || languageNum >= languageCodes.Count)
        {
            Debug.LogWarning($"Language index {languageNum} is out of range. Available languages: {languageCodes.Count}.");
            return;
        }

        CurrentLanguageCodeNum = languageNum;
        PlayerPrefs.SetInt(currentLanguageCodeNumPlayerprefsKey, CurrentLanguageCodeNum);
        UpdateLocalizationUpdater();
    }


    public string GetString(WorkSheet workSheet, string key)
    {
        if (languageCodes.Count == 0 || CurrentLanguageCodeNum < 0 || CurrentLanguageCodeNum >= languageCodes.Count)
        {
            Debug.LogWarning("Language codes have not been loaded yet.");
            return string.Empty;
        }

        string languageKey = $"{key}_{languageCodes[CurrentLanguageCodeNum]}";

        if (dic.TryGetValue(workSheet, out var languageDict))
        {
            if (languageDict.TryGetValue(languageKey, out var result))
            {
                return result;
            }
        }

        return string.Empty;
    }

    private IEnumerator Init()
    {
        yield return StartCoroutine(CheckAndUpdateSheet());
        LoadAllCSVFile();

        if (languageCodes.Count > 0)
        {
            CurrentLanguageCodeNum = Mathf.Clamp(CurrentLanguageCodeNum, 0, languageCodes.Count - 1);
            PlayerPrefs.SetInt(currentLanguageCodeNumPlayerprefsKey, CurrentLanguageCodeNum);
            Debug.Log("Language codes loaded from Google Sheet: " + string.Join(", ", languageCodes));
        }
        else
        {
            Debug.LogError("No language code was found in the localization CSV header.");
        }

        isInit = true;
        OnLocalizrtionInit?.Invoke();
    }

    private IEnumerator CheckAndUpdateSheet()
    {
        //Load LocalData
        string localVersionFilePath = Path.Combine(Application.streamingAssetsPath, WorkSheet.Version.ToString() + ".csv");
        string localVersion = "";
        if (File.Exists(localVersionFilePath))
        {
            ParseCSVVersion(File.ReadAllText(localVersionFilePath), WorkSheet.Version);
            localVersion = dic[WorkSheet.Version]["Version_Value"];
        }

        //Load CloudData
        string versionCsvUrl = baseUrl + gid[(int)WorkSheet.Version].ToString();
        UnityWebRequest versionRequest = UnityWebRequest.Get(versionCsvUrl);
        versionRequest.timeout = 5;
        yield return versionRequest.SendWebRequest();
        Debug.Log(versionCsvUrl);
        if (versionRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to get version: " + versionRequest.error);
            yield break;
        }

        //Load CludData
        ParseCSVVersion(versionRequest.downloadHandler.text, WorkSheet.Version);
        string cloudVersion = dic[WorkSheet.Version]["Version_Value"];

        Debug.Log("LocalVersion :" + localVersion + "CloudVersion :" + cloudVersion);
        // VersionCheck
        if (cloudVersion == localVersion && AllLocalFilesExist())
        {
            Debug.Log("Data is up-to-date.");
            yield break;
        }

        var workSheets = Enum.GetValues(typeof(WorkSheet));
        foreach (WorkSheet sheet in workSheets)
        {
            string csvUrl = baseUrl + gid[(int)sheet].ToString();
            UnityWebRequest dataRequest = UnityWebRequest.Get(csvUrl);
            yield return dataRequest.SendWebRequest();

            if (dataRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to get data: " + dataRequest.error);
                yield break;
            }
            string localFilePath = Path.Combine(Application.streamingAssetsPath, sheet.ToString() + ".csv");
            File.WriteAllText(localFilePath, dataRequest.downloadHandler.text);
        }
        Debug.Log("Data updated successfully.");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    private void LoadAllCSVFile()
    {
        languageCodes.Clear();

        var workSheets = Enum.GetValues(typeof(WorkSheet));
        foreach (WorkSheet sheet in workSheets)
        {
            string localFilePath = Path.Combine(Application.streamingAssetsPath, sheet.ToString() + ".csv");
            if (!File.Exists(localFilePath))
            {
                Debug.LogWarning("Localization CSV not found: " + localFilePath);
                continue;
            }

            if (sheet == WorkSheet.Version)
                ParseCSVVersion(File.ReadAllText(localFilePath), sheet);
            else
                ParseCSV(File.ReadAllText(localFilePath), sheet);
        }
    }

    private bool AllLocalFilesExist()
    {
        var workSheets = Enum.GetValues(typeof(WorkSheet));
        foreach (WorkSheet sheet in workSheets)
        {
            string localFilePath = Path.Combine(Application.streamingAssetsPath, sheet.ToString() + ".csv");
            if (!File.Exists(localFilePath))
                return false;
        }

        return true;
    }


    public class CsvRecordVersion
    {
        public string key { get; set; }
        public string Value { get; set; }
    }


    private void ParseCSV(string csvInfo, WorkSheet workSheet)
    {
        try
        {
            using (StringReader sr = new StringReader(csvInfo))
            using (var csv = new CsvReader(sr, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                if (!csv.Read())
                {
                    Debug.LogError($"Localization CSV '{workSheet}' is empty.");
                    return;
                }

                csv.ReadHeader();
                string[] headers = csv.HeaderRecord;
                if (headers == null || headers.Length < 2)
                {
                    Debug.LogError($"Localization CSV '{workSheet}' must contain a key column and at least one language column.");
                    return;
                }

                int keyColumnIndex = -1;
                List<int> languageColumnIndexes = new List<int>();
                List<string> sheetLanguageCodes = new List<string>();

                for (int i = 0; i < headers.Length; i++)
                {
                    string header = headers[i].Trim().TrimStart('\uFEFF');
                    if (string.Equals(header, "key", StringComparison.OrdinalIgnoreCase))
                    {
                        keyColumnIndex = i;
                    }
                    else if (!string.IsNullOrWhiteSpace(header))
                    {
                        languageColumnIndexes.Add(i);
                        sheetLanguageCodes.Add(header);
                    }
                }

                if (keyColumnIndex < 0)
                {
                    Debug.LogError($"Localization CSV '{workSheet}' does not contain a 'key' column.");
                    return;
                }

                RegisterLanguageCodes(sheetLanguageCodes, workSheet);

                Dictionary<string, string> languageDict = dic[workSheet];
                languageDict.Clear();

                while (csv.Read())
                {
                    string recordKey = csv.GetField(keyColumnIndex)?.Trim();
                    if (string.IsNullOrEmpty(recordKey))
                        continue;

                    for (int i = 0; i < languageColumnIndexes.Count; i++)
                    {
                        string languageCode = sheetLanguageCodes[i];
                        string value = csv.GetField(languageColumnIndexes[i]);

                        if (value != null)
                            languageDict[$"{recordKey}_{languageCode}"] = value;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing the CSV content: " + e.Message);
        }
    }

    private void RegisterLanguageCodes(List<string> sheetLanguageCodes, WorkSheet workSheet)
    {
        if (languageCodes.Count == 0)
        {
            languageCodes.AddRange(sheetLanguageCodes);
            return;
        }

        bool sameLanguages = languageCodes.Count == sheetLanguageCodes.Count;
        for (int i = 0; sameLanguages && i < languageCodes.Count; i++)
        {
            sameLanguages = string.Equals(languageCodes[i], sheetLanguageCodes[i], StringComparison.OrdinalIgnoreCase);
        }

        if (!sameLanguages)
        {
            Debug.LogWarning(
                $"Language columns in '{workSheet}' ({string.Join(", ", sheetLanguageCodes)}) " +
                $"do not match the first localization sheet ({string.Join(", ", languageCodes)})."
            );
        }
    }

    private void ParseCSVVersion(string csvInfo, WorkSheet workSheet)
    {
        try
        {
            using (StringReader sr = new StringReader(csvInfo))
            using (var csv = new CsvReader(sr, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = csv.GetRecords<CsvRecordVersion>();
                List<string> headers = new List<string> { "key", "Value" };

                foreach (var record in records)
                {
                    for (int i = 1; i < headers.Count; i++)
                    {
                        string key = $"{record.key}_{headers[i]}";
                        if (!dic.ContainsKey(workSheet))
                        {
                            dic[workSheet] = new Dictionary<string, string>();
                        }

                        string value = record.Value;

                        if (value != null)
                        {
                            dic[workSheet][key] = value;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing the CSV content: " + e.Message);
        }
    }

    private void UpdateLocalizationUpdater()
    {
        if (updateLocalizationUpdaterCoroutine != null)
            StopCoroutine(updateLocalizationUpdaterCoroutine);
        updateLocalizationUpdaterCoroutine = StartCoroutine(UpdateLocalizationUpdaterIEnumerator());
    }

    private IEnumerator UpdateLocalizationUpdaterIEnumerator()
    {
        while (!isInit) yield return null;
        for (int i = 0; i < LocalizationUpdaters.Count; i++)
        {
            LocalizationUpdater localizationUpdater = LocalizationUpdaters[i];
            localizationUpdater.UpdateText.text = GetString(localizationUpdater.workSheet, localizationUpdater.key);
        }
    }

    [InspectorButton("Test_zh-cht")]
    private void Test_zh_cht()
    {
        SetLanguage(0);
    }

    [InspectorButton("Test_en")]
    private void Test_en()
    {
        SetLanguage(1);
    }
}
