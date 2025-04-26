using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string UserId;
    public float SliderValue;
    public string Score;
    public List<HouseData> Houses = new List<HouseData>();
}
