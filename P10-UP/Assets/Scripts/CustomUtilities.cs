using System.Collections.Generic;
using UnityEngine;

public static class CustomUtilities
{
    #region Renderer

    public static void InstantiateMaterials(GameObject parent)
    {
        Renderer[] renderersInRoom = parent.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderersInRoom.Length; i++)
        {
            for (int j = 0; j < renderersInRoom[i].materials.Length; j++)
            {
                renderersInRoom[i].materials[j] = Material.Instantiate(renderersInRoom[i].materials[j]);
            }
        }
    }

    /// <summary>
    /// Update the stencil value for materials in a list of renderers.
    /// </summary>
    /// <param name="roomMaterials"></param>
    /// <param name="newStencilValue"></param>
    public static void UpdateRoomStencil(List<Renderer> roomMaterials, int newStencilValue)
    {
        for (int i = 0; i < roomMaterials.Count; i++)
        {
            for (int j = 0; j < roomMaterials[i].materials.Length; j++)
            {
                roomMaterials[i].materials[j].SetInt("_StencilValue", newStencilValue);
            }
        }
    }

    /// <summary>
    /// Update the stencil value for materials in a list of renderers in a room. Also updates their shader matrix with the portal transform.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="newStencilValue"></param>
    public static void UpdateRoomStencil(GameObject room, int newStencilValue, Transform portal)
    {
        Renderer[] renderersInRoom = room.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderersInRoom.Length; i++)
        {
            for (int j = 0; j < renderersInRoom[i].materials.Length; j++)
            {
                renderersInRoom[i].materials[j].SetInt("_StencilValue", newStencilValue);
                renderersInRoom[i].materials[j].SetMatrix("_WorldToPortal", portal.worldToLocalMatrix);
            }
        }
    }

    /// <summary>
    /// Update the stencil value for materials in a list of renderers in a gameobject and its children.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="newStencilValue"></param>
    public static void UpdateStencils(GameObject parent, int newStencilValue, bool setRenderQueue)
    {
        Renderer[] renderersInRoom = parent.GetComponentsInChildren<Renderer>();
        if (setRenderQueue)
        {
            int newRenderQueueValue = 2300;
            if (newStencilValue == 0)
            {
                newRenderQueueValue = 2000;
            }
            for (int i = 0; i < renderersInRoom.Length; i++)
            {
                for (int j = 0; j < renderersInRoom[i].materials.Length; j++)
                {
                    renderersInRoom[i].materials[j].SetInt("_StencilValue", newStencilValue);
                    renderersInRoom[i].materials[j].renderQueue = newRenderQueueValue;
                }
            }
        }
        else
        {
            for (int i = 0; i < renderersInRoom.Length; i++)
            {
                for (int j = 0; j < renderersInRoom[i].materials.Length; j++)
                {
                    renderersInRoom[i].materials[j].SetInt("_StencilValue", newStencilValue);
                }
            }
        }
    }

    /// <summary>
    /// Update the shader matrix _WorldToPortal for each renderer material in the given list of renderers, with the relevant portal transform (Portals that are looking at them).
    /// </summary>
    /// <param name="roomMaterials"></param>
    /// <param name="portal"></param>
    public static void UpdateShaderMatrix(List<Renderer> roomMaterials, Transform portal)
    {
        for (int i = 0; i < roomMaterials.Count; i++)
        {
            for (int j = 0; j < roomMaterials[i].materials.Length; j++)
            {
                roomMaterials[i].materials[j].SetMatrix("_WorldToPortal", portal.worldToLocalMatrix);
            }
        }
    }
    /// <summary>
    /// Update the shader matrix _WorldToPortal for each renderer material in the given list of renderers, with the relevant portal transform (Portals that are looking at them).
    /// </summary>
    /// <param name="room"></param>
    /// <param name="portal"></param>
    public static void UpdateShaderMatrix(GameObject room, Transform portal)
    {
        Renderer[] roomMaterials = room.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < roomMaterials.Length; i++)
        {
            for (int j = 0; j < roomMaterials[i].materials.Length; j++)
            {
                roomMaterials[i].materials[j].SetMatrix("_WorldToPortal", portal.worldToLocalMatrix);
            }
        }
    }

    /// <summary>
    /// Get the stencil value of the first renderer object in the gameobject.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static int GetStencil(GameObject obj)
    {
        Renderer rendererInObj = obj.GetComponentInChildren<Renderer>();
        return rendererInObj.material.GetInt("_StencilValue");
    }

    #endregion

    #region Collections    
    /// <summary>
    /// Add all renderers in the given object's hierarchy to the given list using depth-first search.
    /// </summary>
    /// <param name="objToSearch"></param>
    /// <param name="list"></param>
    public static List<Renderer> RenderersInObjectHierarchy(Transform objToSearch)
    {
        List<Renderer> allRenderes = new List<Renderer>();
        allRenderes.AddRange(objToSearch.GetComponentsInChildren<Renderer>(true));
        return allRenderes;
    }
    #endregion

    #region P8-Utils


    static public Vector3 worldSpacePoint = new Vector3(0.0f, 0.0f, 0.0f);
    static int uniqueIterator = 0;

    static public void RandomizeArray(GameObject[] arr) // Fischer-Yates shuffle
    {
        for (int i = 0; i < arr.Length; i++)
        {
            int r = Random.Range(0, arr.Length - 1);
            GameObject temp = arr[i];
            arr[i] = arr[r];
            arr[r] = temp;
        }
    }

    /// For paired renderplanes (reversed)
    static public void SetActiveChild(Transform parent, bool enabled)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            child.gameObject.SetActive(enabled);
        }
    }

    /// Access other portals
    static public void SetActiveChild(Transform parent, bool enabled, string _tag)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == _tag)
                child.gameObject.SetActive(enabled);
        }
    }

    static public void SetActiveChild(Transform parent, bool enabled, string _tag1, string _tag2)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == _tag1 || child.tag == _tag2)
                child.gameObject.SetActive(enabled);
        }
    }

    static public void SetActivePortal(Transform parent, bool enabled, string _tag1, string _tag2)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == _tag1 || child.tag == _tag2)
            {
                child.gameObject.SetActive(enabled);
            }
        }
    }

    static public void SetSiblingPortalActivity(Transform portal, bool enabled, string _tag1, string _tag2)
    {
        Transform parent = portal.transform.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform sibling = parent.GetChild(i);
            if (sibling.tag != portal.tag && (sibling.tag == _tag1 || sibling.tag == _tag2))
            {
                sibling.gameObject.SetActive(enabled);
            }
        }
    }

    static public List<Transform> GetPortalTransformsInRoom(GameObject room, string _tag)
    {
        List<Transform> portalList = new List<Transform>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            Transform child = room.transform.GetChild(i);
            if (child.tag == _tag)
            {
                portalList.Add(child);
            }
        }
        return portalList;
    }

    static public List<Transform> GetPortalTransformsInRoom(GameObject room, string entryPortalTag, string exitPortalTag)
    {
        List<Transform> portalList = new List<Transform>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            Transform child = room.transform.GetChild(i);
            if (child.tag == entryPortalTag || child.tag == exitPortalTag)
            {
                portalList.Add(child);
            }
        }
        return portalList;
    }

    static public List<Vector3> GetPortalPositionsInRoom(GameObject room, string _tag, float rotationParameter)
    {
        List<Vector3> portalPositions = new List<Vector3>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            Transform child = room.transform.GetChild(i);
            if (child.tag == _tag)
            {
                if (rotationParameter > 0)
                {
                    Vector3 portal = child.position;
                    portal = Quaternion.Euler(0.0f, rotationParameter, 0.0f) * portal;
                    portal = new Vector3(Mathf.Round(portal.x * 100.0f) / 100.0f, Mathf.Round(portal.y * 100.0f) / 100.0f,
                        Mathf.Round(portal.z * 100.0f) / 100.0f); // Avoid floating-point comparison errors when rotating parent
                    portalPositions.Add(portal);
                }
                else
                {
                    portalPositions.Add(child.position);
                }
            }
        }
        return portalPositions;
    }

    static public List<Vector3> GetPortalPositionsInRoom(GameObject room, string entryPortalTag, string exitPortalTag, float rotationParameter)
    {
        List<Vector3> portalPositions = new List<Vector3>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            Transform child = room.transform.GetChild(i);
            if (child.tag == entryPortalTag || child.tag == exitPortalTag)
            {
                if (rotationParameter > 0)
                {
                    Vector3 portal = child.position;
                    portal = Quaternion.Euler(0.0f, rotationParameter, 0.0f) * portal;
                    portal = new Vector3(Mathf.Round(portal.x * 100.0f) / 100.0f, portal.y,
                        Mathf.Round(portal.z * 100.0f) / 100.0f); // Avoid floating-point comparison errors when rotating parent
                    portalPositions.Add(portal);
                }
                else
                    portalPositions.Add(child.position);
            }
        }
        return portalPositions;
    }

    static public List<Quaternion> GetPortalRotationsInRoom(GameObject room, string entryPortalTag, string exitPortalTag)
    {
        List<Quaternion> portalRotations = new List<Quaternion>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            Transform child = room.transform.GetChild(i);
            if (child.tag == entryPortalTag || child.tag == exitPortalTag)
            {
                portalRotations.Add(child.localRotation);
            }
        }
        return portalRotations;
    }

    static public Vector3 GetARandomPortalPositionInRoom(GameObject room, string entryPortalTag, string exitPortalTag)
    {
        List<Vector3> newExitPortals = new List<Vector3>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            Transform child = room.transform.GetChild(i);
            if (child.tag == entryPortalTag || child.tag == exitPortalTag)
            {
                newExitPortals.Add(child.localPosition);
            }
        }
        int r = Random.Range(0, newExitPortals.Count - 1);
        if (r < 0)
            Debug.Log("No portals found in requested room");
        return newExitPortals[r];
    }

    static public GameObject[] RemoveIndices(GameObject[] IndicesArray, int RemoveAt)
    {
        GameObject[] newIndicesArray = new GameObject[IndicesArray.Length - 1];

        int i = 0;
        int j = 0;
        while (i < IndicesArray.Length)
        {
            if (i != RemoveAt)
            {
                newIndicesArray[j] = IndicesArray[i];
                j++;
            }
            i++;
        }
        return newIndicesArray;
    }

    static public void ChangeLayersRecursively(Transform trans, int newLayer)
    {
        foreach (Transform child in trans)
        {
            child.gameObject.layer = newLayer;
            ChangeLayersRecursively(child, newLayer);
        }
    }

    static public bool VectorApproxComparison(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.0001;
    }

    #endregion

    #region Deprecated
    /*
    /// <summary>
    /// Initialize portals by providing the tag for the gameobjects that serve as portals.
    /// </summary>
    /// <param name="portalTag"></param>
    /// <returns></returns>
    public static List<Portal> InitializePortals(string portalTag)
    {
        List<Portal> portals = new List<Portal>();
        GameObject[] allPortals = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)); // TODO: Pass Portal objects after generation
        for (int i = 0; i < allPortals.Length; i++)
        {
            if (allPortals[i].CompareTag(portalTag))
            {
                portals.Add(new Portal(allPortals[i]));
            }
        }
        return portals;
    }

    */
    #endregion
}
