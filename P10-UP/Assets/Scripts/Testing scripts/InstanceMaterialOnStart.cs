using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceMaterialOnStart : MonoBehaviour
{
    private List<Material> childMaterials;
    private Renderer[] childRenderers;
    [SerializeField] private Shader shaderToApply;
    [SerializeField] private int stencilValueToApply;
    [SerializeField] private Transform shaderMatrixTransform;
    void Awake()
    {
        childRenderers = GetComponentsInChildren<Renderer>();
        childMaterials = new List<Material>();

        for (int i = 0; i < childRenderers.Length; i++)
        {
            for (int j = 0; j < childRenderers[i].materials.Length; j++)
            {
                childRenderers[i].materials[j] = Instantiate(childRenderers[i].materials[j]);
                if (shaderToApply != null)
                {
                    childRenderers[i].materials[j].shader = shaderToApply;
                }
                childRenderers[i].materials[j].SetInt("_StencilValue", stencilValueToApply);
                if (shaderMatrixTransform != null)  // Geometry
                {
                    childRenderers[i].materials[j].SetMatrix("_WorldToPortal", shaderMatrixTransform.worldToLocalMatrix);
                }
                //childRenderers[i].materials[j].renderQueue += (stencilValueToApply - 1) * 300;
            }
        }
    }
}
