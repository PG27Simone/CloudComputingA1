using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SessionTracker : MonoBehaviour
{

    private float sessionStartTime = 0f;

    public TMP_Text myScore;

    void Start()
    {
        sessionStartTime = Time.time;

        TelemetryManager.Instance.LogEvent("session_start", new Dictionary<string, object>
        {
            {"startTime", System.DateTime.UtcNow.ToString("o") }
        });
    }

    private void OnApplicationQuit()
    {
        float sessionDuration = Time.time - sessionStartTime;
        TelemetryManager.Instance.LogEvent("session_end", new Dictionary<string, object>
        {
            { "score", myScore.text },
            { "durationSec", sessionDuration },
            {"endTime", System.DateTime.UtcNow.ToString("o") }
        });
    }
}
