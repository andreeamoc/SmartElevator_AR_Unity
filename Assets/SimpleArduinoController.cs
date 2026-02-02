using UnityEngine;
using System.IO.Ports;
using System;
using System.Collections;

public class SimpleArduinoController : MonoBehaviour
{
    [Header("Serial Settings")]
    [Tooltip("Portul Arduino (ex: COM3, COM4) - se detectează automat dacă nu e disponibil")]
    public string portName = "COM5";
    public int baudRate = 9600;

    [Header("Status")]
    public bool isConnected = false;
    public string lastMessage = "";
    public int currentFloor = 0;

    private SerialPort serialPort;

    // Singleton
    public static SimpleArduinoController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ConnectToArduino();
        StartCoroutine(ReadLoop());
    }

    public void ConnectToArduino()
    {
        try
        {
            string[] availablePorts = SerialPort.GetPortNames();

            if (availablePorts.Length == 0)
            {
                Debug.LogError("❌ Niciun port serial nu a fost găsit!");
                isConnected = false;
                return;
            }

            string portToUse = availablePorts[0];

            serialPort = new SerialPort(portToUse, baudRate);
            serialPort.ReadTimeout = 10;      // FOARTE SCURT
            serialPort.WriteTimeout = 10;
            serialPort.NewLine = "\n";
            serialPort.ReadBufferSize = 4096; // Buffer mai mare
            serialPort.Open();

            isConnected = true;
            Debug.Log($"✅ Conectat la Arduino pe portul {portToUse}");
        }
        catch (Exception e)
        {
            isConnected = false;
            Debug.LogError($"❌ Eroare: {e.Message}");
        }
    }

    // ==========================
    // 🔹 FUNCȚII DE CONTROL LIFT
    // ==========================

    public void GoToFloor(int floor)
    {
        SendCommand($"GOTO {floor - 1}");
    }

    public void ResetLift()
    {
        SendCommand("RESET");
    }

    public void RequestStatus()
    {
        SendCommand("STATUS");
    }

    private void SendCommand(string command)
    {
        if (!isConnected || serialPort == null || !serialPort.IsOpen)
        {
            Debug.LogWarning("⚠️ Arduino nu e conectat!");
            return;
        }

        try
        {
            serialPort.WriteLine(command);
            Debug.Log($"📤 Trimis: {command}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Eroare la trimitere: {e.Message}");
        }
    }

    // ==========================
    // 🔹 CITIRE SERIAL (continuă)
    // ==========================

    private IEnumerator ReadLoop()
    {
        while (true)
        {
            if (isConnected && serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    string line = serialPort.ReadLine();
                    line = line.Trim();
                    if (!string.IsNullOrEmpty(line))
                        HandleArduinoMessage(line);
                }
                catch (TimeoutException) { }
                catch (Exception e)
                {
                    Debug.LogWarning($"⚠️ Citire serial eșuată: {e.Message}");
                }
            }
            yield return null;
        }
    }

    private void HandleArduinoMessage(string message)
    {
        lastMessage = message;
        Debug.Log($"📥 Arduino: {message}");

        // Exemple de mesaje: FLOOR|1, STATUS|0, DONE|2, RESET_DONE
        if (message.StartsWith("FLOOR|"))
        {
            int floor = int.Parse(message.Substring(6));
            currentFloor = floor;
            Debug.Log($"🏢 Liftul este acum la etajul {floor}");
        }
        else if (message.StartsWith("STATUS|"))
        {
            int floor = int.Parse(message.Substring(7));
            currentFloor = floor;
            Debug.Log($"📊 Status actualizat: etaj {floor}");
        }
        else if (message.StartsWith("DONE|"))
        {
            int floor = int.Parse(message.Substring(5));
            Debug.Log($"✅ Lift ajuns la etajul {floor}");
            currentFloor = floor;
        }
        else if (message.Contains("RESET_DONE"))
        {
            currentFloor = 0;
            Debug.Log("🔄 Lift resetat la parter.");
        }
    }

    // ==========================
    // 🔹 Simulări utile (pt test)
    // ==========================

    public void SimulateFloor1Card() => GoToFloor(1);
    public void SimulateFloor2Card() => GoToFloor(2);

    // Ex: când e detectată o mașină (sau alt eveniment)
    public void OnCarDetected()
    {
        Debug.Log("🚗 Mașină detectată - cobor liftul la parter...");
        GoToFloor(0);
    }

    private void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("🔌 Deconectat de la Arduino");
        }
    }
}