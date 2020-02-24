using UnityEngine;

public class Portal : MonoBehaviour
{
    private GameObject connectRoom;
    private GameObject portal;
    private GameObject forwardPortal, backwardPortal;
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
            GameObject child = portalTransform.GetChild(i).gameObject;
            if (child.CompareTag("ForwardPortal"))
            {
                forwardPortal = child;
                portalForwardMasks = child.GetComponentsInChildren<Renderer>();
            }
            else if (child.CompareTag("BackwardPortal"))
            {
                backwardPortal = child;
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

    public void SetActive(bool portal)
    {
        this.portal.SetActive(portal);
        if (backwardPortal.activeSelf)
        {
            forwardPortal.SetActive(true);
            backwardPortal.SetActive(false);
        }
    }

    public void SwitchActiveSubPortal()
    {
        if (forwardPortal.activeSelf)
        {
            backwardPortal.SetActive(true);
            forwardPortal.SetActive(false);
        }
        else
        {
            forwardPortal.SetActive(true);
            backwardPortal.SetActive(false);
        }
    }

    public void SetForwardStencilValue(int stencilValue)
    {
        this.forwardStencilValue = stencilValue;
        for (int i = 0; i < portalForwardMasks.Length; i++)
        {
            for (int j = 0; j < portalForwardMasks[i].materials.Length; j++)
            {
                portalForwardMasks[i].materials[j].SetInt("_StencilValue", stencilValue);
            }
        }
    }

    public void SetBackwardStencilValue(int stencilValue)
    {
        this.backwardStencilValue = stencilValue;
        for (int i = 0; i < portalBackwardMasks.Length; i++)
        {
            for (int j = 0; j < portalBackwardMasks[i].materials.Length; j++)
            {
                portalBackwardMasks[i].materials[j].SetInt("_StencilValue", stencilValue);
            }
        }
    }
}
