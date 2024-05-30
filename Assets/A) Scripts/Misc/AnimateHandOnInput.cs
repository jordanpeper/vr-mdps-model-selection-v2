using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    // Public variables
    public InputActionProperty pinchAnimationAction; // Interaction input button
    public InputActionProperty gripAnimationAction; // Interaction input button
    public Animator handAnimator; // Animation of hand

    void Update()
    {
        float triggerValue = pinchAnimationAction.action.ReadValue<float>(); // Read the value of the input
        float gripValue = gripAnimationAction.action.ReadValue<float>(); // Read the value of the input

        handAnimator.SetFloat("Trigger", triggerValue); // Animate hand based on input value
        handAnimator.SetFloat("Grip", gripValue); // Animate hand based on input value
    }
}
