using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class BoxExporter : MonoBehaviour
{
    //// Publicly accessible variables
    public GameBehavior gameBehavior;
    public InputActionProperty deleteButton;

    // Privately accessible variables
    private BoxCollider _collider;
    private int _frameDelay = 75;
    private bool _countdownActive = false;
    private MatrixPos2 _positionData;

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<BoxCollider>();
        _positionData = gameBehavior.positionData;
    }

    // Update is called once per frame
    void Update()
    {
        // Obtain value of button
        bool buttonJustPressed = deleteButton.action.WasPressedThisFrame();
        float buttonValue = deleteButton.action.ReadValue<float>();

        // Logic for button hold detection
        if (!_countdownActive && buttonJustPressed)
        {
            _countdownActive = true;
            _frameDelay = 75;
        }
        else if (_countdownActive && buttonValue > 0)
        {
            _frameDelay--;
        }
        else if (_countdownActive && buttonValue == 0)
        {
            _countdownActive = false;
        }

        // Delete samples contained in box if held
        if (_frameDelay == 0)
        {
            // Update global positions
            _positionData.SetGlobalTransform();
            Vector3 cubeScale = transform.localScale;
            Vector3 cubeCenter = transform.position;

            // Loop through all positions, allocate indices of contained positions
            List<int> deleteIndices = new List<int>();
            for (int i = 0; i < _positionData.globalMatrix.Count; i++)
            {
                if (PointInCube(_positionData.globalMatrix[i]))
                {
                    deleteIndices.Add(i);
                }
            }

            // Invoke deletion, stall countdown
            if (deleteIndices.Count > 0)
            {
                gameBehavior.DeleteSamples(deleteIndices);
            }
            _countdownActive = false;
            _frameDelay = -1;
        }
    }

    bool PointInCube(Vector3 point)
    {
        Vector3 pointInCubeSpace = transform.InverseTransformPoint(point);

        return Mathf.Abs(pointInCubeSpace.x) <= 0.5f &&
               Mathf.Abs(pointInCubeSpace.y) <= 0.5f &&
               Mathf.Abs(pointInCubeSpace.z) <= 0.5f;
    }
}
