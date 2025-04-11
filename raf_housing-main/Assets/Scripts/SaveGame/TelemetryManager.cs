using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using NUnit.Framework;
using System.Text;

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

        parameters["eventName"] = eventName;
        parameters["sessionId"] = System.Guid.NewGuid().ToString();
        parameters["deviceTime"] = System.DateTime.UtcNow.ToString("o");

        eventQueue.Enqueue(parameters);

        if (!isSending) StartCoroutine(SendEvents());

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
    public void LogSave(string eventName, Dictionary<string, object> parameters = null)
    {
        if (parameters == null)
        {
            parameters = new Dictionary<string, object>();
        }

        parameters["eventName"] = eventName;

        eventQueue.Enqueue(parameters);

        if (!isSending) StartCoroutine(SendSave());

    }

    private IEnumerator SendSave()
    {
        isSending = true;

            Dictionary<string, object> currentEvent = eventQueue.Dequeue();
            string payload = JsonUtility.ToJson(new SerializationWrapper(currentEvent));

            using (UnityWebRequest request = new UnityWebRequest(serverSaveURL, "POST"))
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

                }
                else
                {
                    Debug.Log("Request sent: " + payload);
                }
            }
            yield return new WaitForSeconds(0.1f);


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
}
