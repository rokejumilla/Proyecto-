using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ProgressionManager : MonoBehaviour
{
    // --- Singleton ---
    public static ProgressionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // evita duplicados
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // opcional: persiste entre escenas
    }

    // --- Monedas ---
    [Header("Monedas")]
    [SerializeField] private int coins = 0;
    public UnityEvent<int> OnCoinsChanged; // enlaza UI en inspector

    // --- Palancas ---
    private Dictionary<int, bool> leverStates = new Dictionary<int, bool>();

    // Métodos para monedas
    public void AddCoins(int amount)
    {
        if (amount == 0) return;
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
        Debug.Log($"Monedas: {coins}");
    }

    public int GetCoins() => coins;

    // Métodos para palancas
    public bool GetLeverState(int id) => leverStates.ContainsKey(id) && leverStates[id];

    /// <summary>
    /// Cambia el estado de la palanca y devuelve el nuevo estado (true = activada).
    /// </summary>
    public bool ToggleLever(int id)
    {
        bool newState = !GetLeverState(id);
        leverStates[id] = newState;
        Debug.Log($"Lever {id} nuevo estado: {newState}");
        return newState;
    }
}
