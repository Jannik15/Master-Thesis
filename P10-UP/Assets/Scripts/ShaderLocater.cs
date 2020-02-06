using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is tasked with locating the positions of all materials
/// using stencil shaders, and feed their positions to the relevant shader
/// </summary>
public class ShaderLocater : MonoBehaviour
{
    private Renderer[] allRenderers;
    void Start()
    {
        allRenderers = (Renderer[]) Resources.FindObjectsOfTypeAll(typeof(Renderer));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 0; i < allRenderers.Length; i++)
            {
                Debug.Log(allRenderers[i].sharedMaterial.shader);
            }
        }
    }

    private void UpdateShaderMatrix()
    {
        // If everything is grouped in rooms, find forward and backward portals and use their position for the relevant shaders in a matrix



    }

    private void FindShader(string shaderName)
    {
        List<Material> allMaterials = new List<Material>();

        Renderer[] allRenderers = (Renderer[])Resources.FindObjectsOfTypeAll(typeof(Renderer));
        foreach (Renderer rend in allRenderers)
        {
            foreach (Material mat in rend.sharedMaterials)
            {
                if (!allMaterials.Contains(mat))
                {
                    allMaterials.Add(mat);
                }
            }
        }

        foreach (Material mat in allMaterials)
        {
            if (mat != null && mat.shader != null && mat.shader.name != null && mat.shader.name == shaderName)
            {

            }
        }
    }
}
