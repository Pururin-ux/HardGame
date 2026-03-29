using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Crystals
{
    public class PentagramCrystal : MonoBehaviour
    {
        [SerializeField] private GameObject LightObject;

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("Player entered the PentagramCrystal trigger zone. Press E to activate light.");
                ToggleLight();
            }
        }

        private void ToggleLight()
        {
            if (LightObject != null)
            {
                LightObject.SetActive(!LightObject.activeSelf);
                Debug.Log($"Light toggled: {LightObject.activeSelf}");
            }
            else
            {
                Debug.LogWarning("LightObject is not assigned in PentagramCrystal!");
            }
        }
    }
}
