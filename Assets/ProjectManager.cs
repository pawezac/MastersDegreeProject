using MarchingTerrainGeneration;
using NaughtyAttributes;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MarchingTerrainGeneration
{
    public class ProjectManager : MonoBehaviour
    {
        [SerializeField] Chunk chunk;
        [SerializeField] TMPro.TMP_InputField lodlvlInputField;
        [SerializeField] TMPro.TMP_Dropdown marchtypedropdown;
        [SerializeField] GameObject menu;
        [SerializeField] GameObject crosshair;
        [SerializeField] InputActionReference menuToggle;
        [SerializeField] CharacterController player;
        [SerializeField] FirstPersonController controller;

        bool menuShown = false;

        public Action<bool> onMenuShown;

        private void Awake()
        {
            menuToggle.action.performed += MenuAction;
            ShowMenu();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        private void MenuAction(InputAction.CallbackContext obj)
        {
            menuShown = !menuShown;
            menu.SetActive(menuShown);
            crosshair.SetActive(!menuShown);
            Cursor.lockState = !menuShown ? CursorLockMode.Locked : CursorLockMode.Confined;
            Cursor.visible = menuShown;

            if (menuShown)
            {
                DisablePlayer();
            }
            else
            {
                player.enabled = true;
                controller.enabled = true;
            }
        }

        public void ShowMenu()
        {
            menu.SetActive(true);
            menuShown = true;
            crosshair.SetActive(false);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        public void HideMenu()
        {
            menu.SetActive(false);
            menuShown = false;
            crosshair.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        public void Test_Method()
        {
            Chunk.MarchType marchType = (Chunk.MarchType)marchtypedropdown.value;
            if (int.TryParse(lodlvlInputField.text, out int lodlvl))
            {
                chunk.Test(marchType,lodlvl);
            }
            HideMenu();
            EnablePlayer(chunk.GetNewPlayerPos());
        }

        public void Clear() => chunk.Clear();

        public void Quit()
        {
            Application.Quit();
        }

        public void EnablePlayer(Vector3 position) => StartCoroutine(EnablePlayerRoutine(position));

        private IEnumerator EnablePlayerRoutine(Vector3 position)
        {
            player.enabled = false;
            controller.enabled = false;
            yield return null;
            player.transform.position = position;
            yield return null;
            player.enabled = true;
            controller.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            yield return null;
        }

        public void DisablePlayer()
        {
            player.enabled = false;
            controller.enabled = false;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

    }
}
