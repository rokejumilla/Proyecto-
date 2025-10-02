using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    public List<LevelData> levels = new List<LevelData>();
    public int startLevelIndex = 0;

    private int currentLevelIndex = 0;
    private GameObject container;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (levels.Count == 0) return;
        currentLevelIndex = Mathf.Clamp(startLevelIndex, 0, levels.Count - 1);
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Count) return;

        if (container != null) Destroy(container);
        container = new GameObject("LevelObjects");

        LevelData ld = levels[index];

        // Ajustar temporizador
        var timer = FindObjectOfType<GameTimer>();
        if (timer != null) { timer.SetTime(ld.timeLimit); timer.ResetTimer(); }

        // Instanciar plataformas
        if (ld.platformPrefab != null && ld.platformPositions != null)
            foreach (var p in ld.platformPositions)
                Instantiate(ld.platformPrefab, p, Quaternion.identity, container.transform);

        // Instanciar enemigos
        if (ld.enemyPrefabs != null && ld.enemySpawnPositions != null)
        {
            int count = Mathf.Min(ld.enemyPrefabs.Length, ld.enemySpawnPositions.Length);
            for (int i = 0; i < count; i++)
                if (ld.enemyPrefabs[i] != null)
                    Instantiate(ld.enemyPrefabs[i], ld.enemySpawnPositions[i], Quaternion.identity, container.transform);
        }

        // Instanciar puerta
        if (ld.doorPrefab != null) Instantiate(ld.doorPrefab, ld.doorPosition, Quaternion.identity, container.transform);

        currentLevelIndex = index;
        Debug.Log($"Cargado nivel: {ld.levelName}");
    }

    public void NextLevel()
    {
        int next = currentLevelIndex + 1;
        if (next < levels.Count) LoadLevel(next);
        else Debug.Log("Todos los niveles completados");
    }

    public void RestartLevel() => LoadLevel(currentLevelIndex);
}
