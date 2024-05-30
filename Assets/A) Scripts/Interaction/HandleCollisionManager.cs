using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandleCollisionManager : MonoBehaviour
{
    // VARIABLES
    public InputActionProperty _rightPinch;
    public InputActionProperty _leftPinch;
    private bool _isGrabbed;
    private bool _isTriggered;
    private string _grabName;
    public bool isGrabbed
    {
        get { return _isGrabbed; }
    }

    // METHODS
    // Detect if colliders enter
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("hand"))
        {
            _isTriggered = true;
            _grabName = other.name;
        }
    }

    // Detect if colliders exit
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("hand"))
        {
            _isTriggered = false;
        }
    }

    // Update
    void Update()
    {
        if (!_isGrabbed && _isTriggered)
        {
            if ((_grabName == "RHC" && _rightPinch.action.WasPressedThisFrame()) ||
                (_grabName == "LHC" && _leftPinch.action.WasPressedThisFrame()))
            {
                _isGrabbed = true;
            }
        }
        else if (_isGrabbed)
        {
            if ((_grabName == "RHC" && _rightPinch.action.ReadValue<float>() == 0) ||
                (_grabName == "LHC" && _leftPinch.action.ReadValue<float>() == 0))
            {
                _isGrabbed = false;
            }
        }
    }
}
