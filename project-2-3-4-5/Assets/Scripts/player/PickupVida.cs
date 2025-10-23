using System.Collections;
using System.Reflection;
using UnityEngine;

public class PickupVida : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Cuántas unidades de vida entrega este pickup")]
    [SerializeField] private int cantidadVida = 1;

    [Tooltip("Si true, el pickup se destruye al recoger. Si false y respawnTime > 0, reaparecerá pasado respawnTime")]
    [SerializeField] private bool destruirAlRecoger = true;

    [Tooltip("Si destruirAlRecoger = false y respawnTime > 0, se reactivará después de este tiempo (segundos). 0 = no respawnear")]
    [SerializeField] private float respawnTime = 0f;

    [Header("SFX")]
    [SerializeField] private AudioClip sfxRecoger;
    [Range(0f, 1f)]
    [SerializeField] private float volumenSfx = 1f;

    // Estado interno
    private bool disponible = true;
    private Collider2D miCollider;

    private void Awake()
    {
        miCollider = GetComponent<Collider2D>();
        if (miCollider == null)
            Debug.LogWarning($"[PickupVida] Este GameObject ({name}) no tiene Collider2D. Añade uno y marca 'Is Trigger' para que funcione correctamente.");
        else if (!miCollider.isTrigger)
            Debug.LogWarning($"[PickupVida] El Collider2D en {name} no tiene 'Is Trigger' activado. Es recomendable activarlo para pickups.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!disponible) return;

        // Intentamos darle vida al otro gameobject (player)
        bool exito = TryGiveLife(other.gameObject);

        if (!exito)
        {
            // Opcional: si el objeto no tiene componente de vida, sólo aceptamos si tag == "Player" y no se encontró cómo
            // Puedes comentar/editar esta parte si quieres comportamiento distinto.
            // Debug.Log($"[PickupVida] No se encontró cómo otorgar vida al objeto {other.gameObject.name}.");
            return;
        }

        // Reproducir SFX (si está asignado)
        if (sfxRecoger != null)
        {
            AudioSource.PlayClipAtPoint(sfxRecoger, transform.position, volumenSfx);
        }

        // Destruir o desactivar temporalmente
        if (destruirAlRecoger)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(HandlePickupConsumed());
        }
    }

    private IEnumerator HandlePickupConsumed()
    {
        disponible = false;

        // Desactivar renderers y collider
        if (miCollider != null) miCollider.enabled = false;

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;

        if (respawnTime > 0f)
        {
            yield return new WaitForSeconds(respawnTime);
            // Reactivar
            if (miCollider != null) miCollider.enabled = true;
            foreach (var r in renderers) r.enabled = true;
            disponible = true;
        }
        else
        {
            // Si no hay respawn, mantener desactivado
            yield break;
        }
    }

    /// <summary>
    /// Intenta entregar vida al objeto recibido. Devuelve true si tuvo éxito.
    /// Usa reflexión para ser flexible con diferentes nombres/firmas.
    /// </summary>
    private bool TryGiveLife(GameObject target)
    {
        if (target == null) return false;

        // Nombres de métodos comunes que pueden existir en el jugador
        string[] methodNames = new string[] {
            "SumarVida", "AddLife", "IncreaseLife", "Heal", "AddHealth", "RestoreHealth", "GiveLife", "GainLife"
        };

        // Primero revisamos todos los componentes del gameObject
        Component[] comps = target.GetComponents<Component>();
        foreach (var comp in comps)
        {
            if (comp == null) continue; // skip components missing (por ejemplo scripts eliminados)

            var type = comp.GetType();

            // 1) Buscar métodos con un parámetro int/float
            foreach (var mName in methodNames)
            {
                MethodInfo mi = type.GetMethod(mName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi != null)
                {
                    var parms = mi.GetParameters();
                    try
                    {
                        if (parms.Length == 1)
                        {
                            // si acepta int o float, invocamos con la cantidad
                            if (parms[0].ParameterType == typeof(int))
                            {
                                mi.Invoke(comp, new object[] { cantidadVida });
                                return true;
                            }
                            else if (parms[0].ParameterType == typeof(float))
                            {
                                mi.Invoke(comp, new object[] { (float)cantidadVida });
                                return true;
                            }
                        }
                        else if (parms.Length == 0)
                        {
                            // método sin parámetros (quizás ya conoce cuánto sumar internamente)
                            mi.Invoke(comp, null);
                            return true;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[PickupVida] Error invocando {mName} en {type.Name}: {e.Message}");
                    }
                }
            }

            // 2) Buscar campo "vida" o "health" (int/float) y modificarlo directamente
            FieldInfo fi = type.GetField("vida", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         ?? type.GetField("health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null && (fi.FieldType == typeof(int) || fi.FieldType == typeof(float)))
            {
                try
                {
                    if (fi.FieldType == typeof(int))
                    {
                        int cur = (int)fi.GetValue(comp);
                        fi.SetValue(comp, cur + cantidadVida);
                    }
                    else
                    {
                        float cur = (float)fi.GetValue(comp);
                        fi.SetValue(comp, cur + cantidadVida);
                    }
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[PickupVida] Error modificando campo {fi.Name} en {type.Name}: {e.Message}");
                }
            }

            // 3) Buscar property "vida"/"health"
            PropertyInfo pi = type.GetProperty("vida", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                              ?? type.GetProperty("health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (pi != null && pi.CanRead && pi.CanWrite && (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(float)))
            {
                try
                {
                    if (pi.PropertyType == typeof(int))
                    {
                        int cur = (int)pi.GetValue(comp, null);
                        pi.SetValue(comp, cur + cantidadVida, null);
                    }
                    else
                    {
                        float cur = (float)pi.GetValue(comp, null);
                        pi.SetValue(comp, cur + cantidadVida, null);
                    }
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[PickupVida] Error modificando propiedad {pi.Name} en {type.Name}: {e.Message}");
                }
            }
        }

        // 4) Si no lo encontraste en el objeto, intenta buscar en el padre (un nivel) — útil si el collider está en un child
        if (target.transform.parent != null)
        {
            Component[] parentComps = target.transform.parent.GetComponents<Component>();
            foreach (var comp in parentComps)
            {
                if (comp == null) continue;
                var type = comp.GetType();

                // repetir una versión reducida de búsqueda (sólo métodos comunes y campos)
                foreach (var mName in methodNames)
                {
                    MethodInfo mi = type.GetMethod(mName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (mi != null)
                    {
                        var parms = mi.GetParameters();
                        try
                        {
                            if (parms.Length == 1)
                            {
                                if (parms[0].ParameterType == typeof(int))
                                {
                                    mi.Invoke(comp, new object[] { cantidadVida });
                                    return true;
                                }
                                else if (parms[0].ParameterType == typeof(float))
                                {
                                    mi.Invoke(comp, new object[] { (float)cantidadVida });
                                    return true;
                                }
                            }
                            else if (parms.Length == 0)
                            {
                                mi.Invoke(comp, null);
                                return true;
                            }
                        }
                        catch { /* ignore */ }
                    }
                }

                FieldInfo fi = type.GetField("vida", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             ?? type.GetField("health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null && (fi.FieldType == typeof(int) || fi.FieldType == typeof(float)))
                {
                    try
                    {
                        if (fi.FieldType == typeof(int))
                        {
                            int cur = (int)fi.GetValue(comp);
                            fi.SetValue(comp, cur + cantidadVida);
                        }
                        else
                        {
                            float cur = (float)fi.GetValue(comp);
                            fi.SetValue(comp, cur + cantidadVida);
                        }
                        return true;
                    }
                    catch { }
                }
            }
        }

        // No encontró nada
        return false;
    }
}
