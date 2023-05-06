using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes;

namespace LandmassProceduralGeneration
{
    [Serializable]
    public struct TerrainType
    {
        [SerializeField] string terrainTypeName;
        public float Height;
        public Color Colour;
    }

    public enum DrawMode { NoiseMap, ColourMap, Mesh }

    public class MapGenerator : MonoBehaviour
    {
        const string generalPropertiesTab = "General Properties";
        const string _2DPreviewPropertiesTab = "General Properties";
        const string _3DPreviewPropertiesTab = "General Properties";

        [Space(5)]
        [BoxGroup(generalPropertiesTab),SerializeField, OnValueChanged(nameof(Refresh))] DrawMode drawMode;
        [BoxGroup(generalPropertiesTab)][SerializeField, ReadOnly] MapDisplay mapDisplay;
        [Space(5)]
        [BoxGroup(generalPropertiesTab),SerializeField, OnValueChanged(nameof(Refresh))] float noiseScale;
        [Space(5)]
        [BoxGroup(generalPropertiesTab),SerializeField, OnValueChanged(nameof(Refresh))] int octaves;
        [BoxGroup(generalPropertiesTab),SerializeField, OnValueChanged(nameof(Refresh)), Range(0, 1)] float persistance;
        [BoxGroup(generalPropertiesTab),SerializeField, OnValueChanged(nameof(Refresh))] float lacunarity;
        [Space(5)]
        [BoxGroup(generalPropertiesTab),SerializeField, OnValueChanged(nameof(Refresh))] int seed;
        [BoxGroup(generalPropertiesTab),SerializeField, OnValueChanged(nameof(Refresh))] Vector2 offset;

        [SerializeField, ReadOnly] float[,] noiseMap;

        [Space(10)]
        [BoxGroup(_2DPreviewPropertiesTab),SerializeField, ReadOnly, NaughtyAttributes.ShowAssetPreview(10000, 10000), ShowIf(nameof(IsDrawModeForTexture2D))] Texture2D noiseTexture = null;
        [BoxGroup(_2DPreviewPropertiesTab),SerializeField, ShowIf(nameof(isDrawModeForColour))] TerrainType[] regions;

        [Space(10)]
        const int mapChunkSize = 241;
        [BoxGroup(_3DPreviewPropertiesTab),SerializeField, OnValueChanged(nameof(Refresh)), ShowIf(nameof(IsDrawModeForMesh)), Range(0,6)] int levelOfDetail;
        [BoxGroup(_3DPreviewPropertiesTab),SerializeField, OnValueChanged(nameof(Refresh)), ShowIf(nameof(IsDrawModeForMesh))] float meshHeightMultiplier;
        [BoxGroup(_3DPreviewPropertiesTab),SerializeField, OnValueChanged(nameof(Refresh)), ShowIf(nameof(IsDrawModeForMesh))] AnimationCurve meshHeightCurve;
        
        private bool isDrawModeForColour => drawMode == DrawMode.ColourMap;
        public bool IsDrawModeForTexture2D => drawMode == DrawMode.ColourMap || drawMode == DrawMode.NoiseMap;
        public bool IsDrawModeForMesh => drawMode == DrawMode.Mesh;

        private void OnValidate()
        {
            if (mapDisplay == null)
            {
                TryGetComponent(out mapDisplay);
            }
        }

        public void GenerateMap()
        {
            noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, seed, octaves, persistance, lacunarity, offset);


            if (drawMode == DrawMode.Mesh)
            {
                mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(GenerateColourMap(noiseMap), noiseMap.GetLength(0), noiseMap.GetLength(1)));
            }
            else
            {
                noiseTexture = drawMode switch
                {
                    DrawMode.NoiseMap => TextureGenerator.TextureFromHeightMap(noiseMap),
                    DrawMode.ColourMap => TextureGenerator.TextureFromColourMap(GenerateColourMap(noiseMap), noiseMap.GetLength(0), noiseMap.GetLength(1)),
                    _ => null
                };
                mapDisplay.DrawTexture(noiseTexture);
            }
        }

        [Button(text: nameof(Refresh))]
        void Refresh()
        {
            ClampParameters();
            Clear();
            mapDisplay.Refresh();
            GenerateMap();
        }

        [Button(text: nameof(Clear))]
        void Clear()
        {
            mapDisplay.Clear();
            noiseTexture = null;
        }

        private void ClampParameters()
        {
            lacunarity = lacunarity < 1 ? 1 : lacunarity;
            octaves = octaves < 0 ? 0 : octaves;
        }

        private Color32[] GenerateColourMap(float[,] heightMap)
        {
            //faster solution for generating texture
            int mapWidth = heightMap.GetLength(0);
            int mapHeight = heightMap.GetLength(1);

            Color32[] colourMap = new Color32[mapWidth * mapHeight];

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float height = heightMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (height <= regions[i].Height)
                        {
                            colourMap[y * mapWidth + x] = regions[i].Colour;
                            break;
                        }
                    }
                }
            }
            return colourMap;
        }
    }
}