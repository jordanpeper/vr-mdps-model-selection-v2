using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CubeTool : MonoBehaviour
{
    // VARIABLES
    public Transform leftHandTransform;
    public Transform rightHandTransform;
    public InputActionProperty _rightGrab;
    public InputActionProperty _leftGrab;
    public HandleCollisionManager[] handles;
    public GameBehavior gameBehavior;

    private bool _scaleIsActive = false;
    private string _direction = "n";
    private float _distanceDatum; // Datum of distance between hands
    private Vector3 _totalScaleFactor;

    // Start is called before the first frame update
    void Start()
    {
        _totalScaleFactor = Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        // Flow of logic to manage slide detection
        bool xyGrab = handles[0].isGrabbed && handles[1].isGrabbed;
        bool yzGrab = handles[2].isGrabbed && handles[3].isGrabbed;
        bool xzGrab = handles[4].isGrabbed && handles[5].isGrabbed;
        if (!_scaleIsActive && xyGrab)
        {
            _scaleIsActive = true;
            _direction = "x";
            SetDistanceDatum(_direction);
            gameBehavior.gameObject.GetComponent<PinchScale>().enabled = false;
        }
        else if (!_scaleIsActive && yzGrab)
        {
            _scaleIsActive = true;
            _direction = "z";
            SetDistanceDatum(_direction);
            gameBehavior.gameObject.GetComponent<PinchScale>().enabled = false;

        }
        else if (!_scaleIsActive && xzGrab)
        {
            _scaleIsActive = true;
            _direction = "y";
            SetDistanceDatum(_direction);
            gameBehavior.gameObject.GetComponent<PinchScale>().enabled = false;
        }
        else if (_scaleIsActive)
        {
            if ((_direction == "x" && !xyGrab) ||
                (_direction == "z" && !yzGrab) ||
                (_direction == "y" && !xzGrab))
            {
                _scaleIsActive = false;
                gameBehavior.gameObject.GetComponent<PinchScale>().enabled = true;
                _direction = "n";
            }
        }

        // Invoke scaling in specified direction
        if (_scaleIsActive)
        {
            float distance;
            Vector3 currentScale = transform.localScale;

            // Get local positions of hands in original frame
            var posOut = GetLocalPositions();
            Vector3 lhLocal = posOut.lhLocal;
            Vector3 rhLocal = posOut.rhLocal;

            // Scale along specified direction
            if (_direction == "x")
            {
                distance = Mathf.Abs(lhLocal.x - rhLocal.x);
                float scaleFactor = distance / _distanceDatum;
                transform.localScale = new Vector3(currentScale.x * scaleFactor,
                                                   currentScale.y,
                                                   currentScale.z);
                //_totalScaleFactor = new Vector3(_totalScaleFactor.x * scaleFactor,
                //                                _totalScaleFactor.y,
                //                                _totalScaleFactor.z);
            }
            else if (_direction == "z")
            {
                distance = Mathf.Abs(lhLocal.z - rhLocal.z);
                float scaleFactor = distance / _distanceDatum;
                transform.localScale = new Vector3(currentScale.x,
                                                   currentScale.y,
                                                   currentScale.z * scaleFactor);
                //_totalScaleFactor = new Vector3(_totalScaleFactor.x,
                //                                _totalScaleFactor.y,
                //                                _totalScaleFactor.z * scaleFactor);
            }
            else
            {
                distance = Mathf.Abs(lhLocal.y - rhLocal.y);
                float scaleFactor = distance / _distanceDatum;
                transform.localScale = new Vector3(currentScale.x,
                                                   currentScale.y * scaleFactor,
                                                   currentScale.z);
                //_totalScaleFactor = new Vector3(_totalScaleFactor.x,
                //                                _totalScaleFactor.y * scaleFactor,
                //                                _totalScaleFactor.z);
            }
            _distanceDatum = distance;
        }
    }

    // Set directional datum method to reduce code redundancy
    private void SetDistanceDatum(string direction)
    {
        // Get local positions of hands in original frame
        var posOut = GetLocalPositions();
        Vector3 lhLocal = posOut.lhLocal;
        Vector3 rhLocal = posOut.rhLocal;

        // Set direction datum
        if (direction == "x")
        {
            _distanceDatum = Mathf.Abs(lhLocal.x - rhLocal.x);
        }
        else if (direction == "z")
        {
            _distanceDatum = Mathf.Abs(lhLocal.z - rhLocal.z);
        }
        else
        {
            _distanceDatum = Mathf.Abs(lhLocal.y - rhLocal.y);
        }
    }

    // Method to obtain locally-transformed hand positions based on original scale
    private (Vector3 lhLocal, Vector3 rhLocal) GetLocalPositions()
    {
        // Determine positions in local frame based on original scale
        Vector3 lhLocalPrime = transform.InverseTransformPoint(leftHandTransform.position);
        Vector3 lhLocal = new Vector3(lhLocalPrime.x / _totalScaleFactor.x,
                                      lhLocalPrime.y / _totalScaleFactor.y,
                                      lhLocalPrime.z / _totalScaleFactor.z);
        Vector3 rhLocalPrime = transform.InverseTransformPoint(rightHandTransform.position);
        Vector3 rhLocal = new Vector3(rhLocalPrime.x / _totalScaleFactor.x,
                                      rhLocalPrime.y / _totalScaleFactor.y,
                                      rhLocalPrime.z / _totalScaleFactor.z);
        return (lhLocal, rhLocal);
    }
}
