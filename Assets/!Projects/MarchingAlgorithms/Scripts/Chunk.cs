﻿using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MarchingTerrainGeneration
{
    public class Chunk : MonoBehaviour
    {
        enum MarchType { MarchCubes, MarchTetrahedra };

        [SerializeField] MarchType marchType;
        [SerializeField] ComputeShader marchCubesGenerationShader;
        [SerializeField] ComputeShader marchTetrahedraGenerationShader;
        [SerializeField] NoiseGenerator noiseGenerator;
        [SerializeField] MeshFilter meshFilter;
        [SerializeField] MeshCollider meshCollider;
        [SerializeField, Range(0, 8), OnValueChanged(nameof(CreateTerrain))] int lodLVL;
        [SerializeField] private int terrainScale;

        Mesh generatedMesh;

        ComputeShader usedShader;
        int usedKernelID;

        int marchingCubesGenerationKernelID;
        int marchingTetrahedrasGenerationKernelID;
        int updateWeightsKernelID;


        float[] weights;

        struct Triangle
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;

            public static int SizeOf => sizeof(float) * 3 * 3;
        }

        // buffers for transfering data

        //The triangles buffer holds all of our triangle objects that will be generated by marching cubes
        ComputeBuffer trianglesBuffer;

        //Since we can’t know how many triangles will be generated, we have to keep track of them in triangles count buffer
        ComputeBuffer trianglesCountBuffer;

        //Weights buffer contains the noise values generated in the first part of this tutorial
        ComputeBuffer weightsBuffer;

        private void Awake()
        {
            noiseGenerator.onValuesChanged += CreateTerrainOnNoiseChanged;
        }

        [Button]
        private void Setup()
        {
            marchingCubesGenerationKernelID = marchCubesGenerationShader.FindKernel("MarchingCubesGeneration");
            marchingTetrahedrasGenerationKernelID = marchTetrahedraGenerationShader.FindKernel("MarchingTetrahedrasGeneration");

            usedShader = marchType == MarchType.MarchCubes ? marchCubesGenerationShader : marchTetrahedraGenerationShader;
            usedKernelID = marchType == MarchType.MarchCubes ? marchingCubesGenerationKernelID : marchingTetrahedrasGenerationKernelID;

            updateWeightsKernelID = usedShader.FindKernel("UpdateWeights");
        }

        private void Start()
        {
            weights = noiseGenerator.GetNoise(GridMetrics.LastLodLvl);
        }

        [Button]
        public void Test()
        {
            int[] scales = new int[] { 64, 128, 256 , 512, 1024 , 2048, 4096 };

            marchType = MarchType.MarchCubes;
            Setup();
            for (int i = 0; i < scales.Length; i++)
            {
                GridMetrics.Scale = scales[i];
                CreateTerrain();
            }

            marchType = MarchType.MarchTetrahedra;
            Setup();

            for (int i = 0; i < scales.Length; i++)
            {
                GridMetrics.Scale = scales[i];
                CreateTerrain();
            }
        }

        void CreateTerrainOnNoiseChanged() => CreateTerrain(true);

        [Button]
        private void CreateTerrain(bool regenerateWeights = false)
        {
            CreateBuffers();
            generatedMesh = new Mesh();
            UpdateMesh();
            ReleaseBuffers();
        }

        void UpdateMesh()
        {
            Mesh mesh = CreateMesh();
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;

            TimeSpan elapsed = generateStopWatch.Elapsed;
            Debug.LogError($"{marchType}, " +
                $"scale : {GridMetrics.Scale} , " +
                $"mesh vert count, {generatedMesh.vertexCount}" +
                $"generation time : {elapsed.TotalMilliseconds} ms");
        }

        public void EditWeights(Vector3 hitPosition, float brushSize, bool add)
        {
            CreateBuffers();

            weightsBuffer.SetData(weights);
            usedShader.SetBuffer(updateWeightsKernelID, "_Weights", weightsBuffer);

            usedShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(GridMetrics.LastLodLvl));
            usedShader.SetVector("_HitPosition", hitPosition);
            usedShader.SetFloat("_BrushSize", brushSize);
            usedShader.SetFloat("_TerraformStrength", add ? 1f : -1f);
            usedShader.SetInt("_Scale", GridMetrics.Scale);

            usedShader.Dispatch(updateWeightsKernelID, GridMetrics.ThreadGroups(GridMetrics.LastLodLvl), GridMetrics.ThreadGroups(GridMetrics.LastLodLvl), GridMetrics.ThreadGroups(GridMetrics.LastLodLvl));

            weightsBuffer.GetData(weights);

            UpdateMesh();
            ReleaseBuffers();
        }

        System.Diagnostics.Stopwatch generateStopWatch = new System.Diagnostics.Stopwatch();

        Mesh CreateMesh()
        {
            generateStopWatch.Reset();
            generateStopWatch.Start();

            usedShader.SetBuffer(usedKernelID, "_Triangles", trianglesBuffer);
            usedShader.SetBuffer(usedKernelID, "_Weights", weightsBuffer);

            usedShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(GridMetrics.LastLodLvl));
            usedShader.SetFloat("_IsoLevel", .5f);
            usedShader.SetInt("_Scale", GridMetrics.Scale);
            usedShader.SetInt("_LODSize", GridMetrics.PointsPerChunk(lodLVL));

            float lodScaleFactor = ((float)GridMetrics.PointsPerChunk(GridMetrics.LastLodLvl) + 1) / (float)GridMetrics.PointsPerChunk(lodLVL);

            usedShader.SetFloat("_LodScaleFactor", lodScaleFactor);

            weightsBuffer.SetData(weights);
            trianglesBuffer.SetCounterValue(0);

            usedShader.Dispatch(usedKernelID, GridMetrics.ThreadGroups(lodLVL), GridMetrics.ThreadGroups(lodLVL), GridMetrics.ThreadGroups(lodLVL));

            Triangle[] triangles = new Triangle[GetTriangleCount()];
            trianglesBuffer.GetData(triangles);

            generateStopWatch.Stop();

            return CreateMeshFromTriangles(triangles);
        }

        private void CreateBuffers()
        {
            var bufferCount = GridMetrics.PointsPerChunk(lodLVL) * GridMetrics.PointsPerChunk(lodLVL) * GridMetrics.PointsPerChunk(lodLVL);

            //we have to tell the buffers the maximum amount of elements it can contain,
            //we need to initialize the trianglesBuffer with 5 * the total grid size.
            //5 is derived from the fact that there can at most be 5 triangles per cube configuration.

            //We already defined the size of a single triangle which is the stride of the buffer.
            //We also have to make our buffer of type append. Just as a regular buffer is similar to an array,
            //the append buffer is similar to a list. Instead of having to use an index to add to the list, we can simply call list.Append(myElement)

            //The triangles count buffer will simply contain a single integer.

            //The weightsBuffer, just like the weightsBuffer in the NoiseGenerator, contains a single float value per point in the 3D grid.

            trianglesBuffer = new ComputeBuffer(5 * bufferCount, Triangle.SizeOf, ComputeBufferType.Append);
            trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            weightsBuffer = new ComputeBuffer(GridMetrics.PointsPerChunk(GridMetrics.LastLodLvl) * GridMetrics.PointsPerChunk(GridMetrics.LastLodLvl) * GridMetrics.PointsPerChunk(GridMetrics.LastLodLvl), sizeof(float));
        }

        private void ReleaseBuffers()
        {
            trianglesBuffer.Release();
            trianglesCountBuffer.Release();
            weightsBuffer.Release();
        }

        //In case you are wondering why we can’t just use _trianglesBuffer.count (which is an existing function),
        //the .count gives us the max capacity of the buffer, not the actual length of the appendBuffer.
        int GetTriangleCount()
        {
            int[] triCount = { 0 };
            ComputeBuffer.CopyCount(trianglesBuffer, trianglesCountBuffer, 0);
            trianglesCountBuffer.GetData(triCount);
            return triCount[0];
        }

        Mesh CreateMeshFromTriangles(Triangle[] triangles)
        {
            // Initialize the vertices and triangles list(this is for our mesh data).
            // Every triangle contains 3 vertices so we will need triangles.Length * 3 as size of our vertices and triangles array.

            Vector3[] vertices = new Vector3[triangles.Length * 3];
            int[] tris = new int[triangles.Length * 3];

            //Loop through all triangles generated by marching cubes, and add them to our verts and tris.

            for (int i = 0; i < triangles.Length; i++)
            {
                int startIndex = i * 3;
                vertices[startIndex] = triangles[i].a;
                vertices[startIndex + 1] = triangles[i].b;
                vertices[startIndex + 2] = triangles[i].c;
                tris[startIndex] = startIndex;
                tris[startIndex + 1] = startIndex + 1;
                tris[startIndex + 2] = startIndex + 2;
            }

            generatedMesh.Clear();
            generatedMesh.vertices = vertices;
            generatedMesh.triangles = tris;
            generatedMesh.RecalculateNormals();
            return generatedMesh;
        }
    }
}