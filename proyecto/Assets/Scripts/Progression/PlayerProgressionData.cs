using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProgressionData", menuName = "ScriptableObjects/PlayerProgressionData", order = 1)]
public class PlayerProgressionData : ScriptableObject
{
    [Header("Progresi�n")]
    [SerializeField] int currentLevel = 1;
    [SerializeField] int currentExperience = 0;
    [SerializeField] int experienceToNextLevel = 10;
    [SerializeField, Tooltip("Multiplicador para escalar EXP requerida al subir")] float experienceScale = 1.2f;

    // Lectura p�blica (solo get)
    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public int ExperienceToNextLevel => experienceToNextLevel;

    // M�todo simple para a�adir experiencia (puede invocarse desde ProgressionManager)
    public void AddExperience(int amount)
    {
        currentExperience += amount;
        // no hacemos LevelUp aqu� para dejar la l�gica en el manager si prefieres eventos
        if (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            currentLevel++;
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * experienceScale);
        }
    }
}
