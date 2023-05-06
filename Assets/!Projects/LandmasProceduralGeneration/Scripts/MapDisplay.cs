using UnityEngine;
using NaughtyAttributes;

namespace LandmassProceduralGeneration
{
    public class MapDisplay : MonoBehaviour
    {
        private bool isDrawModeForTexture2D => mapGenerator.IsDrawModeForTexture2D;
        private bool isDrawModeForMesh => mapGenerator.IsDrawModeForMesh;

        [Label("General Properties")]
        [Space(10)]
        [SerializeField, ReadOnly] MapGenerator mapGenerator;

        [ShowIf(nameof(isDrawModeForTexture2D))]
        [SerializeField, HideInInspector] GameObject planePreview;
        [ShowIf(nameof(isDrawModeForTexture2D))]
        [SerializeField, OnValueChanged(nameof(ToggleWorldPreview))] bool worldPreviewEnabled = true;

        [Space(10), Label("2D Preview Properties")]
        [ShowIf(nameof(isDrawModeForMesh))]
        [SerializeField, HideInInspector] GameObject meshPreview;

        [SerializeField, HideInInspector] Material planePreviewMat = null;
        [SerializeField, HideInInspector] Material meshPreviewMat = null;

        public void DrawTexture(Texture2D noiseTexture)
        {
            if (planePreview == null && noiseTexture != null)
            {
                planePreview = GameObject.CreatePrimitive(PrimitiveType.Plane);
                planePreview.hideFlags = HideFlags.HideInHierarchy;
                planePreview.transform.SetParent(transform, false);
                planePreviewMat ??= new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                planePreviewMat.mainTexture = noiseTexture;
                planePreview.GetComponent<MeshRenderer>().sharedMaterial = planePreviewMat;
                planePreview.transform.localScale = new Vector3(noiseTexture.width, 1, noiseTexture.height);
            }
        }

        public void DrawMesh(MeshData meshData, Texture2D noiseTexture)
        {
            if (meshPreview == null && noiseTexture != null)
            {
                meshPreview = new GameObject(nameof(meshPreview), new System.Type[] { typeof(MeshFilter), typeof(MeshRenderer) });
                meshPreview.hideFlags = HideFlags.HideInHierarchy;
                meshPreview.transform.SetParent(transform, false);
                meshPreview.GetComponent<MeshFilter>().sharedMesh = meshData.CreateMesh();
                meshPreviewMat ??= new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                meshPreviewMat.mainTexture = noiseTexture;
                meshPreview.GetComponent<MeshRenderer>().sharedMaterial = meshPreviewMat;
            }
        }

        private void ToggleWorldPreview()
        {
            if (planePreview != null)
            {
                planePreview.SetActive(worldPreviewEnabled && isDrawModeForTexture2D);
            }
            if (meshPreview != null)
            {
                meshPreview.SetActive(worldPreviewEnabled && isDrawModeForMesh);
            }
        }


        public void Refresh()
        {
            worldPreviewEnabled = true;
        }

        public void Clear()
        {
            GameObject.DestroyImmediate(planePreview);
            planePreview = null;
            GameObject.DestroyImmediate(meshPreview);
            meshPreview = null;
            planePreviewMat = null;
            meshPreviewMat = null;

            worldPreviewEnabled = false;
        }

        private void OnValidate()
        {
            if (mapGenerator == null)
            {
                TryGetComponent(out mapGenerator);
            }
        }
    }
}