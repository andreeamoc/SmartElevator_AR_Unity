using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Vuforia;

public class ImageTargetDetection : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI scannedListText;

    [Header("Informații obiect")]
    public string objectName = "Obiect";

    [Header("Script pentru detalii (optional)")]
    public TextChange textChangeScript;

    private static HashSet<string> scannedObjects = new HashSet<string>();
    private ObserverBehaviour observerBehaviour;
    private bool wasTracked = false;

    void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();

        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    void OnDestroy()
    {
        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        bool isTracked = (targetStatus.Status == Status.TRACKED ||
                         targetStatus.Status == Status.EXTENDED_TRACKED);

        // Detectează când obiectul devine tracked pentru prima dată
        if (isTracked && !wasTracked)
        {
            OnTargetFound();
        }

        wasTracked = isTracked;
    }

    void OnTargetFound()
    {
        // Adaugă în listă dacă nu există deja
        if (!scannedObjects.Contains(objectName))
        {
            scannedObjects.Add(objectName);
            UpdateScannedList();
        }

        Debug.Log("Target detectat: " + objectName);
    }

    void UpdateScannedList()
    {
        if (scannedListText != null)
        {
            scannedListText.text = "Obiecte scanate:\n\n";

            foreach (string obj in scannedObjects)
            {
                scannedListText.text += "• " + obj + "\n";
            }
        }
    }
}