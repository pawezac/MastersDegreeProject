using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back}

public class Planet : MonoBehaviour
{
    [SerializeField,Range(2,256)] private int resolution = 10;
    [SerializeField] private FaceRenderMask renderMask;

    public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;
    public bool autoUpdate;


    TerrainFace[] terrainFaces;
    [HideInInspector] public bool shapeSettingsFoldout;
    [HideInInspector] public bool colourSettingsFoldout;
    [SerializeField,HideInInspector] MeshFilter[] meshFilters;

    Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColorGenerator colorGenerator = new ColorGenerator();
    public static int sidesNum = 6;

    void Initialize()
    {
        shapeGenerator.UpdateSettings(shapeSettings);
        colorGenerator.UpdateSettings(colourSettings);
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

                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;

            terrainFaces[i] = new TerrainFace(shapeGenerator,meshFilters[i].sharedMesh, resolution, directions[i]);
            bool renderFace = renderMask == FaceRenderMask.All || (int)renderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }

    void GenerateMesh()
    {
        for (int i = 0; i < sidesNum; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
            }
        }
        colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColour()
    {
        colorGenerator.UpdateColours();
        for (int i = 0; i < sidesNum; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].UpdateUVs(colorGenerator);
            }
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
