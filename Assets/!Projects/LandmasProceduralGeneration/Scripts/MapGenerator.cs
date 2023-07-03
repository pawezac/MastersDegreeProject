using System;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UIElements;
using TMPro;
using System.Threading;
using System.Collections.Generic;

namespace LandmassProceduralGeneration
{
    [Serializable]
    public struct TerrainType
    {
        [SerializeField] string terrainTypeName;
        public float Height;
        public Color Colour;
    }

    public enum DrawMode { NoiseMap, ColourMap, Mesh, FallOffMap }

    public class MapGenerator : MonoBehaviour
    {
        const string generalPropertiesTab = "General Properties";
        const string _2DPreviewPropertiesTab = "General Properties";
        const string _3DPreviewPropertiesTab = "General Properties";

        [Space(5)]
        [BoxGroup(generalPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh))] DrawMode drawMode;
        [BoxGroup(generalPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh))] Noise.NormalizeMode normalizeMode;
        [BoxGroup(generalPropertiesTab)][SerializeField, ReadOnly] MapDisplay mapDisplay;
        [Space(5)]
        [BoxGroup(generalPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh))] float noiseScale;
        [Space(5)]
        [BoxGroup(generalPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh))] int octaves;
        [BoxGroup(generalPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh)), Range(0, 1)] float persistance;
        [BoxGroup(generalPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh))] float lacunarity;
        [Space(5)]
        [BoxGroup(generalPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh))] int seed;
        [BoxGroup(generalPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh))] Vector2 offset;
        [BoxGroup(generalPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh))] bool applyFalloffMap;
        [BoxGroup(generalPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh))] bool flatShading;

        [Space(10)]
        [BoxGroup(_2DPreviewPropertiesTab), SerializeField, ReadOnly, NaughtyAttributes.ShowAssetPreview(10000, 10000), ShowIf(nameof(IsDrawModeForTexture2D))] Texture2D noiseTexture = null;
        [BoxGroup(_2DPreviewPropertiesTab), SerializeField] TerrainType[] regions;

        public int MapChunkSize => flatShading ? 95 : 239;

        [BoxGroup(_3DPreviewPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh)), ShowIf(nameof(IsDrawModeForMesh)), Range(0, 6)] int levelOfDetailEditorPreview;
        [BoxGroup(_3DPreviewPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh)), ShowIf(nameof(IsDrawModeForMesh))] float meshHeightMultiplier;
        [BoxGroup(_3DPreviewPropertiesTab), SerializeField, OnValueChanged(nameof(Refresh)), ShowIf(nameof(IsDrawModeForMesh))] AnimationCurve meshHeightCurve;

        [SerializeField, HideInInspector] float[,] falloffMap;

        private bool isDrawModeForColour => drawMode == DrawMode.ColourMap;
        public bool IsDrawModeForTexture2D => drawMode == DrawMode.ColourMap || drawMode == DrawMode.NoiseMap;
        public bool IsDrawModeForMesh => drawMode == DrawMode.Mesh;

        private void OnValidate()
        {
            if (mapDisplay == null)
            {
                TryGetComponent(out mapDisplay);
            }
            falloffMap = FallofGenerator.GenerateFallofMap(MapChunkSize);
        }

        private void Awake()
        {
            falloffMap = FallofGenerator.GenerateFallofMap(MapChunkSize);
        }

        public void GenerateMap()
        {
            MapData data = GenerateMapData(Vector2.zero);
            DrawMap(data);
        }

        #region Threading

        Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

        public void RequestMapdata(Vector2 center, Action<MapData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MapDataThread(center, callback);
            };
            new Thread(threadStart).Start();
        }

        void MapDataThread(Vector2 center, Action<MapData> callback)
        {
            var mapData = GenerateMapData(center);
            lock (mapDataThreadInfoQueue)
            {
                mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        public void RequestMeshData(MapData mapData, Action<MeshData> callback, int levelOfDetail)
        {
            ThreadStart threadStart = delegate
            {
                MeshDataThread(mapData, callback, levelOfDetail);
            };
            new Thread(threadStart).Start();
        }

        void MeshDataThread(MapData mapData, Action<MeshData> callback, int levelOfDetail)
        {
            var meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail, flatShading);
            lock (meshDataThreadInfoQueue)
            {
                meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        private void Update()
        {
            if (mapDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
                {
                    MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.data);
                }
            }

            if (meshDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
                {
                    MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.data);
                }
            }
        }

        #endregion

        private MapData GenerateMapData(Vector2 center)
        {
            var noiseMap = Noise.GenerateNoiseMap(MapChunkSize + 2, MapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            var colorMap = GenerateColourMap(noiseMap);
            return new MapData(noiseMap, colorMap);
        }

        private void DrawMap(MapData data)
        {
            if (drawMode == DrawMode.Mesh)
            {
                mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(data.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetailEditorPreview, flatShading), TextureGenerator.TextureFromColourMap(GenerateColourMap(data.heightMap), MapChunkSize, MapChunkSize));
            }
            else
            {
                noiseTexture = drawMode switch
                {
                    DrawMode.NoiseMap => TextureGenerator.TextureFromHeightMap(data.heightMap),
                    DrawMode.ColourMap => TextureGenerator.TextureFromColourMap(data.colorMap, MapChunkSize, MapChunkSize),
                    DrawMode.FallOffMap => TextureGenerator.TextureFromHeightMap(FallofGenerator.GenerateFallofMap(MapChunkSize)),
                    _ => null
                };
                mapDisplay.DrawTexture(noiseTexture);
            }
        }

        #region EditorHelpers

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
            Color32[] colourMap = new Color32[MapChunkSize * MapChunkSize];

            for (int y = 0; y < MapChunkSize; y++)
            {
                for (int x = 0; x < MapChunkSize; x++)
                {
                    if (applyFalloffMap)
                    {
                        heightMap[x, y] = Mathf.Clamp01(heightMap[x, y] - falloffMap[x, y]);
                    }

                    float height = heightMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (height >= regions[i].Height)
                        {
                            colourMap[y * MapChunkSize + x] = regions[i].Colour;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return colourMap;
        }
        #endregion
    }

    public struct MapData
    {
        public readonly float[,] heightMap;
        public readonly Color32[] colorMap;

        public MapData(float[,] heightMap, Color32[] colorMap)
        {
            this.heightMap = heightMap;
            this.colorMap = colorMap;
        }
    }

    public struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T data;

        public MapThreadInfo(Action<T> callback, T data)
        {
            this.callback = callback;
            this.data = data;
        }
    }
}