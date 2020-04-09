using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Room inRoom;
    [SerializeField] private Room connectedRoom;
    [SerializeField] private Portal connectedPortal;
    private GameObject forwardPortal, backwardPortal;
    [SerializeField] private int portalId;
    private Renderer[] renderers;

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
            }
            else if (child.CompareTag("BackwardPortal"))
            {
                backwardPortal = child;
            }
        }

        renderers = gameObject.GetComponentsInChildren<Renderer>();
    }

    public Room GetRoom()
    {
        return inRoom;
    }

    public Renderer GetRenderer()
    {
        return renderers[0];
    }

    public Room GetConnectedRoom()
    {
        return connectedRoom;
    }
    public Portal GetConnectedPortal()
    {
        return connectedPortal;
    }

    public void SetMaskShader(Shader shader,int stencilValue, int readMaskValue, int renderQueue)
    {
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
