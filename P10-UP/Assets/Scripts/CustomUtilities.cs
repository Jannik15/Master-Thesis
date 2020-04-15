using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public static class CustomUtilities
{
    #region Renderer

    public static void InstantiateMaterials(GameObject parent)
    {
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                renderers[i].materials[j] = Material.Instantiate(renderers[i].materials[j]);
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
    /// Update the stencils, discard matrix, and render queue of all renderers in the parent.
    /// If portal == null discard matrix is not updated, and if renderQueue == 0 it is not updated either.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="newStencilValue"></param>
    public static void UpdateStencils(GameObject parent, Transform portal, int newStencilValue, int renderQueue)
    {
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                renderers[i].materials[j].SetInt("_StencilValue", newStencilValue);
                if (renderQueue > 0)
                {
                    renderers[i].materials[j].renderQueue = renderQueue;
                }
                if (portal != null)
                {
                    renderers[i].materials[j].SetMatrix("_WorldToPortal", portal.worldToLocalMatrix);
                }
            }
        }
    }

    /// <summary>
    /// Update the stencils, discard matrix, and render queue of all renderers in the parent.
    /// If portal == null discard matrix is not updated, and if renderQueue == 0 it is not updated either.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="newStencilValue"></param>
    public static void UpdateStencils(Renderer[] renderers, Transform portal, int newStencilValue, int renderQueue)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                renderers[i].materials[j].SetInt("_StencilValue", newStencilValue);
                if (renderQueue > 0)
                {
                    renderers[i].materials[j].renderQueue = renderQueue;
                }
                if (portal != null)
                {
                    renderers[i].materials[j].SetMatrix("_WorldToPortal", portal.worldToLocalMatrix);
                }
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
    /// Set readMaskValue <= 0 to ignore.
    /// </summary>
    /// <param name="portal"></param>
    /// <param name="newStencilValue"></param>
    /// <param name="readMaskValue"></param>
    /// <param name="baseRenderQueueValue"></param>
    /// <param name="portalShader"></param>
    /// <param name="enablePortal"></param>
    public static void UpdatePortalAndItsConnectedRoom(Portal portal, int newStencilValue, int readMaskValue, int baseRenderQueueValue, Shader portalShader, bool enablePortal)
    {
        portal.SetActive(enablePortal);
        portal.SetMaskShader(portalShader, newStencilValue, readMaskValue, baseRenderQueueValue + 100);

        portal.GetConnectedRoom().gameObject.SetActive(true);
        portal.GetConnectedRoom().UpdateRoomStencil(portal.transform, newStencilValue, baseRenderQueueValue + 300);
    }

    /// <summary>
    /// Get the stencil value of the first renderer object in the gameObject.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static int GetStencil(GameObject obj)
    {
        Renderer rendererInObj = obj.GetComponentInChildren<Renderer>();
        return rendererInObj.material.GetInt("_StencilValue");
    }

    #endregion

    #region Transform

    /// <summary>
    /// Change the parent from the currentParent to the newParent, for any objects that have a matching tag.
    /// </summary>
    /// <param name="currentParent"></param>
    /// <param name="newParent"></param>
    /// <param name="tag"></param>
    public static void ChangeParentWithTag(Transform currentParent, Transform newParent, string tag)
    {
        List<Transform> changedParentObjects = new List<Transform>();
        for (int i = 0; i < currentParent.childCount; i++)
        {
            if (currentParent.GetChild(i).CompareTag(tag))
            {
                currentParent.GetChild(i).parent = newParent;
            }
        }
    }

    #endregion

    #region Tiles
    /// <summary>
    /// Returns a double-list of tile positions, referred to as portal zones. Takes a list of tiles, and a size variable for the distance between tiles.
    /// If the tileSize is equal to the size of the tiles, the zones will only consist of horizontally and vertically neighboring tiles. If increased, it can include diagonally neighboring tiles.
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="tileSize"></param>
    /// <returns></returns>
    public static List<List<Vector2>> GetTilesAsZone(List<Vector2> tiles, float tileSize)
    {
        List<List<Vector2>> zones = new List<List<Vector2>>();
        zones.Add(new List<Vector2>());
        zones[0].Add(tiles[0]);

        List<int> connections = new List<int>();
        for (int i = 1; i < tiles.Count; i++)
        {
            for (int j = 0; j < zones.Count; j++)
            {
                for (int k = 0; k < zones[j].Count; k++)
                {
                    if (Vector3.Distance(tiles[i], zones[j][k]) <= tileSize * 1.1f) // There can be some floating point inaccuracies here, so we multiply by 1.1f to overshoot the comparison
                    {
                        connections.Add(j);
                        break;
                    }
                }
            }

            if (connections.Count > 0)
            {
                zones[connections[0]].Add(tiles[i]);
                if (connections.Count > 1)
                {
                    for (int j = 1; j < connections.Count; j++)
                    {
                        zones[connections[0]].AddRange(zones[connections[j]]);
                        zones.RemoveAt(connections[j]);
                    }
                }
            }
            else //(connections.Count == 0)
            {
                zones.Add(new List<Vector2>());
                zones[zones.Count - 1].Add(tiles[i]);
            }
            connections.Clear();
        }
        return zones;
    }

    /// <summary>
    /// [DEPRECATED] Returns a list of surrounding tiles to the input tiles (horizontally and vertically)
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    /// <returns></returns>
    public static List<Tile> CheckSurroundingTiles(Tile[,] tiles, int xIndex, int yIndex)
    {
        List<Tile> surroundingTiles = new List<Tile>();
        List<Tile> portalTiles = new List<Tile> { tiles[xIndex, yIndex] };
        if (xIndex > 0)
        {
            surroundingTiles.Add(tiles[xIndex - 1, yIndex]);
        }
        if (xIndex < tiles.GetLength(1) - 1)
        {
            surroundingTiles.Add(tiles[xIndex + 1, yIndex]);
        }
        if (yIndex > 0)
        {
            surroundingTiles.Add(tiles[xIndex, yIndex - 1]);
        }
        if (yIndex < tiles.GetLength(1) - 1)
        {
            surroundingTiles.Add(tiles[xIndex, yIndex + 1]);
        }
        for (int i = 0; i < surroundingTiles.Count; i++)
        {
            if (surroundingTiles[i].GetTileType() == TileGeneration.TileType.Portal)
            {
                portalTiles.Add(surroundingTiles[i]);
            }
        }

        return portalTiles;
    }

    #endregion

    #region Rooms

    public static void ChangeRoom(this Transform thisObject, Room currentRoom, Room newRoom, bool playerCanCollide)
    {
        currentRoom?.RemoveObjectFromRoom(thisObject); // If there is a 'Room' component in parent (meaning it has previously been assigned to a room) remove it from that room.
        newRoom.AddObjectToRoom(thisObject, playerCanCollide);
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

    #region Layers and Tags

    public static int LayerMaskToLayer(LayerMask layerMask)
    {
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 0)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber - 1;
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
