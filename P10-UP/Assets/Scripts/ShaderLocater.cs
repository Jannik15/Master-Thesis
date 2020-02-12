using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is tasked with locating the positions of all materials
/// using stencil shaders, and feed their positions to the relevant shader
/// </summary>
public class ShaderLocater
{
    public List<Portal> InitializePortals(string portalTag)
    {
        List<Portal> portals = new List<Portal>();
        GameObject[] allPortals = (GameObject[]) Resources.FindObjectsOfTypeAll(typeof(GameObject)); // TODO: Pass Portal objects after generation
        for (int i = 0; i < allPortals.Length; i++)
        {
            if (allPortals[i].CompareTag(portalTag))
            {
                portals.Add(new Portal(allPortals[i]));
            }
        }
        return portals;
    }

    public Transform GetPortalTransform(List<GameObject> portalsToCheck, int stencilValue)
    {
        Debug.Log(portalsToCheck.Count);
        for (int i = 0; i < portalsToCheck.Count; i++)
        {
            Debug.Log("Checking at index " + i + " object " + portalsToCheck[i].name);
            Transform child = portalsToCheck[i].transform.GetChild(0);
            if (child.CompareTag("Mask"))
            {
                if (stencilValue == child.GetComponent<Renderer>().material.GetInt("_StencilValue"))
                {
                    return portalsToCheck[i].transform;

                }
            }
            else
            {
                Transform childOfChild = child.GetChild(0);
                if (childOfChild.CompareTag("Mask"))
                {
                    if (stencilValue == childOfChild.GetComponent<Renderer>().material.GetInt("_StencilValue"))
                    {
                        return portalsToCheck[i].transform;
                    }
                }
                else
                {
                    Debug.Log("Insert debug message here!");
                    return null;
                }
            }
        }
        Debug.Log("didnt work");
        return null;
    }

    public void UpdateShaderMatrix(List<Renderer> roomMaterials, Transform portal)
    {
        // If everything is grouped in rooms, find forward and backward portals and use their position for the relevant shaders in a matrix

        for (int i = 0; i < roomMaterials.Count; i++)
        {
            roomMaterials[i].material.SetMatrix("_WorldToPortal", portal.worldToLocalMatrix);
        }
    }

    public void FindShader(string shaderName)
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
