using NaughtyAttributes;
using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MarchingTerrainGeneration
{
    public class NoiseVisual : MonoBehaviour
    {
        [SerializeField] NoiseGenerator noiseGenerator;
        float[] _weights;

        [Button]
        private void Start()
        {
            //_weights = noiseGenerator.GetNoise();

            //_weights = new float[GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk];
            //for (int i = 0; i < _weights.Length; i++)
            //{
            //    _weights[i] = Random.value;
            //}
        }

        private void OnDrawGizmos()
        {
        //    if (_weights == null || _weights.Length == 0)
        //    {
        //        return;
        //    }
        //    for (int x = 0; x < GridMetrics.PointsPerChunk; x++)
        //    {
        //        for (int y = 0; y < GridMetrics.PointsPerChunk; y++)
        //        {
        //            for (int z = 0; z < GridMetrics.PointsPerChunk; z++)
        //            {
        //                int index = x + GridMetrics.PointsPerChunk * (y + GridMetrics.PointsPerChunk * z);
        //                float noiseValue = _weights[index];
        //                Gizmos.color = Color.Lerp(Color.black, Color.white, noiseValue);
        //                Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one * .2f);
        //            }
        //        }
        //    }
        }
    }
}