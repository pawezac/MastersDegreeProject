using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField,Range(2,256)] private int resolution = 10;
    
    public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;
    public bool autoUpdate;


    TerrainFace[] terrainFaces;
    [HideInInspector] public bool shapeSettingsFoldout;
    [HideInInspector] public bool colourSettingsFoldout;
    [SerializeField,HideInInspector] MeshFilter[] meshFilters;

    Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    ShapeGenerator shapeGenerator;
    int sidesNum = 6;

    void Initialize()
    {
        shapeGenerator = new ShapeGenerator(shapeSettings);
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[sidesNum];
        }

        terrainFaces = new TerrainFace[sidesNum];


        for (int i = 0; i < sidesNum; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(shapeGenerator,meshFilters[i].sharedMesh, resolution, directions[i]);
        }
    }

    void GenerateMesh()
    {
        foreach (var terrainFace in terrainFaces)
        {
            terrainFace.ConstructMesh();
        }
    }

    void GenerateColour()
    {
        foreach (var filter in meshFilters)
        {
            filter.TryGetComponent(out MeshRenderer renderer);
            renderer.sharedMaterial.color = colourSettings.planetColour;
        }
    }

    public void OnColourSettingsUpdated()
    {
        if (!autoUpdate) return;
        Initialize();
        GenerateColour();
    }

    public void OnShapeSettingsUpdated()
    {
        if (!autoUpdate) return;
        Initialize();
        GenerateMesh();
    }

    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        GenerateColour();
    }
}
