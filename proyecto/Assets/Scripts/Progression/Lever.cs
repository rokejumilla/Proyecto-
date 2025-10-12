using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [Tooltip("ID único para esta palanca (usa IDs distintos por cada palanca)")]
    public int leverId = 0;

    [Tooltip("Puentes u objetos a togglear")]
    public GameObject[] bridges;

    [Tooltip("Si true: cuando la palanca está ACTIVADA -> el puente se OCULTA")]
    public bool hideWhenActivated = true;

    public UnityEvent<bool> OnToggled; // pasa el nuevo estado (true=activada)

    void Start()
    {
        // Aplicar estado inicial guardado en ProgressionManager
        if (ProgressionManager.Instance != null && bridges != null)
        {
            bool state = ProgressionManager.Instance.GetLeverState(leverId);
            ApplyStateToBridges(state);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        bool newState = ProgressionManager.Instance.ToggleLever(leverId);
        ApplyStateToBridges(newState);
        OnToggled?.Invoke(newState);

        // Opcional: reproducir animación/sonido aquí o via inspector en OnToggled
    }

    void ApplyStateToBridges(bool leverActivated)
    {
        // Si hideWhenActivated == true: activated => ocultar puente => SetActive(false)
        bool bridgeActive = hideWhenActivated ? !leverActivated : leverActivated;

        foreach (var b in bridges)
        {
            if (b == null) continue;
            b.SetActive(bridgeActive);
        }
    }
}
