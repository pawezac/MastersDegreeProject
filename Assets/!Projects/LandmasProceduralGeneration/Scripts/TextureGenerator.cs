using UnityEngine;

namespace LandmassProceduralGeneration
{
    public static class TextureGenerator
    {
        public static Texture2D TextureFromColourMap(Color32[] colourMap, int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels32(colourMap);
            texture.Apply();
            return texture;
        }

        public static Texture2D TextureFromHeightMap(float[,] heightMap)
        {
            var mapWidth = heightMap.GetLength(0);
            var mapHeight = heightMap.GetLength(1);

            //faster solution for generating texture
            Color32[] colourMap = new Color32[mapWidth * mapHeight];

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    colourMap[y * mapWidth + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                }
            }

            return TextureFromColourMap(colourMap, mapWidth, mapHeight);
        }
    }
}