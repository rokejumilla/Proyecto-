using UnityEngine;

[CreateAssetMenu(menuName = "Progression/LevelData", fileName = "LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName = "Nivel 1";
    public float timeLimit = 30f;

    public GameObject platformPrefab;
    public Vector2[] platformPositions;

    public GameObject[] enemyPrefabs;
    public Vector2[] enemySpawnPositions;

    public GameObject doorPrefab;
    public Vector2 doorPosition;
}
