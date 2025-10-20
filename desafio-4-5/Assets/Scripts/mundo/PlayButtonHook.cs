using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra c�mo asignar el onClick del bot�n por c�digo apuntando al MainController.
/// �til si prefieres hacerlo desde script en vez del Inspector.
/// </summary>
public class PlayButtonHook : MonoBehaviour
{
    public Button playButton;              // asignar en inspector (o obtener por GetComponent)
    public MainController mainController;  // referencia al MainController en MainUI

    void Start()
    {
        if (playButton == null || mainController == null)
        {
            Debug.LogError("PlayButtonHook: Asigna playButton y mainController en el Inspector.");
            return;
        }

        // A�adimos la llamada a LoadNextScene cuando se haga click
        playButton.onClick.AddListener(mainController.LoadNextScene);
    }
}
