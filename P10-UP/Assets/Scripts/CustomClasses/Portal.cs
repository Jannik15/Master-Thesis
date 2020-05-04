using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Room inRoom;
    [SerializeField] private Room connectedRoom;
    [SerializeField] private Portal connectedPortal;
    private GameObject forwardPortal, backwardPortal;
    [SerializeField] private int portalId;
    private List<Renderer> renderers = new List<Renderer>();

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
                renderers.AddRange(child.GetComponentsInChildren<Renderer>());
            }
            else if (child.CompareTag("BackwardPortal"))
            {
                backwardPortal = child;
                renderers.AddRange(child.GetComponentsInChildren<Renderer>());
            }
        }
    }

    public Renderer GetRenderer()
    {
        return renderers[0];
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

    public void SetLayer(int layer)
    {
        gameObject.layer = layer;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).CompareTag("InteractionHandler"))
            {
                transform.GetChild(i).gameObject.layer = layer;
                break;
            }
        }
    }

    public void SetMaskShader(Shader shader,int stencilValue, int readMaskValue, int renderQueue)
    {
        for (int i = 0; i < renderers.Count; i++)
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
