using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ScoopMode : MonoBehaviour
{
    // Public variables
    public GameBehavior gameBehavior;
    public InputActionProperty scoopHoldButton;

    // Private variables
    private BoxCollider _palmCollider;
    private InputActionProperty _scoopHoldButton;
    private List<int> _removalIndices;
    private MatrixPos2 _positionData;

    // Start is called in the first frame
    void Start()
    {
        // Get properties from gameBehavior
        _palmCollider = gameObject.GetComponent<BoxCollider>();
        _positionData = gameBehavior.positionData;
    }

    // Update is called once per frame
    void Update()
    {
        // Read value of scoop hold button
        float scoopButtonValue = scoopHoldButton.action.ReadValue<float>();

        if (scoopHoldButton.action.WasPressedThisFrame())
        {
            // Update global positions
            _positionData.SetGlobalTransform();
        }

        // Perform scoop
        if (scoopButtonValue > 0f)
        {
            // Loop through all positions, allocate indices of contained positions
            List<int> deleteIndices = new List<int>();
            for (int i = 0; i < _positionData.globalMatrix.Count; i++)
            {
                if (_palmCollider.bounds.Contains(_positionData.globalMatrix[i]))
                {
                    deleteIndices.Add(i);
                }
            }

            // Invoke deletion, stall countdown
            if (deleteIndices.Count > 0)
            {
                gameBehavior.DeleteSamples(deleteIndices);
            }
        }
    }
}
