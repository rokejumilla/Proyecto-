using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// GameTimer: cuenta regresiva. Cuando llega a 0 congela la escena (timeScale = 0),
/// pausa audio (opcional), detiene Rigidbody/Animator y muestra un mensaje TMP:
/// "TIEMPO AGOTADO\nPERDISTE!!!"
/// Para usarlo solo necesitas tener TextMeshPro en tu proyecto y un TextMeshProUGUI
/// para el temporizador (el script intenta obtenerlo del mismo GameObject si no se asigna).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class GameTimer : MonoBehaviour
{
    [Header("Ajustes del temporizador")]
    [Tooltip("Segundos iniciales del temporizador")]
    public float startTime = 30f;

    [Tooltip("TMP que mostrar� el reloj. Si no se asigna, se busca en este GameObject.")]
    public TextMeshProUGUI timerText;

    [Header("Mensaje final (opcional)")]
    [Tooltip("TMP que mostrar� el mensaje final. Si est� vac�o, el script lo crear� en la Canvas.")]
    public TextMeshProUGUI messageText;

    [Tooltip("Texto que aparece cuando termina el tiempo")]
    [TextArea]
    public string endMessage = "TIEMPO AGOTADO\nPERDISTE!!!";

    [Tooltip("Pausar audio al terminar")]
    public bool pauseAudio = true;

    float timeRemaining;
    bool finished = false;

    void Awake()
    {
        // Si no asignaste timerText, intenta obtener uno en este GameObject.
        if (timerText == null)
            timerText = GetComponent<TextMeshProUGUI>();

        // Si no hay ninguno, ok � el Start intentar� manejarlo (pero lo ideal es asignarlo).
    }

    void Start()
    {
        timeRemaining = Mathf.Max(0f, startTime);

        if (timerText == null)
        {
            Debug.LogWarning("[GameTimer] No hay TextMeshProUGUI asignado para mostrar el temporizador. A�ade uno o asigna timerText en el inspector.");
        }
        else
        {
            UpdateTimerText();
        }

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (finished) return;

        // Cuenta regresiva normal con Time.deltaTime (funciona solo mientras timeScale > 0)
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerText();
            FinishTimer();
        }
        else
        {
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void FinishTimer()
    {
        finished = true;

        // 1) Congelar el tiempo (detiene la mayor�a de la f�sica y l�gica dependiente de deltaTime)
        Time.timeScale = 0f;

        // 2) Pausar audio si corresponde
        if (pauseAudio)
            AudioListener.pause = true;

        // 3) Detener Rigidbodies y f�sica 2D/3D para "congelar" movimiento inmediato
        var r2ds = FindObjectsOfType<Rigidbody2D>();
        foreach (var r in r2ds)
        {
            r.linearVelocity = Vector2.zero;
            r.angularVelocity = 0f;
            r.simulated = false;
        }

        var r3ds = FindObjectsOfType<Rigidbody>();
        foreach (var r in r3ds)
        {
            r.linearVelocity = Vector3.zero;
            // convertir a kinematic para detener efectos f�sicos (no reversible en este script)
            r.isKinematic = true;
        }

        // 4) Pausar animadores (esto evita que las animaciones sigan en Update)
        var anims = FindObjectsOfType<Animator>();
        foreach (var a in anims)
        {
            a.enabled = false;
        }

        // 5) Mostrar el mensaje final
        ShowEndMessage();
    }

    void ShowEndMessage()
    {
        if (messageText != null)
        {
            messageText.text = endMessage;
            messageText.gameObject.SetActive(true);
            return;
        }

        // Si no te dieron un messageText, creamos uno sencillo en la Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // Crear canvas si no existe
            GameObject cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
        }

        GameObject go = new GameObject("GameTimer_Message");
        go.transform.SetParent(canvas.transform, false);

        TextMeshProUGUI msg = go.AddComponent<TextMeshProUGUI>();
        msg.alignment = TextAlignmentOptions.Center;
        msg.enableWordWrapping = true;
        msg.text = endMessage;
        msg.fontSize = 72; // puedes ajustar desde el inspector si prefieres crear messageText manualmente
        msg.raycastTarget = false;

        RectTransform rt = msg.rectTransform;
        rt.anchorMin = new Vector2(0.1f, 0.35f);
        rt.anchorMax = new Vector2(0.9f, 0.7f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // M�todo p�blico para forzar el fin desde otro script (�til para pruebas)
    public void ForceFinish()
    {
        if (!finished)
        {
            timeRemaining = 0f;
            FinishTimer();
        }
    }
}
