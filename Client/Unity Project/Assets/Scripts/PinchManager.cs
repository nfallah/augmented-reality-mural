using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using System;
using UnityEngine;

public class PinchManager : MonoBehaviour
{
    public static PinchManager Instance { get; private set; }

    [SerializeField]
    private bool gridOnStart;

    [SerializeField]
    private ControllerLookup controllerLookup;

    [Range(0f, 1f)]
    [SerializeField]
    private float pinchSensitivity;

    public Action<bool> OnHandPinched, OnHandReleased;

    private IHandsAggregatorSubsystem handsAggregatorSubsystem;

    private ArticulatedHandController leftHand, rightHand;

    private bool grid, leftStatusOld, rightStatusOld;

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

        handsAggregatorSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<IHandsAggregatorSubsystem>();
        leftHand = (ArticulatedHandController)controllerLookup.LeftHandController;
        rightHand = (ArticulatedHandController)controllerLookup.RightHandController;
        grid = gridOnStart;
    }

    private void Update()
    {
        bool leftStatusNew = IsPinched(leftHand);
        bool rightStatusNew = IsPinched(rightHand);

        // Started pinching left hand
        if (leftStatusNew == true && leftStatusOld == false)
        {
            OnHandPinched?.Invoke(true); 
        }
        // Stopped pinching left hand
        else if (leftStatusNew == false && leftStatusOld == true)
        {
            OnHandReleased?.Invoke(true);
        }

        // Started pinching right hand
        if (rightStatusNew == true && rightStatusOld == false)
        {
            OnHandPinched?.Invoke(false);
        }
        // Stopped pinching right hand
        else if (rightStatusNew == false && rightStatusOld == true)
        {
            OnHandReleased?.Invoke(false);
        }

        leftStatusOld = leftStatusNew;
        rightStatusOld = rightStatusNew;
    }

    private bool IsPinched(ArticulatedHandController hand)
    {
        return hand.currentControllerState.selectInteractionState.value > pinchSensitivity;
    }

    public Vector3 LeftPosition
    {
        get
        {
            Vector3 pos = handsAggregatorSubsystem.TryGetPinchingPoint(leftHand.HandNode, out var jointPose) ?
                          jointPose.Position : Vector3.zero;
            return grid ? MathUtils.Vector3Truncate(pos, 0.1f) : pos;
        }
    }

    public Vector3 RightPosition
    {
        get
        {
            Vector3 pos = handsAggregatorSubsystem.TryGetPinchingPoint(rightHand.HandNode, out var jointPose) ?
                          jointPose.Position : Vector3.zero;
            return grid ? MathUtils.Vector3Truncate(pos, 0.1f) : pos;
        }
    }
}