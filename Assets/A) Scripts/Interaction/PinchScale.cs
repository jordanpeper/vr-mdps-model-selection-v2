using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PinchScale : MonoBehaviour
{
    // Public variables
    public float scaleSensitivity = 1; // Sensitivity of scaling motion; 1 => direct correlation
    public Transform _leftHandTransform; // Left and right hand gameObjects 
    public Transform _rightHandTransform; //
    public InputActionProperty _leftPinch; // Left and right pinch inputs
    public InputActionProperty _rightPinch; //

    // Private variables
    private float _distanceDatum; // Datum of distance between hands
    private Collider _collider; // The collider of the gameObject being scaled
    private GameObject _pseudoPivot;
    private bool _pinchScaleIsRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        // Get the collider of this gameObject
        _collider = GetComponent<Collider>();

        // Create new empty gameObject as pseudoPivot
        _pseudoPivot = new GameObject("PseudoPivot"); 
    }

    // Update is called once per frame
    void Update()
    {
        // Enable when inspection mode is not running, double pinch is made, and hands are bounded by collider
        if (DoublePinchCloud() && HandsAreBounded())
        {
            // Set the datum if pinch scaling was just activated
            if (_leftPinch.action.WasPressedThisFrame() || _rightPinch.action.WasPressedThisFrame())
            {
                _distanceDatum = Vector3.Distance(_leftHandTransform.position, _rightHandTransform.position);

                // Initialize pseudoPivot parameters; set gameObject as child
                _pseudoPivot.transform.position = (_leftHandTransform.position + _rightHandTransform.position) / 2; // Set between hands
                _pseudoPivot.transform.localScale = transform.localScale; // Set the scale to gameObject scale to minimize computation
                transform.SetParent(_pseudoPivot.transform);
            }

            // Set scale factor based on datum hand distance; reset datum
            float distance = Vector3.Distance(_leftHandTransform.position, _rightHandTransform.position);
            float scaleFactor = distance / _distanceDatum;
            _pseudoPivot.transform.localScale *= scaleFactor * scaleSensitivity;
            _distanceDatum = distance;

            // Set activity status
            _pinchScaleIsRunning = true;
        }
        else if (_pinchScaleIsRunning) // Was just deactivated
        {
            _pinchScaleIsRunning = false;
            transform.SetParent(null);
        }
    }

    // Check whether the double pinch action is being made
    private bool DoublePinchCloud()
    {
        float lhPinchValue = _leftPinch.action.ReadValue<float>();
        float rhPinchValue = _rightPinch.action.ReadValue<float>();
        return (lhPinchValue > 0 && rhPinchValue > 0);
    }

    // Check whether the hands are bounded by the collider
    private bool HandsAreBounded()
    {
        Vector3 lhHandPosition = _leftHandTransform.position;
        Vector3 rhHandPosition = _rightHandTransform.position;
        return (_collider.bounds.Contains(lhHandPosition) && _collider.bounds.Contains(rhHandPosition));
    }
}
