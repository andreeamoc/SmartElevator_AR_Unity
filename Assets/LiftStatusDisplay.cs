using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LiftStatusDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Text pentru etajul curent")]
    public TextMeshProUGUI floorText;

    [Tooltip("Text pentru status (mișcare/oprit)")]
    public TextMeshProUGUI statusText;

    [Tooltip("Text pentru log-uri (opțional)")]
    public TextMeshProUGUI logText;

    [Tooltip("Indicator vizual (opțional - cerc colorat)")]
    public Image statusIndicator;

    [Header("Colors")]
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;
    public Color movingColor = Color.yellow;

    private string currentMessage = "";

    private void Start()
    {
        UpdateDisplay();
    }

    private void Update()
    {
        // Verifică dacă au venit mesaje noi de la Arduino
        if (SimpleArduinoController.Instance != null)
        {
            string newMessage = SimpleArduinoController.Instance.lastMessage;

            if (!string.IsNullOrEmpty(newMessage) && newMessage != currentMessage)
            {
                currentMessage = newMessage;
                ProcessArduinoMessage(newMessage);
            }

            UpdateConnectionStatus();
        }
    }

    private void ProcessArduinoMessage(string message)
    {
        // Parsează mesajele de la Arduino și actualizează UI-ul

        // Detectează etajul curent din mesaje
        if (message.Contains("PARTER"))
        {
            UpdateFloor("PARTER");
        }
        else if (message.Contains(" 1"))
        {
            UpdateFloor(" 1");
        }
        else if (message.Contains(" 2"))
        {
            UpdateFloor(" 2");
        }

        // Detectează starea de mișcare
        if (message.Contains("URCĂ") || message.Contains("COBOARĂ") || message.Contains("Pas "))
        {
            UpdateStatus("🔄 În mișcare...", movingColor);
        }
        else if (message.Contains("Ajuns la") || message.Contains("oprit"))
        {
            UpdateStatus("✅ Oprit", connectedColor);
        }

        // Adaugă în log
        AddToLog(message);
    }

    private void UpdateFloor(string floor)
    {
        if (floorText != null)
        {
            floorText.text = $"Etaj: {floor}";
        }
    }

    private void UpdateStatus(string status, Color color)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }

        if (statusIndicator != null)
        {
            statusIndicator.color = color;
        }
    }

    private void UpdateConnectionStatus()
    {
        if (SimpleArduinoController.Instance == null) return;

        if (statusIndicator != null)
        {
            statusIndicator.color = SimpleArduinoController.Instance.isConnected
                ? connectedColor
                : disconnectedColor;
        }
    }

    private void AddToLog(string message)
    {
        if (logText == null) return;

        // Formatează mesajul
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string formattedMessage = $"[{timestamp}] {message}";

        // Adaugă la începutul log-ului
        logText.text = formattedMessage + "\n" + logText.text;

        // Limitează la ultimele 10 linii
        string[] lines = logText.text.Split('\n');
        if (lines.Length > 10)
        {
            System.Array.Resize(ref lines, 10);
            logText.text = string.Join("\n", lines);
        }
    }

    private void UpdateDisplay()
    {
        if (floorText != null)
        {
            floorText.text = "Etaj: ";
        }

        if (statusText != null)
        {
            statusText.text = "Așteptare conexiune...";
        }
    }

    // Butoane pentru testare manuală (poți să le adaugi în UI)
    public void TestGoToFloor1()
    {
        if (SimpleArduinoController.Instance != null)
        {
            SimpleArduinoController.Instance.SimulateFloor1Card();
            Debug.Log("🧪 Test: Trimis card ETAJ 1");
        }
    }

    public void TestGoToFloor2()
    {
        if (SimpleArduinoController.Instance != null)
        {
            SimpleArduinoController.Instance.SimulateFloor2Card();
            Debug.Log("🧪 Test: Trimis card ETAJ 2");
        }
    }
}