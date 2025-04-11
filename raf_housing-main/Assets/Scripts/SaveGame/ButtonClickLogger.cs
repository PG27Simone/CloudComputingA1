using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using static UnityEditor.PlayerSettings;
using UnityEditor.PackageManager.Requests;

public class ButtonClickLogger : MonoBehaviour
{

    public Button myButton;
    public Slider mySlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClicked);
        }
    }


    private void OnButtonClicked()
    {
        try
        {
            string username = SessionManager.Instance.GetUsername();

            List<GameObject> houses = FindAllInLayer("Houses");

            List<string> housePositions = new List<string>();
            foreach (GameObject house in houses)
            {
                Vector3 position = house.transform.position;
                housePositions.Add($"({position.x}, {position.y}, {position.z})");
            }

            List<string> houseRotations = new List<string>();
            foreach (GameObject house in houses)
            {
                Quaternion rotation = house.transform.rotation;
                houseRotations.Add($"({rotation.x}, {rotation.y}, {rotation.z})");
            }

            TelemetryManager.Instance.LogSave("save_button", new Dictionary<string, object>
        {
            {"userName", username},
            {"buttonName", myButton.name},
            {"sliderLocation", mySlider.value },
            {"housePositions", string.Join(", ", housePositions) },
            {"houseRotations", string.Join(", ", houseRotations) }

        });
        }
        catch
        {
            Debug.LogError("Cant save. Not logged in.");
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
