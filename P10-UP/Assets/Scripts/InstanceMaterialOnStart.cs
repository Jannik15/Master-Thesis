using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceMaterialOnStart : MonoBehaviour
{
    private List<Material> childMaterials;
    private Renderer[] childRenderers;
    [SerializeField] private Shader shaderToApply;
    [SerializeField] private int stencilValueToApply;
    void Start()
    {
        childRenderers = GetComponentsInChildren<Renderer>();
        childMaterials = new List<Material>();
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childMaterials.AddRange(childRenderers[i].materials);
        }
        for (int i = 0; i < childMaterials.Count; i++)
        {
            childMaterials[i] = Instantiate(childMaterials[i]);
            childMaterials[i].shader = shaderToApply;
            childMaterials[i].SetInt("_StencilValue", stencilValueToApply);
        }
    }
}
