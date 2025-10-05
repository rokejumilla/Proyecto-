using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progression/ProgressionProfile", fileName = "ProgressionProfile")]
public class ProgressionProfile : ScriptableObject
{
    [Serializable]
    public class LevelRecord
    {
        public LevelData levelData; // referencia a tu ScriptableObject LevelData
        public bool unlocked = false;
        public int stars = 0;
        public int bestScore = 0;
    }

    [Tooltip("Lista de registros, uno por LevelData")]
    public List<LevelRecord> records = new List<LevelRecord>();

    public int GetUnlockedCount() => records.FindAll(r => r.unlocked).Count;

    public LevelRecord GetRecord(int idx) => (idx >= 0 && idx < records.Count) ? records[idx] : null;
}
