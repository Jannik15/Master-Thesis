using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Room inRoom;
    [SerializeField] private Room connectedRoom;
    [SerializeField] private Portal connectedPortal;
    private GameObject forwardPortal, backwardPortal;
    private int stencilValue, readMaskValue;
    private Renderer[] portalForwardMasks, portalBackwardMasks;
    [SerializeField] private int portalId;

    public void AssignValues(Room inRoom, Room connectedRoom, Portal connectedPortal, int portalId)
    {
        this.inRoom = inRoom;
        this.connectedRoom = connectedRoom;
        this.connectedPortal = connectedPortal;
        this.portalId = portalId;

        Transform portalTransform = gameObject.transform;
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

    public int GetStencilValue()
    {
        return stencilValue;
    }

    public Room GetRoom()
    {
        return inRoom;
    }

    public Room GetConnectedRoom()
    {
        return connectedRoom;
    }
    public Portal GetConnectedPortal()
    {
        return connectedPortal;
    }

    public Renderer[] GetForwardMasks()
    {
        return portalForwardMasks;
    }

    public Renderer[] GetBackwardMasks()
    {
        return portalBackwardMasks;
    }

    public void SetMaskShader(Shader shader)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.shader = shader;
        }
    }
    public void SetMaskShader(Shader shader, int stencilValue, int readMaskValue, int renderQueue)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.shader = shader;
            renderers[i].material.SetInt("_StencilValue", stencilValue);
            renderers[i].material.renderQueue = renderQueue;
            if (readMaskValue > 0)
            {
                renderers[i].material.SetInt("_StencilReadMask", readMaskValue);
            }
        }
    }

    public void SetActive(bool portal)
    {
        this.gameObject.SetActive(portal);
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
}
