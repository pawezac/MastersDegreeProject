using UnityEngine;

namespace LandmassProceduralGeneration
{
    public static class MeshGenerator
    {
        public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMult, AnimationCurve meshHeightCurve, int levelOfDetail)
        {
            AnimationCurve heightCurve = new AnimationCurve(meshHeightCurve.keys);
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            float topLeftX = (width - 1) / -2f; //for centering mesh
            float topLeftZ = (height - 1) / 2f;


            int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
            int verteciesPerLine = (width - 1) / meshSimplificationIncrement + 1;
            int vertexIdx = 0;

            MeshData meshData = new MeshData(verteciesPerLine, verteciesPerLine);

            for (int y = 0; y < height; y += meshSimplificationIncrement)
            {
                for (int x = 0; x < width; x += meshSimplificationIncrement)
                {
                    meshData.vertices[vertexIdx] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMult, topLeftZ - y);
                    meshData.uvs[vertexIdx] = new Vector2(x / (float)width, y / (float)height);
                    if (x < width - 1 && y < height - 1) //ignore right and bottom edge of indexes for iteration
                    {
                        meshData.AddTriangle(vertexIdx, vertexIdx + verteciesPerLine + 1, vertexIdx + verteciesPerLine);
                        meshData.AddTriangle(vertexIdx + verteciesPerLine + 1, vertexIdx, vertexIdx + 1);
                    }
                    vertexIdx++;
                }
            }
            return meshData;
        }
    }

    public class MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;

        int triangleIndex;

        public MeshData(int meshWidth, int meshHeight)
        {
            vertices = new Vector3[meshWidth * meshHeight];
            uvs = new Vector2[meshWidth * meshHeight];
            triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        }

        public void AddTriangle(int a, int b, int c)
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}