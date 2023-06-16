using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LandmassProceduralGeneration;
using System;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForUpdate = 25f;
    const float sqrViewerMoveThresholdForUpdate = viewerMoveThresholdForUpdate * viewerMoveThresholdForUpdate;
    public LevelOfDetailInfo[] detailInfo;
    public Transform viewer;
    public MapDisplay mapDisplay;

    public static float maxViewDst;
    public static Vector2 viewerPosition;
    public static Vector2 prevViewerPosition;
    int chunkSize;
    int chunksVisibleInViewDst;

    static MapGenerator MapGenerator;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();


    void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1;
        maxViewDst = detailInfo[detailInfo.Length - 1].visibleDstThreshold;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        MapGenerator = FindObjectOfType<MapGenerator>();
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((prevViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForUpdate)
        {
            UpdateVisibleChunks();
        }
        prevViewerPosition = viewerPosition;
    }

    void UpdateVisibleChunks()
    {

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, detailInfo, mapDisplay.MeshPreviewMat));
                }

            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        MeshRenderer renderer;
        MeshFilter filter;
        LevelOfDetailInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData;
        bool mapDataReceived;

        int prevLodIndex = -1;

        public TerrainChunk(Vector2 coord, int size, Transform parent, LevelOfDetailInfo[] detailLevels, Material material)
        {
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("TerrainChunk");
            renderer = meshObject.AddComponent<MeshRenderer>();
            filter = meshObject.AddComponent<MeshFilter>();
            renderer.material = material;
            meshObject.transform.position = positionV3;

            meshObject.transform.parent = parent;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];

            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            MapGenerator.RequestMapdata(position, OnMapdataReceived);
        }

        public void UpdateTerrainChunk()
        {
            if (!mapDataReceived) return;

            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (visible)
            {
                int lodIdx = 0;
                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                    {
                        lodIdx++;
                    }
                    else
                        break;
                }
                if (lodIdx != prevLodIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIdx];
                    if (lodMesh.hasMesh)
                    {
                        prevLodIndex = lodIdx;
                        filter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(mapData);
                    }
                }
            }

            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }

        void OnMapdataReceived(MapData data)
        {
            mapData = data;
            mapDataReceived = true;
            Texture2D tex = TextureGenerator.TextureFromColourMap(data.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            renderer.material.mainTexture = tex;
            UpdateTerrainChunk();
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;

        System.Action updateCallback;

        public LODMesh(int lod, Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataRecived(MeshData data)
        {
            mesh = data.CreateMesh();
            hasMesh = true;
            updateCallback.Invoke();
        }

        public void RequestMesh(MapData data)
        {
            hasRequestedMesh = true;
            MapGenerator.RequestMeshData(data, OnMeshDataRecived, lod);
        }
    }

    [Serializable]
    public struct LevelOfDetailInfo
    {
        public int lod;
        public float visibleDstThreshold;
    }
}