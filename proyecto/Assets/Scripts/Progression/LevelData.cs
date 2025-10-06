using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Progression/LevelData", fileName = "LevelData_")]
public class LevelData : ScriptableObject
{
    [Header("Identificaci�n")]
    public string levelID; // opcional, �til para saves
    public string displayName;
    public Sprite thumbnail;

    [Header("Escena (opcional)")]
    [Tooltip("Si el nivel tiene su propia escena, asigna el buildIndex o deja -1 para usar la escena actual")]
    public int sceneBuildIndex = -1;

    [Header("Tiempo y objetivo")]
    [Tooltip("Tiempo l�mite en segundos. 0 = sin l�mite")]
    public float timeLimit = 0f;
    public bool hasObjective = false;
    public string objectiveDescription;

    [Header("Spawns / Prefabs")]
    public List<SpawnEntry> spawns = new List<SpawnEntry>();

    [Header("Audio / Visual")]
    public AudioClip music;
    public Color ambientColor = Color.white;

    [Header("Opciones")]
    public bool locked = false; // �til para dise�o de niveles
    public int recommendedScoreToUnlock = 0;
}

[System.Serializable]
public class SpawnEntry
{
    public string name = "Spawn";
    public GameObject prefab;
    public Vector3 position;
    public Vector3 eulerRotation = Vector3.zero;
    [Tooltip("Cantidad a instanciar (1 por defecto)")]
    public int count = 1;
    [Tooltip("Separaci�n entre instancias (si count>1)")]
    public Vector3 spacing = Vector3.zero;
}
