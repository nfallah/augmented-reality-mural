using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings Instance { get; private set; }

    /* These layers are defined as constants within Unity itself; they are copied here for
     * -convenience.
     */
    public const int LAYER_EPHEMERAL =  6;
    public const int LAYER_BRUSH     =  7;
    public const int LAYER_LINE      =  8;
    public const int LAYER_SHAPE     =  9;
    public const int LAYER_TEXT      = 10;

    public float outlineThickness;

    public Material outlineMaterial;

    private void Awake()
    {
        // Enforce a singleton state pattern
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
}