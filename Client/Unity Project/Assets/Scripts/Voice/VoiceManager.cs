using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using UnityEngine;
using UnityEngine.Events;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance { get; private set; }

    private IKeywordRecognitionSubsystem phraseRecognitionSubsystem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        phraseRecognitionSubsystem = XRSubsystemHelpers.KeywordRecognitionSubsystem;
    }

    public void Register(string keyword, UnityAction action, bool verbose = false)
    {
        phraseRecognitionSubsystem.CreateOrGetEventForKeyword(keyword)?.AddListener(action);
        
        if (verbose == true)
        {
            Debug.Log($"Registered keyword \"{keyword}\"");
        }
    }

    public void Deregister(string keyword, UnityAction action, bool verbose = false)
    {
        phraseRecognitionSubsystem.CreateOrGetEventForKeyword(keyword)?.RemoveListener(action);
        
        if (verbose == true)
        {
            Debug.Log($"Deregistered keyword \"{keyword}\"");
        }
    }
}