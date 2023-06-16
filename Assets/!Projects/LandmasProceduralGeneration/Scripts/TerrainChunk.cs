using UnityEngine;

namespace LandmassProceduralGeneration
{
    public class TerrainChunk
    {
        public GameObject meshObject;
        Bounds bounds;
        public Vector2 coord;

        //public bool ShouldBeVisible => Mathf.Sqrt(bounds.SqrDistance(EndlessTerrain.viewerPosition)) <= EndlessTerrain.maxViewDst;
        public bool Enabled => meshObject.activeSelf;
        public bool Released { get; set; }
        public TerrainChunk(Transform parent)
        {
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.parent = parent;
        }

        public void InitializeOnGet(Vector2 coord, int size,Transform parent)
        {
            this.coord = coord;
            var coords = coord * size;
            bounds = new Bounds(coords, Vector2.one * size);
            meshObject.transform.parent = parent;
            meshObject.transform.position = new Vector3(coords.x, 0, coords.y);
            meshObject.transform.localScale = Vector3.one * size / 10f;
        }

        public void InitializeOnRelease(Transform parent)
        {
            meshObject.transform.parent = parent;
        }

        public void SetVisible(bool state)
        {
            meshObject.SetActive(state);
        }

        public void LogVisibility() => Debug.LogError("visible", meshObject);
    }
}