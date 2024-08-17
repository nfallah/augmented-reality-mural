using System.Collections;
using UnityEngine;
using TMPro;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [SerializeField]
    private GameObject popupPrefab;

    [SerializeField]
    private float popupDuration = 2f;

    private GameObject currentPopup;
    private Vector3 offset;

    private void Awake()
    {
        // Enforce a singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    private void Update()
    {
        if (currentPopup != null)
        {
            // Update the popup position to follow the user
            currentPopup.transform.position = Camera.main.transform.position + offset;
            LookAtUser(currentPopup);
        }
    }

    // Method to show a popup message
    public void ShowPopup(string message, Vector3 relativePosition)
    {
        // Destroy any existing popup
        if (currentPopup != null)
        {
            Destroy(currentPopup);
        }

        // Instantiate popup object and set its parent to the current game object
        currentPopup = Instantiate(popupPrefab, transform);

        // Set the text of the popup
        TextMeshProUGUI textComponent = currentPopup.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = message;
        }

        // Set the offset for the popup position
        offset = relativePosition;

        // Position the popup
        currentPopup.transform.position = Camera.main.transform.position + offset;

        // Optionally look at the user
        LookAtUser(currentPopup);

        // Destroy the popup after the specified duration
        StartCoroutine(DestroyPopupAfterDelay(popupDuration));
    }

    // Method to make the popup look at the user
    private void LookAtUser(GameObject popup)
    {
        Vector3 direction = popup.transform.position - Camera.main.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        popup.transform.rotation = rotation;
    }

    // Coroutine to destroy the popup after a delay
    private IEnumerator DestroyPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(currentPopup);
    }

    private string GetPopupMessageForUI(GameObject uiElement)
    {
        if (uiElement.CompareTag("StartMenu"))
        {
            return "This is the Start Menu, it shows the select, draw, " +
                "and more features. More takes you to the submenu!";
        }
        else if (uiElement.CompareTag("Submenu"))
        {
            return "This is the Submenu, it shows the rest of the " +
                "features that we have!";
        }
        else if (uiElement.CompareTag("ColorPalette"))
        {
            return "This is the Color Palette, it provides a variety" +
                " of colors to create in! Tap a color pot to see what " +
                "happens to the color of the menu :)";
        }
        else if (uiElement.CompareTag("SmoothnessSlider"))
        {
            return "This is the Smoothness Slider, it provides your " +
                "preferred smoothness level when drawing or " +
                "creating lines.";
        }
        else if (uiElement.CompareTag("SizeSlider"))
        {
            return "This is the Size Slider, this varies the thickness " +
                "of your lines when using the line select tool or drawing " +
                "tool.";
        }

        return "No specific message for this UI element";
    }

    /*
    public void showHelp(GameObject gameObj) 
    {
        ShowPopupForUI(null, "Need some help? We got you! Check out any of the buttons to review the tutorial for using that feature.");    
    }
    */
}
