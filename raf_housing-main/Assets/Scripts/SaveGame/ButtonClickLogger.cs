using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using static UnityEditor.PlayerSettings;
using UnityEditor.PackageManager.Requests;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using UnityEditor.Overlays;
using TMPro;
using UnityEditor;
using UnityEngine.SceneManagement;

public class ButtonClickLogger : MonoBehaviour
{

    public Button myButton;
    public Button myQuitButton;
    public Slider mySlider;
    public GameObject prefab;
    public TMP_Text myScore;


    private float sessionStartTime = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sessionStartTime = Time.time;

        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClicked);
        }
        if (myQuitButton != null)
        {
            myQuitButton.onClick.AddListener(OnQuitButtonClicked);
        }
    }

    private void OnQuitButtonClicked()
    {

        float sessionDuration = Time.time - sessionStartTime;
        TelemetryManager.Instance.LogEvent("session_end", new Dictionary<string, object>
        {
            { "score", myScore.text },
            { "durationSec", sessionDuration },
            {"endTime", System.DateTime.UtcNow.ToString("o") }
        });
        SceneManager.LoadScene("MainMenu");
    }

    private void Awake()
    {
        LoadData();
    }


    private void OnButtonClicked()
    {

        try
        {
            string userId = SessionManager.Instance.GetUserId();

            SaveData NewSave = new SaveData();
            NewSave.UserId = userId;
            NewSave.Score = myScore.text;
            NewSave.SliderValue = mySlider.value;

            List<GameObject> houses = FindAllInLayer("Houses");

            List<string> housePositions = new List<string>();
            List<string> houseRotations = new List<string>();

            foreach (GameObject house in houses)
            {
                HouseData data = new HouseData
                {
                    Position = house.transform.position,
                    Rotation = house.transform.rotation,
                };

                NewSave.Houses.Add(data);
            }

            TelemetryManager.Instance.LogSave(NewSave);

        }
        catch
        {
            Debug.LogError("Cant save. Not logged in.");
        }
        
    }

    private void LoadData()
    {
        // Set folder path to the UserSaves/SaveData directory
        string folderPath = Path.Combine(Application.persistentDataPath, "SaveData");
        string currentId = SessionManager.Instance.GetUserId();

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("Save folder does not exist: " + folderPath);
            return;
        }

        string[] files = Directory.GetFiles(folderPath, "*.json");

        foreach (string filePath in files)
        {
            string json = File.ReadAllText(filePath);

            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data.UserId == currentId)
            {
                Debug.Log("Found matching file: " + filePath);
                ApplySaveData(data);
                return;
            }
        }

        Debug.LogWarning("No save data found for userID: " + currentId);
    }

    private void ApplySaveData(SaveData data)
    {
        mySlider.value = data.SliderValue;
        myScore.text = data.Score;

        foreach (HouseData house in data.Houses)
        {
            if (prefab != null)
            {
                Instantiate(prefab, house.Position, house.Rotation);
            }
        }
    }


    public static List<GameObject> FindAllInLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"Layer '{layerName}' not found.");
            return new List<GameObject>();
        }

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> results = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == layer)
            {
                if (obj.tag != "Block")
                {
                    results.Add(obj);
                }
            }
        }

        return results;
    }
}
