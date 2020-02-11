using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal
{
    private GameObject portal;
    private int stencilValue;
    private Renderer[] portalMasks;

    public Portal(GameObject portal, int stencilValue)
    {
        this.portal = portal;
        this.stencilValue = stencilValue;

        portalMasks = portal.GetComponentsInChildren<Renderer>();
        SetPortalStencilValue(stencilValue);
    }

    public GameObject GetPortal()
    {
        return portal;
    }

    public Transform GetPortalTransform()
    {
        return portal.transform;
    }

    public int GetPortalStencilValue()
    {
        return stencilValue;
    }

    public void SetPortalStencilValue(int newStencilValue)
    {
        for (int i = 0; i < portalMasks.Length; i++)
        {
            for (int j = 0; j < portalMasks[i].materials.Length; j++)
            {
                portalMasks[i].materials[j].SetInt("_StencilValue", newStencilValue);
            }
        }
    }
}
