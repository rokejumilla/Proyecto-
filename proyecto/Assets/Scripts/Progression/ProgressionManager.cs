using UnityEngine;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [Header("Datos")]
    [Tooltip("Puedes usar ProgressionData (recomendado) o asignar niveles directamente")]
    public ProgressionData progressionData;
    public List<LevelData> fallbackLevels = new List<LevelData>(); // si no asignas progressionData

    [Header("Runtime")]
    [Tooltip("Contenedor padre donde se instanciarán los objetos del nivel")]
    public Transform levelContainer;

    [Tooltip("Guardar/recuperar progreso con esta clave")]
    public string playerPrefsKey = "CurrentLevelIndex";

    // ReadOnly eliminado: ahora solo SerializeField para que se vea en el Inspector
    [SerializeField]
    private int currentLevelIndex = 0;

    private GameObject currentLevelHolder;

    public event Action<LevelData, int> OnLevelLoaded;
    public event Action<LevelData, int> OnLevelUnloaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Cargar el progress guardado
        currentLevelIndex = PlayerPrefs.GetInt(playerPrefsKey, 0);
        LoadCurrentLevel();
    }

    private List<LevelData> GetLevelList()
    {
        if (progressionData != null && progressionData.levels != null && progressionData.levels.Count > 0)
            return progressionData.levels;
        return fallbackLevels;
    }

    public void LoadCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        var levels = GetLevelList();
        if (levels == null || levels.Count == 0)
        {
            Debug.LogWarning("[ProgressionManager] No hay niveles asignados.");
            return;
        }

        index = Mathf.Clamp(index, 0, levels.Count - 1);
        currentLevelIndex = index;
        SaveProgress();

        LevelData ld = levels[currentLevelIndex];

        // Si el level tiene escena propia, cargarla (opcional)
        if (ld.sceneBuildIndex >= 0)
        {
            StartCoroutine(LoadSceneAndInstantiate(ld));
        }
        else
        {
            InstantiateLevel(ld);
            OnLevelLoaded?.Invoke(ld, currentLevelIndex);
        }
    }

    private IEnumerator LoadSceneAndInstantiate(LevelData ld)
    {
        // Cargar escena de forma asíncrona (mantiene el ProgressionManager si está en otra escena bajo DontDestroyOnLoad)
        var ao = SceneManager.LoadSceneAsync(ld.sceneBuildIndex, LoadSceneMode.Single);
        while (!ao.isDone) yield return null;

        // Esperar un frame para asegurarse que la escena inicializa
        yield return null;
        InstantiateLevel(ld);
        OnLevelLoaded?.Invoke(ld, currentLevelIndex);
    }

    private void InstantiateLevel(LevelData ld)
    {
        ClearCurrentLevel();

        if (levelContainer == null)
        {
            // Crear un contenedor temporal si no existe
            currentLevelHolder = new GameObject("LevelHolder_" + (ld.displayName ?? currentLevelIndex.ToString()));
        }
        else
        {
            currentLevelHolder = new GameObject("LevelHolder_" + (ld.displayName ?? currentLevelIndex.ToString()));
            currentLevelHolder.transform.SetParent(levelContainer, false);
        }

        foreach (var s in ld.spawns)
        {
            if (s.prefab == null) continue;
            for (int i = 0; i < Mathf.Max(1, s.count); i++)
            {
                Vector3 offset = s.spacing * i;
                Quaternion rot = Quaternion.Euler(s.eulerRotation);
                Vector3 pos = s.position + offset;
                var go = Instantiate(s.prefab, pos, rot, currentLevelHolder.transform);
                go.name = string.IsNullOrEmpty(s.name) ? s.prefab.name : s.name + "_" + i;
            }
        }

        // Opcionales: aplicar música, ambiente, timeLimit en GameTimer, etc
        if (ld.music != null)
        {
            // Busca o crea un AudioSource para reproducir la música del nivel
            var asrc = GetComponent<AudioSource>();
            if (asrc == null) asrc = gameObject.AddComponent<AudioSource>();
            asrc.clip = ld.music;
            asrc.loop = true;
            asrc.Play();
        }

        // Manejar timeLimit -> si tienes un GameTimer, deberías llamar a su API aquí.
        if (ld.timeLimit > 0f)
        {
            var timer = FindObjectOfType<GameTimer>();
            if (timer != null)
            {
                timer.SetTime(ld.timeLimit);
                timer.ResetTimer();
            }
        }
    }

    public void ClearCurrentLevel()
    {
        if (currentLevelHolder != null)
        {
            Destroy(currentLevelHolder);
            currentLevelHolder = null;
        }
        OnLevelUnloaded?.Invoke(null, currentLevelIndex);
    }

    public void NextLevel()
    {
        var levels = GetLevelList();
        if (levels == null || levels.Count == 0) return;
        int next = currentLevelIndex + 1;
        if (next >= levels.Count)
        {
            Debug.Log("[ProgressionManager] Llegaste al final de la colección.");
            return;
        }
        LoadLevel(next);
    }

    public void PrevLevel()
    {
        if (currentLevelIndex <= 0) return;
        LoadLevel(currentLevelIndex - 1);
    }

    public void RestartLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(playerPrefsKey, currentLevelIndex);
        PlayerPrefs.Save();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
        currentLevelIndex = 0;
        SaveProgress();
    }

    // API pública útil para UI / botones
    public int GetCurrentLevelIndex() => currentLevelIndex;
    public int GetTotalLevels() => GetLevelList()?.Count ?? 0;
    public LevelData GetLevelData(int index)
    {
        var list = GetLevelList();
        if (list == null || index < 0 || index >= list.Count) return null;
        return list[index];
    }
}
