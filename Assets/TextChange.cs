using UnityEngine;
using TMPro;

public class TextChange : MonoBehaviour
{
    public TextMeshPro objectNameText;     
    public TextMeshPro objectDetailsText;  

    private bool showingDetails = false;

    void Start()
    {
        if (objectDetailsText != null)
        {
            objectDetailsText.gameObject.SetActive(false);
        }

        if (objectNameText != null)
        {
            objectNameText.gameObject.SetActive(true);
        }
    }

    public void ToggleDetails()
    {
        showingDetails = !showingDetails;

        if (objectNameText != null)
        {
            objectNameText.gameObject.SetActive(!showingDetails);
        }

        if (objectDetailsText != null)
        {
            objectDetailsText.gameObject.SetActive(showingDetails);
        }
    }
}