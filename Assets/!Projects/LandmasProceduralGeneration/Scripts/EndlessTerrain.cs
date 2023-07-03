using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LandmassProceduralGeneration;
using System;

public class EndlessTerrain : MonoBehaviour
{
    const float scale = 2f;
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
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();


    void Start() 
    {
        chunkSize = MapGenerator.MapChunkSize - 1;
        maxViewDst = detailInfo[detailInfo.Length - 1].visibleDstThreshold;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        MapGenerator = FindObjectOfType<MapGenerator>();
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        if ((prevViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForUpdate)
        {
            prevViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
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
        MeshCollider collider;

        LevelOfDetailInfo[] detailLevels;
        LODMesh[] lodMeshes;

        LODMesh collisionLodMesh;

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
            collider = meshObject.AddComponent<MeshCollider>();
            renderer.material = material;
            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];

            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider)
                {
                    collisionLodMesh = lodMeshes[i];
                }
            }

            MapGenerator.RequestMapdata(position, OnMapdataReceived);
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDst;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != prevLodIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            prevLodIndex = lodIndex;
                            filter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    if (lodIndex == 0)
                    {
                        if (collisionLodMesh.hasMesh)
                        {
                            collider.sharedMesh = collisionLodMesh.mesh;
                        }
                        else if (!collisionLodMesh.hasRequestedMesh)
                        {
                            collisionLodMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
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
            Texture2D tex = TextureGenerator.TextureFromColourMap(data.colorMap, MapGenerator.MapChunkSize, MapGenerator.MapChunkSize);
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
        public bool useForCollider;
    }
}