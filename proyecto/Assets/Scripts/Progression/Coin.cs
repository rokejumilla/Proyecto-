using UnityEngine;
using UnityEngine.Events;

public class Coin : MonoBehaviour
{
    [Tooltip("Valor en monedas que otorga")] public int coinValue = 1;
    [Tooltip("Opcional: EXP que da al recoger")] public int expValue = 0;
    [Tooltip("Opcional: Item ScriptableObject para inventario")] public Item itemData;
    public UnityEvent OnCollected; // conecta animaciones/sonidos desde inspector

    bool collected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        // Añadir item si existe
        if (itemData != null)
            ProgressionManager.Instance.AddItemToInventory(itemData);

        // Añadir monedas
        if (coinValue != 0)
            ProgressionManager.Instance.AddCoins(coinValue);

        // Añadir EXP si corresponde
        if (expValue != 0)
            ProgressionManager.Instance.GainExperience(expValue);

        OnCollected?.Invoke();

        // Efectos opcionales: desactivar visual y luego destruir
        GetComponent<Collider2D>().enabled = false;
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = false;

        // Destruir despues de un frame para que el evento se ejecute
        Destroy(gameObject, 0.05f);
    }
}
