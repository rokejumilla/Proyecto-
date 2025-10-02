using UnityEngine;
using UnityEngine.SceneManagement;

public class MainController : MonoBehaviour
{
    // Llamar desde el onClick del bot�n "Jugar"
    public void CargarSiguienteEscena()
    {
        // Obtiene el �ndice de la escena actualmente activa
        int indiceActual = SceneManager.GetActiveScene().buildIndex;

        // Calcula el �ndice de la siguiente escena
        int indiceSiguiente = indiceActual + 1;

        // Comprueba que el �ndice siguiente exista en Build Settings
        if (indiceSiguiente < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(indiceSiguiente);
        }
        else
        {
            Debug.LogWarning($"No hay m�s escenas en Build Settings despu�s de la escena {indiceActual}. �ndiceSiguiente={indiceSiguiente}");
            // Opcional: volver a la escena 0 (portada) en lugar de hacer nada
            // SceneManager.LoadScene(0);
        }
    }

    // Variante �til: cargar por nombre (si prefieres usar nombres)
    public void CargarEscenaPorNombre(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }

    // Variante: carga as�ncrona (�til para pantallas de carga)
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
