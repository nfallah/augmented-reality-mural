using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MixedReality.Toolkit.SpatialManipulation;  // For TextMeshProUGUI

public class ObjectTool : MonoBehaviour
{
    public static ObjectTool Instance { get; private set; }

    // UI Elements for Object Selection
    //[SerializeField]
    private Transform resultsContainer;

    [SerializeField]
    public GameObject objectResultPrefab;

    // Local objects for demonstration
    [SerializeField]    
    public List<GameObject> localObjects;

    // Reference to the scroll-down menu
    [SerializeField]
    public GameObject scrollDownMenu; // Assign this in the Inspector

    private void Start()
    {
        // Populate the scroll-down menu at the start
        DisplayLocalObjects();
    }

    // Method to display local 3D objects for selection
    public void DisplayLocalObjects()
    {
        foreach (Transform child in resultsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var obj in localObjects)
        {
            GameObject newObjectResult = Instantiate(objectResultPrefab, resultsContainer);
            newObjectResult.GetComponentInChildren<TextMeshProUGUI>().text = obj.name;
            newObjectResult.GetComponent<Button>().onClick.AddListener(() => OnObjectSelected(obj));
        }
    }

    // Handle 3D object selection and placement in the environment
    void OnObjectSelected(GameObject obj)
    {
        GameObject placedObject = Instantiate(obj);
        placedObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.0f;

        // Add MRTK components for interaction
        if (placedObject.GetComponent<ObjectManipulator>() == null)
        {
            placedObject.AddComponent<ObjectManipulator>();
        }

        if (placedObject.GetComponent<BoundsControl>() == null)
        {
            placedObject.AddComponent<BoundsControl>();
        }

        // Hide the scroll-down menu after selecting an object
        scrollDownMenu.SetActive(false);
    }
}
