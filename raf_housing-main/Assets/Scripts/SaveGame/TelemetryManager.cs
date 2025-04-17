using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using NUnit.Framework;
using System.Text;
using TMPro;
using System.IO;

public class TelemetryManager : MonoBehaviour
{
    string serverURL = "http://localhost:3000/telemetry";
    string serverSaveURL = "http://localhost:3000/save";

    public static TelemetryManager Instance { get; private set; }

    private Queue<Dictionary<string, object>> eventQueue;
    private bool isSending;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            eventQueue = new Queue<Dictionary<string, object>>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //CREATING EVENT FILE
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (parameters == null)
        {
            parameters = new Dictionary<string, object>();
        }

        parameters["username"] = SessionManager.Instance.GetUsername();
        parameters["eventName"] = eventName;
        parameters["sessionId"] = System.Guid.NewGuid().ToString();
        parameters["deviceTime"] = System.DateTime.UtcNow.ToString("o");

        //to save locally
        SaveEventToStreamingAssets(parameters);

        //save to the cloud
        eventQueue.Enqueue(parameters);

        if (!isSending) StartCoroutine(SendEvents());

    }

    //local save to persistent data path
    private void SaveEventToStreamingAssets(Dictionary<string, object> data)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "Events");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        //save per-user event file
        string fileName = SessionManager.Instance.GetUsername() + "_events.json";
        string filePath = Path.Combine(folderPath, fileName);

        //load existing data if it exists
        SerializationListWrapper allEvents = new SerializationListWrapper();

        if (File.Exists(filePath))
        {
            string existingJson = File.ReadAllText(filePath);
            allEvents = JsonUtility.FromJson<SerializationListWrapper>(existingJson);
        }

        //add new event
        SerializationWrapper newEvent = new SerializationWrapper(data);
        allEvents.events.Add(newEvent);

        // Save back to file
        string json = JsonUtility.ToJson(allEvents, true);
        File.WriteAllText(filePath, json);

        Debug.Log("Appended event to: " + filePath);
    }

    private IEnumerator SendEvents()
    {
        isSending = true;

        while (eventQueue.Count > 0)
        {
            Dictionary<string, object> currentEvent = eventQueue.Dequeue();
            string payload = JsonUtility.ToJson(new SerializationWrapper(currentEvent));

            using (UnityWebRequest request = new UnityWebRequest(serverURL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                //TODO: Add bearer token --- "bearer ......."

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError($"Error {request.error}");
                    eventQueue.Enqueue(currentEvent);
                    break;
                }
                else
                {
                    Debug.Log("Request sent: " + payload);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        isSending = false;

    }


    //CREATING SAVE FILE
    public void LogSave(SaveData data)
    {
        if (data == null)
        {
            return;
        }

        SaveDataToFile(data);

        if (!isSending) StartCoroutine(SendSave(data));

    }

    //save locally
    private void SaveDataToFile(SaveData data)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "SaveData");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileName = data.Name + "_save.json";
        string filePath = Path.Combine(folderPath, fileName);

        //serialize to JSON
        string json = JsonUtility.ToJson(data, true); 

        File.WriteAllText(filePath, json);

        Debug.Log("Saved game data to: " + filePath);
    }


    private IEnumerator SendSave(SaveData data)
    {
        isSending = true;

        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest("http://localhost:3000/save", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error sending save data: " + request.error);
            }
            else
            {
                Debug.Log("Save data sent successfully");
            }
        }
        isSending = false;

    }



    [System.Serializable]
    private class SerializationWrapper
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();

        public SerializationWrapper(Dictionary<string, object> parameters)
        {
            foreach (KeyValuePair<string, object> kvp in parameters)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value.ToString());
            }
        }
    }

    [System.Serializable]
    private class SerializationListWrapper
    {
        public List<SerializationWrapper> events = new List<SerializationWrapper>();
    }
}
