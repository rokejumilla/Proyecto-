using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Progression/LevelData", fileName = "LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Identidad")]
    public string levelName = "Nivel 1";
    public float timeLimit = 30f;

    [Header("Plataformas (instanciación directa)")]
    public GameObject platformPrefab;
    public Vector2[] platformPositions;

    [Header("Enemigos")]
    public GameObject[] enemyPrefabs;
    public Vector2[] enemySpawnPositions;

    [Header("Puerta / Meta")]
    public GameObject doorPrefab;
    public Vector2 doorPosition;

    [Header("Tilemap / Nivel")]
    [Tooltip("Prefab que contenga Grid + Tilemap (editor-friendly). Si usas esto, se instanciará al cargar el nivel.")]
    public GameObject tilemapPrefab;

    [Tooltip("Si prefieres generar el Tilemap desde datos, asigna aquí un CSV (cada celda = índice en tilePalette; -1 = vacío).")]
    public TextAsset tilemapCSV;

    [Tooltip("Paleta de Tiles usada cuando cargas desde CSV (cada índice en CSV apunta a esta lista).")]
    public TileBase[] tilePalette;

    [Header("Spawns adicionales")]
    [Tooltip("Prefabs adicionales a instanciar al cargar el nivel (ej: pickups, decoraciones).")]
    public List<GameObject> additionalPrefabsToSpawn = new List<GameObject>();

    [Tooltip("Origen de instanciación (coordenadas local en el mundo). Evita referenciar Transforms de la escena desde SO.")]
    public Vector2 spawnOrigin = Vector2.zero;
}
