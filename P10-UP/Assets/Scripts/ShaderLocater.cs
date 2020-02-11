using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is tasked with locating the positions of all materials
/// using stencil shaders, and feed their positions to the relevant shader
/// </summary>
public class ShaderLocater
{
    private GameObject[] allPortals;
    
    // TODO: Return transforms?
    public void LoadPortals(List<Portal> forwardPortals, List<Portal> backwardPortals, string[] portalTags)
    {
        // Store all portals in a list, based on their room index. 2 Lists are required, one for forward portals, and one for backward portals
        allPortals = (GameObject[]) Resources.FindObjectsOfTypeAll(typeof(GameObject));
        int iterator = 1;
        // TODO: The following can be optimized with Jobs and Burst
        for (int i = 0; i < allPortals.Length; i++)
        {
            if (allPortals[i].tag.Contains("Portal"))
            {
                if (allPortals[i].CompareTag(portalTags[0]))
                {
                    forwardPortals.Add( new Portal(allPortals[i], iterator));
                    iterator++;
                }
                else if (allPortals[i].CompareTag(portalTags[1]))
                {
                    backwardPortals.Add(new Portal(allPortals[i], iterator));
                    iterator++;
                }
            }
        }
        Debug.Log("ForwardPortals Count: " + forwardPortals.Count + " | BackwardPortals Count: " + backwardPortals.Count);
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
            //for (int j = 0; j < roomMaterials[i].materials.Length; j++)
            //{
            //    Debug.Log("Setting matrix for material of object: " + roomMaterials[i] + " with portal " + portal.name);
            //    roomMaterials[i].materials[j].SetMatrix("_WorldToPortal", portal.worldToLocalMatrix);
            //}
        }
        //if (forwardPortals[roomIndex] != null && oppositePortal != null) // if both stencils are enabled
        //{
        //    Transform portalRenderer = portal.GetComponent<Transform>();
        //    Transform oppositePortalRenderer = oppositePortal.GetComponent<Transform>();

        //    otherWorldMaterial = GetComponent<Renderer>().sharedMaterials;
        //    foreach (Material m in otherWorldMaterial)
        //    {
        //        m.SetMatrix("_WorldToPortal", portalRenderer.worldToLocalMatrix);
        //        m.SetMatrix("_WorldToPortal", oppositePortalRenderer.worldToLocalMatrix);
        //    }
        //}
        //else if (portal != null) // if MidStencil is enabled
        //{
        //    Transform portalRenderer = portal.GetComponent<Transform>();

        //    otherWorldMaterial = GetComponent<Renderer>().sharedMaterials;
        //    foreach (Material m in otherWorldMaterial)
        //    {
        //        m.SetMatrix("_WorldToPortal", portalRenderer.worldToLocalMatrix);
        //    }
        //}
        //else if (oppositePortal != null) // if OppositeMidStencil is enabled
        //{
        //    Transform oppositePortalRenderer = oppositePortal.GetComponent<Transform>();

        //    otherWorldMaterial = GetComponent<Renderer>().sharedMaterials;
        //    foreach (Material m in otherWorldMaterial)
        //    {
        //        m.SetMatrix("_WorldToPortal", oppositePortalRenderer.worldToLocalMatrix);
        //    }
        //}
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
