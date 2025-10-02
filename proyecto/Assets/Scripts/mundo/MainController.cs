using UnityEngine;
using UnityEngine.SceneManagement;

public class MainController : MonoBehaviour
{
    // Llamar desde el onClick del botón "Jugar"
    public void CargarSiguienteEscena()
    {
        // Obtiene el índice de la escena actualmente activa
        int indiceActual = SceneManager.GetActiveScene().buildIndex;

        // Calcula el índice de la siguiente escena
        int indiceSiguiente = indiceActual + 1;

        // Comprueba que el índice siguiente exista en Build Settings
        if (indiceSiguiente < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(indiceSiguiente);
        }
        else
        {
            Debug.LogWarning($"No hay más escenas en Build Settings después de la escena {indiceActual}. ÍndiceSiguiente={indiceSiguiente}");
            // Opcional: volver a la escena 0 (portada) en lugar de hacer nada
            // SceneManager.LoadScene(0);
        }
    }

    // Variante útil: cargar por nombre (si prefieres usar nombres)
    public void CargarEscenaPorNombre(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }

    // Variante: carga asíncrona (útil para pantallas de carga)
    public void CargarSiguienteEscenaAsync()
    {
        int indiceSiguiente = SceneManager.GetActiveScene().buildIndex + 1;
        if (indiceSiguiente < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(CargarAsync(indiceSiguiente));
        }
    }

    private System.Collections.IEnumerator CargarAsync(int buildIndex)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);
        // opcional: mostrar barra de progreso leyendo op.progress
        while (!op.isDone)
        {
            yield return null;
        }
    }
}
