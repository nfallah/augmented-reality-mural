using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorWheelSpawner : MonoBehaviour
{
    public GameObject colorWheelPrefab;
    public Transform spawnParent;

    void Start()
    {
        if (colorWheelPrefab != null && spawnParent != null)
        {
            // Instantiate the color wheel prefab as a child of the specified parent
            GameObject spawnedColorWheel = Instantiate(colorWheelPrefab, spawnParent);

            // Optional: Adjust the position, rotation, and scale of the spawned prefab
            RectTransform rectTransform = spawnedColorWheel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;
            }
        }
        else
        {
            Debug.LogError("Color wheel prefab or spawn parent is not set.");
        }
    }
}