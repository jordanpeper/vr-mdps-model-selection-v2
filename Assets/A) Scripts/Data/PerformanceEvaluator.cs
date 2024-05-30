using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PerformanceEvaluator : MonoBehaviour
{
    // Publicly accesible variables
    public InputActionProperty triggerCalcButton;

    // Privately accesible variables
    private GameBehavior _gameBehavior;

    // Start is called before the first frame update
    void Start()
    {
        _gameBehavior = GetComponent<GameBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        if (triggerCalcButton.action.WasPressedThisFrame())
        {
            // Preallocate
            List<float[]> matrix = _gameBehavior.perfData.matrix;
            float RMSEC = 0;
            float RMSEP = 0;
            float R2C = 0;
            float R2P = 0;

            // Loop through and take sum
            for (int i = 0; i < matrix.Count; i++)
            {
                RMSEC += matrix[i][0];
                RMSEP += matrix[i][1];
                R2C += matrix[i][2];
                R2P += matrix[i][3];
            }

            // Calculate averages
            RMSEC /= matrix.Count;
            RMSEP /= matrix.Count;
            R2C /= matrix.Count;
            R2P /= matrix.Count;

            // Write to console
            Debug.LogFormat("RMSEC: {0}, RMSEP: {1}, R2C: {2}, R2P: {3}, Pairs: {4}", RMSEC, RMSEP, R2C, R2P, matrix.Count);
        }
    }
}
