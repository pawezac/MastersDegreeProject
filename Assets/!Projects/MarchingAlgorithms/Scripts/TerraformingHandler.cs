using UnityEngine;
using UnityEngine.InputSystem;

namespace MarchingTerrainGeneration 
{
    public class TerraformingHandler : MonoBehaviour
    {
        [SerializeField] Camera mainCamera;
        [SerializeField] float brushSize = 2f;
        [SerializeField] float raycastlength;
        [SerializeField] InputActionReference terraformAdd;
        [SerializeField] InputActionReference terraformRemove;

        Vector3 hitpoint;
        RaycastHit hit;
        Chunk hittedChunk;

        bool terraformAddStarted;
        bool terraformRemoveStarted;

        private void Awake()
        {
            terraformAdd.action.performed += OnTerraformAddStarted;
            terraformAdd.action.canceled += OnTerraformAddEnded;
            terraformRemove.action.performed += OnTerraformRemoveStarted;
            terraformRemove.action.canceled += OnTerraformRemoveEnded;
        }

        private void OnTerraformAddEnded(InputAction.CallbackContext obj)
        {
            terraformAddStarted = false;
        }

        public void OnTerraformAddStarted(InputAction.CallbackContext obj)
        {
            terraformAddStarted = true;
        }

        public void OnTerraformRemoveStarted(InputAction.CallbackContext obj)
        {
            terraformRemoveStarted = true;
        }

        private void OnTerraformRemoveEnded(InputAction.CallbackContext obj)
        {
            terraformRemoveStarted = false;
        }

        private void Update()
        {
            if (terraformAddStarted)
            {
                Terraform(true);
            }
            if (terraformRemoveStarted) 
            {
                Terraform(false);
            }
        }

        void Terraform(bool add)
        {
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward), out hit, raycastlength))
            {
                if (hit.collider.gameObject.TryGetComponent(out hittedChunk) && hittedChunk)
                {
                    hitpoint = hit.point;
                    hittedChunk.EditWeights(hitpoint, brushSize, add);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitpoint, brushSize / 2);
        }
    }
}