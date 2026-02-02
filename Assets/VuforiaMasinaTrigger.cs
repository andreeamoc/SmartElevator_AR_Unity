using UnityEngine;
using Vuforia;

public class VuforiaMasinaTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Câte secunde între comenzi (pentru a evita spam)")]
    public float cooldownTime = 3f;

    private ObserverBehaviour imageTarget;
    private bool isImageDetected = false;
    private float lastTriggerTime = -999f;

    private void Start()
    {
        // Găsește componenta ObserverBehaviour (de pe ImageTarget)
        imageTarget = GetComponent<ObserverBehaviour>();

        if (imageTarget != null)
        {
            imageTarget.OnTargetStatusChanged += OnImageStatusChanged;
            Debug.Log("✅ Vuforia Trigger pentru mașină activat");
        }
        else
        {
            Debug.LogError("❌ ObserverBehaviour nu a fost găsit! Pune scriptul pe ImageTarget_Masina!");
        }
    }

    private void OnImageStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        // Verifică dacă imaginea e detectată
        bool isTracked = (targetStatus.Status == Status.TRACKED ||
                          targetStatus.Status == Status.EXTENDED_TRACKED);

        if (isTracked && !isImageDetected)
        {
            // Imaginea tocmai a fost detectată
            OnMasinaDetected();
        }

        isImageDetected = isTracked;
    }

    private void OnMasinaDetected()
    {
        Debug.Log("🚗 === MAȘINĂ DETECTATĂ ===");

        // Verifică cooldown (să nu trimită comenzi prea des)
        if (Time.time - lastTriggerTime < cooldownTime)
        {
            float remainingTime = cooldownTime - (Time.time - lastTriggerTime);
            Debug.Log($"⏱️ Cooldown activ. Așteaptă {remainingTime:F1} secunde");
            return;
        }

        // Trimite comanda către Arduino
        if (SimpleArduinoController.Instance != null && SimpleArduinoController.Instance.isConnected)
        {
            Debug.Log("📍 Trimite lift la PARTER");

            // Logica: Dacă liftul e la etaj, scanăm același card pentru a coborî
            // Arduino are logică: dacă e la etaj X și scanezi cardX -> cobori la parter

            // Simulăm scanarea unui card (oricare) - Arduino va gestiona logica
            SimpleArduinoController.Instance.SimulateFloor1Card();

            lastTriggerTime = Time.time;

            Debug.Log("✅ Comandă trimisă - Liftul va coborî la parter");
        }
        else
        {
            Debug.LogWarning("⚠️ Arduino nu e conectat!");
        }
    }

    private void OnDestroy()
    {
        if (imageTarget != null)
        {
            imageTarget.OnTargetStatusChanged -= OnImageStatusChanged;
        }
    }

    //// Pentru debugging - afișează status pe ecran
    //private void OnGUI()
    //{
    //    if (!Debug.isDebugBuild) return;

    //    GUILayout.BeginArea(new Rect(10, 10, 400, 150));
    //    GUILayout.Box("=== VUFORIA STATUS ===");
    //    GUILayout.Label($"Imagine detectată: {(isImageDetected ? "✅ DA" : "❌ NU")}");

    //    float cooldownRemaining = Mathf.Max(0, cooldownTime - (Time.time - lastTriggerTime));
    //    GUILayout.Label($"Cooldown: {cooldownRemaining:F1}s");

    //    if (SimpleArduinoController.Instance != null)
    //    {
    //        GUILayout.Label($"Arduino: {(SimpleArduinoController.Instance.isConnected ? "✅ Conectat" : "❌ Deconectat")}");
    //    }

    //    GUILayout.EndArea();
    ////}
}