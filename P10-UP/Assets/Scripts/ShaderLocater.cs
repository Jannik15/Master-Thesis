using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>
/// This script is tasked with locating the positions of all materials
/// using stencil shaders, and feed their positions to the relevant shader
/// </summary>
public class ShaderLocater : MonoBehaviour
{
    private GameObject[] allPortals;
    [TagSelector] [SerializeField] private string[] portalTags;
    
    public void LoadPortals(ref List<GameObject> forwardPortals, ref List<GameObject> backwardPortals)
    {
        forwardPortals = new List<GameObject>();
        backwardPortals = new List<GameObject>();

        // Store all portals in a list, based on their room index. 2 Lists are required, one for forward portals, and one for backward portals
        allPortals = (GameObject[]) Resources.FindObjectsOfTypeAll(typeof(GameObject));
        // TODO: The following can be optimized with Jobs and Burst
        for (int i = 0; i < allPortals.Length; i++)
        {
            if (allPortals[i].tag.Contains("Portal"))
            {
                if (allPortals[i].CompareTag(portalTags[0]))
                {
                    forwardPortals.Add(allPortals[i]);
                }
                else if (allPortals[i].CompareTag(portalTags[1]))
                {
                    backwardPortals.Add(allPortals[i]);
                }
                else
                {
                    Debug.Log(allPortals[i].name + "'s tag contained \"Portal\", but did not match the tags listed in the portal tag array.");
                }
            }
        }
    }

    public void UpdateShaderMatrix(List<Renderer> roomMaterials, Transform portal, int roomIndex)
    {
        // If everything is grouped in rooms, find forward and backward portals and use their position for the relevant shaders in a matrix

        for (int i = 0; i < roomMaterials.Count; i++)
        {
            for (int j = 0; j < roomMaterials[i].materials.Length; j++)
            {
                roomMaterials[i].materials[j].SetMatrix("_ShaderMatrixName", portal.worldToLocalMatrix);
            }
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
