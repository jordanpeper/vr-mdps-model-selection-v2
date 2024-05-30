using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


public class MatrixPos2
{
    // PRIVATELY ACCESSIBLE VARIABLES WITH PUBLIC READ-ONLY EXNTENSIONS

    // Vector3 list containing local positions
    private List<Vector3> _matrix;
    public List<Vector3> matrix
    {
        get { return _matrix; }
    }

    // Vector3 list containing global positions
    private List<Vector3> _globalMatrix;
    public List<Vector3> globalMatrix
    {
        get { return _globalMatrix; }
    }

    // Form of the matrix which may be altered from original state
    private List<Vector3> _staticMatrix;
    public List<Vector3> staticMatrix
    {
        get { return _staticMatrix; }
    }

    private Transform _trackedTransform;
    private float _positionScale;


    // CONSTRUCTOR METHODS

    // MatrixManager constructor
    public MatrixPos2(TextAsset csvFile, Transform trackedTransform, float positionScale)
    {
        // Import CSV data
        List<float[]> matrix = ImportCSV(csvFile);
        _staticMatrix = new List<Vector3>();
        _matrix = new List<Vector3>();
        _globalMatrix = new List<Vector3>();

        // Make a deep copy of the matrix
        for (int k = 0; k < matrix.Count; k++)
        {
            // Append back list (swap y and z)
            Vector3 newVector3 = new Vector3(matrix[k][0], matrix[k][2], matrix[k][1]) * positionScale;
            _staticMatrix.Add(newVector3);
            _matrix.Add(newVector3);
            _globalMatrix.Add(newVector3);
        }

        // Set trackked transform and determine initial global positions
        _trackedTransform = trackedTransform;
        SetGlobalTransform();

        // Log the matrix sizes
        Debug.LogFormat("New 2D Position Matrix: {0}x{1}", _matrix.Count, 3);
    }

    // Public method to reset the dynamic matrix to original (static)
    public void Restore()
    {
        // Make a deep copy of the matrix
        for (int k = 0; k < _staticMatrix.Count; k++)
        {
            _matrix[k] = _staticMatrix[k];
        }
    }

    // Transform positions about tracked transform
    public void SetGlobalTransform()
    {
        // Loop through each row and apply transformation
        for (int i = 0; i < _matrix.Count; i++)
        {
            // Transform local matrix positions ot global with respect to tracked transform
            _globalMatrix[i] = _trackedTransform.TransformPoint(_matrix[i]);
        }
    }


    // PRIVATELY ACCESSIBLE METHODS

    // Private method to convert a CSV file to the desired format
    private List<float[]> ImportCSV(TextAsset newDataset)
    {
        // Split the TextAsset along newline
        string[] StringArrayData = newDataset.text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        int numRows = StringArrayData.GetLength(0);
        int numCols = StringArrayData[0].Split(new string[] { "," }, StringSplitOptions.None).GetLength(0);

        // Allocate empty "Matrix"; loop through split string array
        List<float[]> formattedArray = new List<float[]>(numRows);
        for (int i = 0; i < numRows; i++)
        {
            formattedArray.Add(new float[numCols]);

            // Break apart a single row into another string array respective to the columns; allocate
            string[] rowData = StringArrayData[i].Split(new string[] { "," }, StringSplitOptions.None);
            for (int j = 0; j < numCols; j++)
            {
                formattedArray[i][j] = (float)double.Parse(rowData[j]);
            }
        }

        return formattedArray;
    }
}
