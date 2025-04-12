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

public class ButtonClickLogger : MonoBehaviour
{

    public Button myButton;
    public Slider mySlider;
    public TMP_Text myScore;
    public GameObject prefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void Awake()
    {
        LoadData();
    }


    private void OnButtonClicked()
    {

        try
        {
            string username = SessionManager.Instance.GetUsername();

            SaveData NewSave = new SaveData();
            NewSave.Name = username;
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
        string path = Path.Combine(Application.streamingAssetsPath, "save.json");

        if(File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            mySlider.value = data.SliderValue;

            foreach (HouseData house in data.Houses)
            {
                if (prefab != null)
                {
                    Instantiate(prefab, house.Position, house.Rotation);
                }
            }
        }
        else
        {
            Debug.LogError("File not found at: " + path);
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
