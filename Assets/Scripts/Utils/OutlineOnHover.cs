using UnityEngine;

namespace Utils
{
    public class OutlineOnHover : MonoBehaviour
    {
        public Material outlineMaterial;

        private Renderer objectRenderer;
        private Material[] originalMaterials;

        void Start()
        {
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                // Store the original materials at startup.
                originalMaterials = objectRenderer.materials;
            }
        }

        // Called when the mouse pointer enters the object's collider.
        void OnMouseEnter()
        {
            if (objectRenderer != null)
            {
                // Create a new materials array that includes the original materials plus the outline material.
                int originalLength = originalMaterials.Length;
                Material[] materialsWithOutline = new Material[originalLength + 1];
                for (int i = 0; i < originalLength; i++)
                {
                    materialsWithOutline[i] = originalMaterials[i];
                }
                materialsWithOutline[originalLength] = outlineMaterial;

                objectRenderer.materials = materialsWithOutline;
            }
        }

        // Called when the mouse pointer exits the object's collider.
        void OnMouseExit()
        {
            if (objectRenderer != null)
            {
                // Restore the original materials.
                objectRenderer.materials = originalMaterials;
            }
        }
    }
}