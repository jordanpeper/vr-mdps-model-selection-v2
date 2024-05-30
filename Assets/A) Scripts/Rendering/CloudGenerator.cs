using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class CloudGenerator : MonoBehaviour
{
    // Public variables
    public GameObject glyph; // Data point prefab
    public float scaleNormalizer = 1f;
    public Material baseGlyphMaterial;
    public int[] featFillOrder; // Maps feature array positions to placement on glyph

    // Private variables
    private GameBehavior _gameBehavior; // GameBehaviour component for shared data access
    private Color[] _categoricalColors; // Color array for data class materials
    private int _numSubdivisions;


    // Start is called before the first frame update
    void Start()
    {
        // Get gameBehavior and useful variables
        _gameBehavior = GetComponent<GameBehavior>();
        _numSubdivisions = _gameBehavior.numSubdivisions;

        // Generate a "color map" for each batch
        _categoricalColors = GenerateColors(_numSubdivisions);

        // Instantiate the mesh; destroy real collection
        InstantiateDataCloud();

        // Set group counts in game behavior
        _gameBehavior.SetGroupCounts();
    }


    // Instantiate the data cloud
    private void InstantiateDataCloud()
    {
        // Create a temporary container for the points
        GameObject prefabParent = new GameObject();
        prefabParent.transform.position = transform.position;
        prefabParent.SetActive(false); // Disable since this contains individual models

        // Retrieve the position and modeling data
        List<Vector3> positionData = _gameBehavior.positionData.matrix;
        List<float[]> modelingData = _gameBehavior.modelingData.matrix;
        int numRows = positionData.Count;
        int numFeats = modelingData[0].Length;

        // Throw exception if the fetaure fill order count does not match feature count
        if (featFillOrder.Length != numFeats)
        {
            Debug.LogError("Feature fill order does not correspond to number of features.");
        }

        // Mesh combine instance data (list index is a group)
        List<List<CombineInstance>> meshCombine = new List<List<CombineInstance>>(_numSubdivisions);

        // Estimate number of samples per group
        int numSamples = (int)Mathf.Round(numRows / (_numSubdivisions-1));
        int lastGroup = 0;

        // Loop through every sample; color according to group membership
        int combinePosition = 0;
        for (int k = 0; k < numRows; k++)
        {
            // Determine group based on sample index; append combine list if new group
            int group = (k - (k % numSamples)) / numSamples;

            if (group != lastGroup || k == 0)
            {
                // Check if this is the last subdivision
                if (group == _numSubdivisions-1)
                {
                    meshCombine.Add(new List<CombineInstance>(numRows - (numSamples * (_numSubdivisions - 1))));
                }
                else
                {
                    meshCombine.Add(new List<CombineInstance>(numSamples));
                }

                // Reset combine position; set new group as last group
                combinePosition = 0;
                lastGroup = group;
            }

            // Instantiate glyph and shape keys
            GameObject dataPoint = Instantiate(glyph) as GameObject;
            SkinnedMeshRenderer smr = dataPoint.GetComponent<SkinnedMeshRenderer>();
            for (int f = 0; f < numFeats; f++)
            {
                smr.SetBlendShapeWeight(featFillOrder[f], modelingData[k][f]);
            }

            // Set data point properties
            dataPoint.transform.SetParent(prefabParent.transform);
            dataPoint.transform.localScale = dataPoint.transform.localScale / scaleNormalizer;
            // Temp. patch - MATLAB projection is differernt than Unity, swap y and z
            dataPoint.transform.localPosition = positionData[k];

            // Bake smr mesh as static mesh
            Mesh staticMesh = new Mesh();
            smr.BakeMesh(staticMesh);

            // Allocate mesh properties to correct combine mesh group
            CombineInstance newCombine = new CombineInstance();
            newCombine.mesh = staticMesh;
            newCombine.transform = dataPoint.transform.localToWorldMatrix;
            meshCombine[group].Add(newCombine);

            // Increment combine position
            combinePosition++;
        }

        // Loop through groups and create merged mesh object
        MeshFilter[] meshFilters = new MeshFilter[_numSubdivisions];
        for (int g = 0; g < _numSubdivisions; g++)
        {
            // Instance a batch mesh container and properties
            GameObject batchContainer = new GameObject("batchContainer" + g);
            batchContainer.transform.SetParent(transform);

            // Create combined mesh
            Mesh newMesh = new Mesh();
            newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            newMesh.CombineMeshes(meshCombine[g].ToArray(), true, true, true);

            // Create combined material
            Material newMaterial = new Material(baseGlyphMaterial);
            newMaterial.color = _categoricalColors[g];

            // Apply mesh and material to batch
            MeshFilter meshFilter = batchContainer.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = newMesh;
            MeshRenderer meshRenderer = batchContainer.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = newMaterial;
            meshRenderer.sharedMaterial.color = _categoricalColors[g];
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            // Allocate mesh filter
            meshFilters[g] = meshFilter;
        }

        // Delete prefab parent
        Destroy(prefabParent);

        // Allocate group (batch) mesh instances and mesh filters to gameBehavior
        _gameBehavior.groupMeshCombines = meshCombine;
        _gameBehavior.groupMeshFilters = meshFilters;

    }


    // Generate n distinct colors for use in 
    private Color[] GenerateColors(int numGroups)
    {
        // Create new Color array
        Color[] colors = new Color[numGroups];

        // Set variables
        float hueIncrement = 0.667f / numGroups; // Red to blue equally spaced
        float saturation = 1f;
        float value = 1f;
        float hue = 0.667f;

        // Loop through and increment the hue
        for (int i = 0; i < numGroups; i++)
        {
            hue -= hueIncrement; // Reverse so smallest color indicators start at blue
            colors[i] = Color.HSVToRGB(hue, saturation, value, true);
        }

        return colors;
    }


    // Generate n distinct colors for use in 
    private Color[] GenerateSpecificColors(float[] groupMeasures)
    {
        // Create new Color array
        int numGroups = groupMeasures.Length;
        Color[] colors = new Color[numGroups];

        // Range scale between 0 and 0.667
        float minimum = groupMeasures.Min();
        float maximum = groupMeasures.Max();
        float[] newGroupMeasures = new float[numGroups];
        for (int i = 0; i < numGroups; i++)
        {
            float measure = groupMeasures[i];
            float adjustedMeasure = 0.667f - ((0.667f) * (measure - minimum) / (maximum - minimum));
            colors[i] = Color.HSVToRGB(adjustedMeasure, 1, 1, true);
        }

        return colors;
    }
}
