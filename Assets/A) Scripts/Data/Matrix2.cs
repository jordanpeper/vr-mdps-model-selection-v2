using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public class Matrix2
{
    // Array typically accessed to retrieve current data
    private List<float[]> _matrix;
    public List<float[]> matrix
    {
        get { return _matrix; }
    }

    // Array typically accessed to retrieve original data
    private List<float[]> _staticMatrix;
    public List<float[]> staticMatrix
    {
        get { return _staticMatrix; }
    }

    // Constructor
    public Matrix2(TextAsset csvFile)
    {
        // Import CSV data
        List<float[]> matrix = ImportCSV(csvFile);
        _staticMatrix = new List<float[]>();
        _matrix = new List<float[]>();

        // Make a deep copy of the matrix
        for (int i = 0; i < matrix.Count; i++)
        {
            _staticMatrix.Add(new float[matrix[i].Length]);
            _matrix.Add(new float[matrix[i].Length]);

            for (int j = 0; j < matrix[i].Length; j++)
            {
                _matrix[i][j] = matrix[i][j];
                _staticMatrix[i][j] = matrix[i][j];
            }
        }

        // Log the matrix sizes
        Debug.LogFormat("New 2D Matrix: {0}x{1}", _matrix.Count, _matrix[0].Length);
    }

    // Restore the dynamic matrix to initial values
    public void Restore()
    {
        // Make a deep copy of the matrix
        for (int k = 0; k < _staticMatrix.Count; k++)
        {
            for (int i = 0; i < _staticMatrix[k].Length; i++)
            {
                _matrix[k][i] = _staticMatrix[k][i];
            }
        }
    }

    // Parse csv file iteratively
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
