using UnityEngine;

public class EraseTool : MonoBehaviour
{
    public static EraseTool Instance { get; private set; }

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

    private void StartTool(bool isLeftHand)
    {

    }

    private void StopTool(bool isLeftHand)
    {

    }

    public void Activate()
    {

    }

    public void Deactivate()
    {

    }
}