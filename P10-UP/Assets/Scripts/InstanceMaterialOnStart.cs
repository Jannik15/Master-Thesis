using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceMaterialOnStart : MonoBehaviour
{
    private Material[] materials;
    private List<Material> childMaterials;
    private Renderer[] childRenderers;
    void Start()
    {
        Renderer render = GetComponent<Renderer>();
        if (render != null)
        {
            materials = render.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = Instantiate(materials[i]);
            }
        }
        childRenderers = GetComponentsInChildren<Renderer>();
        childMaterials = new List<Material>();
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childMaterials.AddRange(childRenderers[i].materials);
        }
        for (int i = 0; i < childMaterials.Count; i++)
        {
            childMaterials[i] = Instantiate(childMaterials[i]);
        }
    }
}
