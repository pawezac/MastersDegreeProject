using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LandmassProceduralGeneration
{
    public static class Noise
    {
        static float minPossibleScale = 0.0001f;

        static (int min, int max) randomRange = (-100000, 100000); //most effective range for generating random

        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int seed, int octaves, float persistance, float lacunarity, Vector2 offset)
        {
            System.Random prng = new System.Random(seed);

            Vector2[] octaveOffsets = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
            {
                octaveOffsets[i] = new Vector2(prng.Next(randomRange.min, randomRange.max), prng.Next(randomRange.min, randomRange.max)) + offset;
            }

            scale = Mathf.Clamp(scale, minPossibleScale, float.MaxValue);
            float[,] noiseMap = new float[mapWidth, mapHeight];

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfHeight = mapHeight / 2;
            float halfWidth = mapWidth / 2;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                        float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                        noiseHeight += (Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1) * amplitude;


                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]); //
                }
            }

            return noiseMap;
        }
    }
}