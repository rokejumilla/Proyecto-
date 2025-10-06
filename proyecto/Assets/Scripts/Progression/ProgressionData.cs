using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Progression/ProgressionData", fileName = "ProgressionData_")]
public class ProgressionData : ScriptableObject
{
    [Tooltip("Lista ordenada de LevelData que componen la campa�a")]
    public List<LevelData> levels = new List<LevelData>();
}
