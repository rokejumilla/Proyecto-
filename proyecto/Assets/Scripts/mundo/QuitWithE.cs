using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Presiona 'E' para guardar (si hay ProgressionManager) y salir.
/// Adjuntar a cualquier GameObject.
/// </summary>
public class QuitWithE : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Si tienes un ProgressionManager que maneja el guardado, úsalo:
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.SaveAndQuit();
                return;
            }

            // Fallback: detener el modo Play en el Editor o salir en build
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
