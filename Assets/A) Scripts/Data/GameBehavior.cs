using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class GameBehavior : MonoBehaviour
{
    // Public variables
    [Header("Data Configuration")]
    public TextAsset positionDataCsv; // .csv data array of positions
    public TextAsset modelingDataCsv; // .csv data array of features
    public TextAsset perfDataCsv; // .csv data array of performance evaluators
    public int numSubdivisions = 100; // Batch rendering
    public int positionScale = 10; // Position scale factor

    // Construct certain classes upon script initialization
    void Awake()
    {
        // Initialize 3D matrix from csv file
        _positionData = new MatrixPos2(positionDataCsv, transform, positionScale);
        _modelingData = new Matrix2(modelingDataCsv);
        _perfData = new Matrix2(perfDataCsv);

        // Throw exception if the number of samples dod not match
        if (_positionData.matrix.Count != _modelingData.matrix.Count || _perfData.matrix.Count != _modelingData.matrix.Count)
        {
            Debug.LogError("Number of sample positions does not match the number of sample positions.");
        }

        // Determine total number of samples
        _numRows = _positionData.matrix.Count;
    }

    // Total number of samples
    private int _numRows;

    // 2D matrix of point positions
    private MatrixPos2 _positionData;
    public MatrixPos2 positionData
    {
        get { return _positionData; }
        set { _positionData = value; }
    }

    // 2D matrix of glyph features
    private Matrix2 _modelingData;
    public Matrix2 modelingData
    {
        get { return _modelingData; }
        set { _modelingData = value; }
    }

    // 2D matrix of glyph performance data
    private Matrix2 _perfData;
    public Matrix2 perfData
    {
        get { return _perfData; }
        set { _perfData = value; }
    }

    // List of combine instance arrays containing mesh per group (list index)
    private List<List<CombineInstance>> _groupMeshCombines;
    public List<List<CombineInstance>> groupMeshCombines
    {
        get { return _groupMeshCombines; }
        set { _groupMeshCombines = value; }
    }

    // List of group mesh filters corresponding to each mesh group in meshCombine
    private MeshFilter[] _groupMeshFilters;
    public MeshFilter[] groupMeshFilters
    {
        get { return _groupMeshFilters; }
        set { _groupMeshFilters = value; }
    }

    // List of group sizes
    private int[] _groupCounts;
    public int[] groupCounts
    {
        get { return _groupCounts; }
        set { _groupCounts = value; }
    }

    // Method to delete samples and reconfigure mesh
    public void DeleteSamples(List<int> indices)
    {
        indices.Sort((a, b) => b.CompareTo(a));
        List<List<int>> newIndices = new List<List<int>>();
        List<int> groupNums = new List<int>();

        // Loop through indices; ID the group number
        for (int i = 0; i < indices.Count; i++)
        {
            int index = indices[i];

            // Calculate group number and local index
            (int group, int localIndex) = LinearToCoord(index);

            // Check whether a list for this group has been created already
            if (groupNums.Contains(group))
            {
                // Append to correct list
                int groupNumPosition = groupNums.IndexOf(group);
                newIndices[groupNumPosition].Add(localIndex);
            }
            else
            {
                // Create new list; append group number and local index
                groupNums.Add(group);
                newIndices.Add(new List<int>());
                newIndices[newIndices.Count - 1].Add(localIndex);
            }

            // Remove from position and modeling matrices
            _positionData.matrix.RemoveAt(index);
            _positionData.globalMatrix.RemoveAt(index);
            _modelingData.matrix.RemoveAt(index);
            _perfData.matrix.RemoveAt(index);
        }

        // Loop through each set of group local indices
        for (int g = 0; g < groupNums.Count; g++)
        {
            int group = groupNums[g];
            newIndices[g].Sort((a, b) => b.CompareTo(a));

            // Remove sample from mesh combine
            foreach (int kprime in newIndices[g])
            {
                _groupMeshCombines[group].RemoveAt(kprime);
            }

            // Combine mesh and reapply to corresponding mesh filter
            Mesh newMesh = new Mesh();
            newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            newMesh.CombineMeshes(_groupMeshCombines[group].ToArray(), true, true, true);
            _groupMeshFilters[group].sharedMesh = newMesh;
        }
    }

    // Public method to set the group counts of batch mesh objects (run after allocation)
    public void SetGroupCounts()
    {
        // Instantiate group counts list
        _groupCounts = new int[numSubdivisions];
        int numGroups = numSubdivisions;
        for (int g = 0; g < numGroups; g++)
        {
            _groupCounts[g] = _groupMeshCombines[g].Count;
        }
    }

    // Convert linear index into row & column, i.e., group and local index
    private (int row, int column) LinearToCoord(int linearIndex)
    {
        int currentRow = 0;
        int currentCol = 0;

        foreach (List<CombineInstance> row in _groupMeshCombines)
        {
            int rowLength = row.Count;
            if (linearIndex < rowLength)
            {
                currentRow = _groupMeshCombines.IndexOf(row);
                currentCol = linearIndex;
                break;
            }
            else
            {
                linearIndex -= rowLength;
            }
        }

        return (currentRow, currentCol);
    }
}
