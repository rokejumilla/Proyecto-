using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [Header("Profile (TDA)")]
    [SerializeField] private ProgressionProfile profile;

    [Header("Inicio")]
    [SerializeField] private int startLevelIndex = 0;

    // runtime
    private int currentLevelIndex = 0;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private const string SAVE_KEY = "progression_profile_json";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (profile == null)
        {
            Debug.LogError("ProgressionManager: profile no asignado en inspector.");
            return;
        }

        LoadProfile();
        currentLevelIndex = Mathf.Clamp(startLevelIndex, 0, Mathf.Max(0, profile.records.Count - 1));
        LoadLevel(currentLevelIndex);
    }

    #region Level Loading
    public void LoadLevel(int idx)
    {
        if (profile == null) return;
        if (idx < 0 || idx >= profile.records.Count)
        {
            Debug.LogWarning("LoadLevel: índice inválido " + idx);
            return;
        }

        var rec = profile.records[idx];
        if (rec == null || rec.levelData == null)
        {
            Debug.LogWarning("LoadLevel: LevelData nulo en record " + idx);
            return;
        }

        ClearCurrentLevel();
        InstantiateLevelFromData(rec.levelData);
        currentLevelIndex = idx;
        Debug.Log($"Cargado nivel {idx} ({rec.levelData.name})");
        // disparar eventos si necesitas
    }

    private void ClearCurrentLevel()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null) Destroy(spawnedObjects[i]);
        }
        spawnedObjects.Clear();
    }

    private void InstantiateLevelFromData(LevelData ld)
    {
        // 1 - Plataformas simples (platformPrefab + platformPositions)
        if (ld.platformPrefab != null && ld.platformPositions != null)
        {
            foreach (var pos in ld.platformPositions)
            {
                var go = Instantiate(ld.platformPrefab, (Vector3)pos, Quaternion.identity);
                spawnedObjects.Add(go);
            }
        }

        // 2 - Enemigos (matching by index)
        if (ld.enemyPrefabs != null && ld.enemySpawnPositions != null)
        {
            int spawnCount = Math.Min(ld.enemyPrefabs.Length, ld.enemySpawnPositions.Length);
            for (int i = 0; i < spawnCount; i++)
            {
                var prefab = ld.enemyPrefabs[i];
                var pos = ld.enemySpawnPositions[i];
                if (prefab != null)
                {
                    var go = Instantiate(prefab, (Vector3)pos, Quaternion.identity);
                    spawnedObjects.Add(go);
                }
            }
        }

        // 3 - Puerta / meta
        if (ld.doorPrefab != null)
        {
            var door = Instantiate(ld.doorPrefab, (Vector3)ld.doorPosition, Quaternion.identity);
            spawnedObjects.Add(door);
        }

        // 4 - Tilemap prefab (si existe)
        if (ld.tilemapPrefab != null)
        {
            var tgo = Instantiate(ld.tilemapPrefab);
            spawnedObjects.Add(tgo);
        }
        // 5 - Tilemap desde CSV
        else if (ld.tilemapCSV != null && ld.tilePalette != null && ld.tilePalette.Length > 0)
        {
            GameObject loaderPrefab = Resources.Load<GameObject>("TilemapLoaderPrefab");
            if (loaderPrefab != null)
            {
                var inst = Instantiate(loaderPrefab);
                spawnedObjects.Add(inst);
                var loader = inst.GetComponent<TilemapLoader>();
                if (loader != null)
                {
                    loader.LoadFromCSV(ld.tilemapCSV, ld.tilePalette);
                }
                else Debug.LogWarning("TilemapLoaderPrefab no tiene TilemapLoader component.");
            }
            else
            {
                Debug.LogWarning("No se encontró Resources/TilemapLoaderPrefab. Crea un prefab y colócalo en Resources.");
            }
        }

        // 6 - Spawns adicionales (usa spawnOrigin como offset)
        if (ld.additionalPrefabsToSpawn != null)
        {
            foreach (var p in ld.additionalPrefabsToSpawn)
            {
                if (p == null) continue;
                var go = Instantiate(p, (Vector3)ld.spawnOrigin, Quaternion.identity);
                spawnedObjects.Add(go);
            }
        }

        // 7 - Ajustar tiempo si LevelData tiene timeLimit (ejemplo)
        var timer = FindObjectOfType<GameTimer>();
        if (timer != null)
        {
            // Asumimos que GameTimer tiene SetTime(float)
            timer.SetTime(ld.timeLimit);
        }
    }
    #endregion

    #region Progresión API
    public void UnlockLevel(int idx)
    {
        var r = profile.GetRecord(idx);
        if (r != null && !r.unlocked)
        {
            r.unlocked = true;
            SaveProfile();
        }
    }

    public void SetLevelStars(int idx, int stars)
    {
        var r = profile.GetRecord(idx);
        if (r != null && stars > r.stars)
        {
            r.stars = stars;
            SaveProfile();
        }
    }

    public ProgressionProfile.LevelRecord GetCurrentRecord() => profile.GetRecord(currentLevelIndex);

    // --- Método que faltaba: avanzar al siguiente nivel
    public void NextLevel()
    {
        int next = currentLevelIndex + 1;
        if (next < profile.records.Count)
        {
            LoadLevel(next);
        }
        else
        {
            Debug.Log("NextLevel: no hay más niveles.");
            // Aquí podrías reiniciar, volver al menú, etc.
        }
    }

    public bool HasNextLevel()
    {
        return currentLevelIndex + 1 < profile.records.Count;
    }
    #endregion

    #region Persistencia (PlayerPrefs JSON)
    [Serializable]
    private class ProfileSave
    {
        public List<bool> unlocked = new List<bool>();
        public List<int> stars = new List<int>();
    }

    public void SaveProfile()
    {
        try
        {
            ProfileSave save = new ProfileSave();
            foreach (var r in profile.records)
            {
                save.unlocked.Add(r.unlocked);
                save.stars.Add(r.stars);
            }
            string json = JsonUtility.ToJson(save);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("ProgressionManager: perfil guardado.");
        }
        catch (Exception e) { Debug.LogWarning("SaveProfile error: " + e.Message); }
    }

    public void LoadProfile()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;
        try
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            var save = JsonUtility.FromJson<ProfileSave>(json);
            for (int i = 0; i < profile.records.Count && i < save.unlocked.Count; i++)
            {
                profile.records[i].unlocked = save.unlocked[i];
                profile.records[i].stars = save.stars[i];
            }
            Debug.Log("ProgressionManager: perfil cargado.");
        }
        catch (Exception e) { Debug.LogWarning("LoadProfile error: " + e.Message); }
    }

    private void OnApplicationQuit()
    {
        SaveProfile();
    }
    #endregion
}
