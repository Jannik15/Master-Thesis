using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPreviousPortalPosition : MonoBehaviour
{
    GameObject[] potentialPortals, potentialOppositePortals;
    GameObject portal, oppositePortal;
    Material[] otherWorldMaterial;

    private void Update()
    {
        potentialPortals = GameObject.FindGameObjectsWithTag("EntryPortal"); // Find all Next portals
        portal = FindStencilPos("MidStencil", potentialPortals); // For forward facing stencils

        potentialOppositePortals = GameObject.FindGameObjectsWithTag("ExitPortal"); // Find all Previous portals
        oppositePortal = FindStencilPos("OppositeMidStencil", potentialOppositePortals); // For bacward facing stencils

        if (portal != null && oppositePortal != null) // if both stencils are enabled
        {
            Transform portalRenderer = portal.GetComponent<Transform>();
            Transform oppositePortalRenderer = oppositePortal.GetComponent<Transform>();

            otherWorldMaterial = GetComponent<Renderer>().sharedMaterials;
            foreach (Material m in otherWorldMaterial)
            {
                m.SetMatrix("_WorldToPortal", portalRenderer.worldToLocalMatrix);
                m.SetMatrix("_WorldToPortal", oppositePortalRenderer.worldToLocalMatrix);
            }
        }
        else if (portal != null) // if MidStencil is enabled
        {
            Transform portalRenderer = portal.GetComponent<Transform>();

            otherWorldMaterial = GetComponent<Renderer>().sharedMaterials;
            foreach (Material m in otherWorldMaterial)
            {
                m.SetMatrix("_WorldToPortal", portalRenderer.worldToLocalMatrix);
            }
        }
        else if (oppositePortal != null) // if OppositeMidStencil is enabled
        {
            Transform oppositePortalRenderer = oppositePortal.GetComponent<Transform>();

            otherWorldMaterial = GetComponent<Renderer>().sharedMaterials;
            foreach (Material m in otherWorldMaterial)
            {
                m.SetMatrix("_WorldToPortal", oppositePortalRenderer.worldToLocalMatrix);
            }
        }
    }

    private GameObject FindStencilPos(string stencilTag, GameObject[] portals)
    {
        foreach (GameObject p in portals) // Loop through each portal to check which is active
        {
            for (int i = 0; i < p.transform.childCount; i++)
            {
                Transform child = p.transform.GetChild(i);
                if (child.gameObject.activeSelf && child.name == stencilTag) // If child of portal is active AND has the right tag
                {
                    return child.gameObject;
                }
            }
        }
        return null;
    }
}