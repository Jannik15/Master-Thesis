using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstanceMaterialOnStart : MonoBehaviour
{
    private Renderer[] childRenderers;
    private Text[] childTexts;
    private TextMeshProUGUI[] childTMPTexts;
    private Image[] childImages;
    [SerializeField] private Shader shaderToApply;
    [SerializeField] private int stencilValueToApply;
    [SerializeField] private int stencilMaskValueToApply;
    [SerializeField] private int renderQueueToApply = -1;
    [SerializeField] private Transform shaderMatrixTransform;
    void Awake()
    {
        childRenderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < childRenderers.Length; i++)
        {
            for (int j = 0; j < childRenderers[i].materials.Length; j++)
            {
                childRenderers[i].materials[j] = Instantiate(childRenderers[i].materials[j]);
                //if (shaderToApply != null)
                //{
                //    childRenderers[i].materials[j].shader = shaderToApply;
                //}
                //childRenderers[i].materials[j].SetInt("_StencilValue", stencilValueToApply);
                //childRenderers[i].materials[j].SetInt("_StencilReadMask", stencilMaskValueToApply);
                //childRenderers[i].materials[j].renderQueue = renderQueueToApply != -1 ? renderQueueToApply : childRenderers[i].materials[j].renderQueue;
                //if (shaderMatrixTransform != null)  // Geometry
                //{
                //    childRenderers[i].materials[j].SetMatrix("_WorldToPortal", shaderMatrixTransform.worldToLocalMatrix);
                //}
            }
        }

        childTexts = GetComponentsInChildren<Text>();
        for (int i = 0; i < childTexts.Length; i++)
        {
            childTexts[i].material = Instantiate(childTexts[i].material);
        }

        childTMPTexts = GetComponentsInChildren<TextMeshProUGUI>();
        for (int i = 0; i < childTMPTexts.Length; i++)
        {
            for (int j = 0; j < childTMPTexts[i].fontMaterials.Length; j++)
            {
                childTMPTexts[i].fontMaterials[j] = Instantiate(childTMPTexts[i].fontMaterials[j]);
            }
        }

        childImages = GetComponentsInChildren<Image>();
        for (int i = 0; i < childImages.Length; i++)
        {
            childImages[i].material = Instantiate(childImages[i].material);
        }
    }
}
