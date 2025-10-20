using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainController: m�todo p�blico para cargar la siguiente escena en Build Settings.
/// Adjuntar al GameObject "MainUI" (Canvas).
/// </summary>
public class MainController : MonoBehaviour
{
    [Tooltip("Si es true, al llegar a la �ltima escena vuelve a la primera (�ndice 0).")]
    public bool loopToFirst = true;

    /// <summary>
    /// Carga la siguiente escena seg�n el �ndice en Build Settings.
    /// </summary>
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex; // �ndice de la escena actual
        int nextIndex = currentIndex + 1; // siguiente posici�n

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            if (loopToFirst)
            {
                SceneManager.LoadScene(0); // vuelve a la primera escena
            }
            else
            {
                Debug.LogWarning("LoadNextScene: ya est�s en la �ltima escena y loopToFirst est� desactivado.");
            }
        }
    }

    /// <summary>
    /// Cargar una escena por nombre (alternativa).
    /// </summary>
    /// <param name="sceneName">Nombre de la escena tal como est� en Build Settings.</param>
    public void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("LoadSceneByName: sceneName vac�o.");
    }

    /// <summary>
    /// Recarga la escena actual.
    /// </summary>
    public void ReloadCurrentScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex);
    }
}
