using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private GameObject inRoom;
    [SerializeField] private GameObject connectedRoom;
    [SerializeField] private GameObject connectedPortal;
    private GameObject forwardPortal, backwardPortal;
    private int forwardStencilValue, backwardStencilValue;
    private Renderer[] portalForwardMasks, portalBackwardMasks;
    [SerializeField] private int portalId;

    public void AssignValues(GameObject inRoom, GameObject connectedRoom, GameObject connectedPortal, int portalId)
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

    public int GetForwardStencilValue()
    {
        return forwardStencilValue;
    }

    public int GetBackwardStencilValue()
    {
        return backwardStencilValue;
    }

    public GameObject GetRoom()
    {
        return inRoom;
    }

    public GameObject GetConnectedRoom()
    {
        return connectedRoom;
    }
    public GameObject GetConnectedPortal()
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
