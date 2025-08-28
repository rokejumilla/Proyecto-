using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VidaMeta : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float vida = 5f;

    /// <summary>
    /// Modifica la vida con un valor positivo o negativo
    /// </summary>
    public void ModificarVida(float puntos)
    {
        vida += puntos;
        Debug.Log("¿Sigue vivo?: " + EstasVivo());
    }

    /// <summary>
    /// Devuelve true si la vida es mayor que 0
    /// </summary>
    private bool EstasVivo()
    {
        return vida > 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Meta")) return;

        Debug.Log("GANASTE");
    }
}
