using UnityEngine;

public class Portal
{
    private GameObject portal;
    private int forwardStencilValue, backwardStencilValue;
    private Renderer[] portalForwardMasks, portalBackwardMasks;

    /// <summary>
    /// Initialize Portal object by assigning stencil values to its forward and backward masks.
    /// During this initialization, the masks are exposed and can be retrieved with GetForward(or Backward)Masks(),
    /// and the stencil values can be assigned using SetPortalStencils().
    /// </summary>
    /// <param name="portal"></param>
    public Portal(GameObject portal)
    {
        this.portal = portal;

        Transform portalTransform = portal.transform;
        for (int i = 0; i < portalTransform.childCount; i++)
        {
            Transform child = portalTransform.GetChild(i);
            if (child.CompareTag("ForwardPortal"))
            {
                portalForwardMasks = child.GetComponentsInChildren<Renderer>();
            }
            else if (child.CompareTag("BackwardPortal"))
            {
                portalBackwardMasks = child.GetComponentsInChildren<Renderer>();
            }
        }
    }

    public GameObject GetPortal()
    {
        return portal;
    }

    public Transform GetTransform()
    {
        return portal.transform;
    }

    public int GetForwardStencilValue()
    {
        return forwardStencilValue;
    }
    public int GetBackwardStencilValue()
    {
        return backwardStencilValue;
    }

    public Renderer[] GetForwardMasks()
    {
        return portalForwardMasks;
    }

    public Renderer[] GetBackwardMasks()
    {
        return portalBackwardMasks;
    }

    public void SetPortalStencilValues(int forwardStencilValue, int backwardStencilValue)
    {
        this.forwardStencilValue = forwardStencilValue;
        this.backwardStencilValue = backwardStencilValue;
        for (int i = 0; i < portalForwardMasks.Length; i++)
        {
            for (int j = 0; j < portalForwardMasks[i].materials.Length; j++)
            {
                portalForwardMasks[i].materials[j].SetInt("_StencilValue", forwardStencilValue);
            }
        }
        for (int i = 0; i < portalBackwardMasks.Length; i++)
        {
            for (int j = 0; j < portalBackwardMasks[i].materials.Length; j++)
            {
                portalBackwardMasks[i].materials[j].SetInt("_StencilValue", backwardStencilValue);
            }
        }
    }
}
