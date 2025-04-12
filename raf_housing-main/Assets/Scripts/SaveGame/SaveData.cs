using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string Name;
    public float SliderValue;
    public List<HouseData> Houses = new List<HouseData>();
}
