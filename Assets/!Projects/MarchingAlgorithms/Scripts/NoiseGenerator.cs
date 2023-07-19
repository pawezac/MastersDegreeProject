using NaughtyAttributes;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace MarchingTerrainGeneration
{
    public class NoiseGenerator : MonoBehaviour
    {
        [SerializeField] ComputeShader noiseCompute;

        [Space(10)]
        [SerializeField,OnValueChanged(nameof(OnNoiseValuesChanged))] float amplitude = 5f;
        [SerializeField, Range(.01f, .04f), OnValueChanged(nameof(OnNoiseValuesChanged))] float frequency = 0.005f;
        [SerializeField, OnValueChanged(nameof(OnNoiseValuesChanged))] int octaves = 8;
        [SerializeField, OnValueChanged(nameof(OnNoiseValuesChanged))] float groundPercent = 0.2f;

        ComputeBuffer weightsBuffer;
        float noiseScale = 1f;

        void OnNoiseValuesChanged()
        {
            onValuesChanged?.Invoke();
        }

        public Action onValuesChanged;

        public float[] GetNoise(int lodLvl)
        {
            CreateBuffers(lodLvl);

            float[] noiseValues =
                new float[GridMetrics.PointsPerChunk(lodLvl) * GridMetrics.PointsPerChunk(lodLvl) * GridMetrics.PointsPerChunk(lodLvl)];

            noiseCompute.SetBuffer(0, "_Weights", weightsBuffer);

            noiseCompute.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(lodLvl));
            noiseCompute.SetFloat("_NoiseScale", noiseScale);
            noiseCompute.SetFloat("_Amplitude", amplitude);
            noiseCompute.SetFloat("_Frequency", frequency);
            noiseCompute.SetInt("_Octaves", octaves);
            noiseCompute.SetFloat("_GroundPercent", groundPercent);
            noiseCompute.SetInt("_Scale", GridMetrics.Scale);
            noiseCompute.SetInt("_GroundLevel", GridMetrics.GroundLevel);

            noiseCompute.Dispatch(
                     0, GridMetrics.ThreadGroups(lodLvl), GridMetrics.ThreadGroups(lodLvl), GridMetrics.ThreadGroups(lodLvl)
                 );

            weightsBuffer.GetData(noiseValues);

            ReleaseBuffers();
            return noiseValues;
        }

        void CreateBuffers(int lodLvl)
        {
            weightsBuffer = new ComputeBuffer(
                GridMetrics.PointsPerChunk(lodLvl) * GridMetrics.PointsPerChunk(lodLvl) * GridMetrics.PointsPerChunk(lodLvl), sizeof(float)
            );
        }

        void ReleaseBuffers()
        {
            weightsBuffer.Release();
        }

    }
}