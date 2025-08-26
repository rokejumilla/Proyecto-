using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vida-meta : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float vida = 5f;

    public void ModificarVida(float puntos)
    {
        vida += puntos;
        Debug.Log(EstasVivo());
    }


    private bool EstasVivo()
    {
        return vida > 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Meta")) { return; }

        Debug.Log("GANASTE");
    }
}


 