using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetsGeneration
{
    public class TerrainFace
    {
        private ShapeGenerator shapeGenerator;
        Mesh mesh;
        int resolution;
        Vector3 localUp;
        Vector3 axisA;
        Vector3 axisB;

        public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp)
        {
            this.shapeGenerator = shapeGenerator;
            this.mesh = mesh;
            this.resolution = resolution;
            this.localUp = localUp;

            axisA = new Vector3((float)localUp.y, (float)localUp.z, (float)localUp.x);
            axisB = Vector3.Cross(localUp, axisA);
        }

        public void ConstructMesh()
        {
            Vector3[] vertices = new Vector3[resolution * resolution];
            int[] triangles = new int[(resolution - 1) * (resolution - 1) * Planet.sidesNum];
            Vector2[] uv = mesh.uv.Length == vertices.Length ? mesh.uv : new Vector2[vertices.Length];
            int triIdx = 0;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int idx = x + y * resolution;
                    Vector2 percent = new Vector2(x, y) / (resolution - 1);
                    Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    float unscaledElevation = shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);

                    vertices[idx] = pointOnUnitSphere * shapeGenerator.GetScaledElevation(unscaledElevation);
                    uv[idx].y = unscaledElevation;

                    if (x != resolution - 1 && y != resolution - 1)
                    {
                        triangles[triIdx] = idx;
                        triangles[triIdx + 1] = idx + resolution + 1;
                        triangles[triIdx + 2] = idx + resolution;

                        triangles[triIdx + 3] = idx;
                        triangles[triIdx + 4] = idx + 1;
                        triangles[triIdx + 5] = idx + resolution + 1;

                        triIdx += 6;
                    }
                }
            }
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.uv = uv;
        }

        public void UpdateUVs(ColorGenerator colorGenerator)
        {
            Vector2[] uv = mesh.uv;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int idx = x + y * resolution;
                    Vector2 percent = new Vector2(x, y) / (resolution - 1);
                    Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                    uv[idx].x = colorGenerator.BiomePercentFromPoint(pointOnUnitSphere);
                }
            }
            mesh.uv = uv;
        }
    }
}