using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float velocidad = 5f;

    // Variables de uso interno en el script
    private float moverHorizontal;
    private float moverVertical;
    private Vector2 direccion;

    // Referencia al Rigidbody2D
    private Rigidbody2D miRigidbody2D;

    // Se cachea el componente en Awake (se ejecuta antes que OnEnable/Start)
    private void Awake()
    {
        miRigidbody2D = GetComponent<Rigidbody2D>();
        if (miRigidbody2D == null)
        {
            Debug.LogError($"PlayerController: Rigidbody2D no encontrado en '{gameObject.name}'. Añade un Rigidbody2D al GameObject.");
        }
    }

    // Lectura de input en Update (intervalo variable)
    private void Update()
    {
        moverHorizontal = Input.GetAxis("Horizontal");
        moverVertical = Input.GetAxis("Vertical");
        direccion = new Vector2(moverHorizontal, moverVertical);

        // Normalizar para evitar que moverse en diagonal sea más rápido
        if (direccion.sqrMagnitude > 1f) direccion = direccion.normalized;
    }

    // Movimiento físico en FixedUpdate (intervalo fijo)
    private void FixedUpdate()
    {
        if (miRigidbody2D == null) return;
        Vector2 nuevaPos = miRigidbody2D.position + direccion * (velocidad * Time.fixedDeltaTime);
        miRigidbody2D.MovePosition(nuevaPos);
    }
}
