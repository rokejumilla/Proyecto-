using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int lives = 5;
    [SerializeField] private int maxLives = 5; // opcional: tope de vidas

    // Event que notifica la cantidad de vidas (int)
    public UnityEvent<int> OnLivesChanged;

    // Propiedad pública para que otros scripts (HUD) puedan leer las vidas
    public int Lives => lives;

    private void Awake()
    {
        if (OnLivesChanged == null) OnLivesChanged = new UnityEvent<int>();
        // asegurar que maxLives no sea menor que lives
        if (maxLives < lives) maxLives = lives;
    }

    private void Start()
    {
        OnLivesChanged.Invoke(lives);
    }

    public void LoseLife()
    {
        lives--;
        OnLivesChanged.Invoke(lives);

        if (lives <= 0)
        {
            Debug.Log("Game Over");
            // manejar Game Over aquí
        }
    }

    // --- NUEVO: método público para sumar vidas ---
    public void AddLife(int amount)
    {
        if (amount <= 0) return;
        lives = Mathf.Min(lives + amount, maxLives);
        OnLivesChanged.Invoke(lives);
        Debug.Log($"[PlayerHealth] +{amount} vidas. Vidas actuales: {lives}/{maxLives}");
    }
}
