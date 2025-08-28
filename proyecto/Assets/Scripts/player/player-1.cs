using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player1 : MonoBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private float velocidad = 5f;

    // Variables internas
    private float moverHorizontal;
    private Rigidbody2D rb;

    // Se obtiene el Rigidbody2D al inicializar el objeto
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Input por frame (intervalo variable)
    private void Update()
    {
        // GetAxisRaw para respuesta m�s inmediata; si prefieres suavizado usa GetAxis.
        moverHorizontal = Input.GetAxisRaw("Horizontal");
    }

    // F�sica (intervalo fijo)
    private void FixedUpdate()
    {
        // Mantener la velocidad vertical actual (gravedad, salto, etc.)
        rb.linearVelocity = new Vector2(moverHorizontal * velocidad, rb.linearVelocity.y);
    }
}

