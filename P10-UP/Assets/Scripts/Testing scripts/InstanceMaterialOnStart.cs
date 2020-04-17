using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceMaterialOnStart : MonoBehaviour
{
    private List<Material> childMaterials;
    private Renderer[] childRenderers;
    [SerializeField] private Shader shaderToApply;
    [SerializeField] private int stencilValueToApply;
    [SerializeField] private int stencilMaskValueToApply;
    [SerializeField] private int renderQueueToApply = -1;
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
                childRenderers[i].materials[j].SetInt("_StencilReadMask", stencilMaskValueToApply);
                childRenderers[i].materials[j].renderQueue = renderQueueToApply != -1 ? renderQueueToApply : childRenderers[i].materials[j].renderQueue;
                if (shaderMatrixTransform != null)  // Geometry
                {
                    childRenderers[i].materials[j].SetMatrix("_WorldToPortal", shaderMatrixTransform.worldToLocalMatrix);
                }
            }
        }
    }
}
