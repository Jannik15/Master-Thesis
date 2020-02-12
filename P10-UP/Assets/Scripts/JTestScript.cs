using System.Collections.Generic;
using RendererExtensions;
using ThisProject.Utils;
using UnityEngine;

public class JTestScript : MonoBehaviour
{
    // Purpose: Test swapping shaders AND masks on a button press with minimal design effort
    private ShaderUtils shaderUtils;
    private List<Portal> portals;
    [SerializeField] private GameObject[] rooms;
    [SerializeField] private int stencilMaxValue = 1;
    [TagSelector] [SerializeField] private string portalTag;
    private int stencilMaterialIterator, stencilMaskIterator;
    private List<List<Renderer>> objectsInRooms, materialObjects, maskObjects;
    public int roomIndex;

    void Awake()
    {
        shaderUtils = new ShaderUtils(); // TODO: Check if this is necessary (it is, but there should be an easier way when using namespaces)
        portals = shaderUtils.InitializePortals(portalTag);
        
        objectsInRooms = new List<List<Renderer>>();
        // Find objects in the given rooms and store them in a list // TODO: Add helper method for this
        for (int i = 0; i < rooms.Length; i++)
        {
            List<Renderer> tempList = new List<Renderer>();
            RecursivelySearchChildrenForRenderers(rooms[i].transform, tempList);
            objectsInRooms.Add(tempList);
        }

        materialObjects = new List<List<Renderer>>(); 
        maskObjects = new List<List<Renderer>>();
        for (int i = 0; i < objectsInRooms.Count; i++)
        {
            List<Renderer> tempMaterialList = new List<Renderer>();
            List<Renderer> tempMaskList = new List<Renderer>();
            for (int j = 0; j < objectsInRooms[i].Count; j++)
            {
                if (objectsInRooms[i][j].CompareTag("Mask")) // TODO: Differentiate between masks that look for next rooms and masks that look for previous rooms
                {
                    tempMaskList.Add(objectsInRooms[i][j]);
                }
                else // TODO: Add more shader variations, need a way to determine the original shader and use the appropriate container
                {
                    tempMaterialList.Add(objectsInRooms[i][j]);
                }
            }
            materialObjects.Add(tempMaterialList);
            maskObjects.Add(tempMaskList);
        }
    }

    void Update() // TODO: Save useful code as helper methods
    {
        //if (Input.GetKeyDown(KeyCode.M)) // Change shader on geometry
        //{
        //    stencilMaterialIterator++;
        //    if (stencilMaterialIterator == stencilMaxValue+1)
        //    {
        //        stencilMaterialIterator = 0;
        //    }
        //    ChangeShaderStencil(materialObjects[roomIndex], stencilMaterialIterator);
        //    Debug.Log("Changing stencil value to " + stencilMaskIterator + " for " + materialObjects[roomIndex].Count + " materials in room " + roomIndex);

        //    for (int i = 0; i < forwardPortals.Count; i++)
        //    {
        //        if (forwardPortals[i].GetForwardStencilValue() == stencilMaterialIterator - 1)
        //        {
        //            shaderUtils.UpdateShaderMatrix(materialObjects[roomIndex], forwardPortals[i].GetTransform());
        //            Debug.Log("Changed stencil value for portal to " + stencilMaterialIterator);
        //            break;
        //        }
        //    }
        //    //shaderUtils.UpdateShaderMatrix(materialObjects[roomIndex], shaderUtils.GetPortalTransform(backwardPortals, stencilMaterialIterator));

        //}
        //if (Input.GetKeyDown(KeyCode.N)) // Change shader on masks
        //{
        //    stencilMaskIterator++;
        //    if (stencilMaskIterator == stencilMaxValue+1)
        //    {
        //        stencilMaskIterator = 0;
        //    }
        //    ChangeShaderStencil(maskObjects[roomIndex], stencilMaskIterator); 
        //    //shaderUtils.UpdateShaderMatrix(materialObjects[roomIndex], shaderUtils.GetPortalTransform(forwardPortals, stencilMaskIterator));
        //    Debug.Log("Changing stencil value to " + stencilMaskIterator + " for " + maskObjects[roomIndex].Count + " masks in room " + roomIndex);
        //}

        //if (Input.GetKeyDown(KeyCode.R)) // Change the room index
        //{
        //    roomIndex++;
        //    if (roomIndex == objectsInRooms.Count)
        //    {
        //        roomIndex = 0;
        //    }
        //    ChangeShaderStencil(materialObjects[roomIndex], stencilMaterialIterator);
        //    ChangeShaderStencil(maskObjects[roomIndex], stencilMaskIterator);
        //}

        //shaderUtils.UpdateShaderMatrix(objectsInRooms[0], forwardPortals[0].GetTransform());
        //shaderUtils.UpdateShaderMatrix(objectsInRooms[1], forwardPortals[0].GetTransform());
        //shaderUtils.UpdateShaderMatrix(objectsInRooms[0], backwardPortals[0].GetTransform());
        //shaderUtils.UpdateShaderMatrix(objectsInRooms[1], backwardPortals[0].GetTransform());

    }

    // Brute force recursion - could be optimized by knowing which children to search in advance
    void RecursivelySearchChildrenForRenderers(Transform objToSearch, List<Renderer> list) // TODO: Move to utility script
    {
        Renderer rend = objToSearch.GetComponent<Renderer>();
        if (rend != null)
        {
            list.Add(rend);
        }
        if (objToSearch.GetComponentInChildren<Renderer>() != null)
        {
            for (int i = 0; i < objToSearch.childCount; i++)
            {
                RecursivelySearchChildrenForRenderers(objToSearch.GetChild(i), list);
            }
        }
    }

    // Overload for multidimensional lists (lists containing arrays)
    public void SwapShader(List<Renderer> objectsToSwap, Shader materialShader)
    {
        for (int i = 0; i < objectsToSwap.Count; i++)
        {
            for (int j = 0; j < objectsToSwap[i].materials.Length; j++)
            {
                objectsToSwap[i].materials[j].shader = materialShader;
            }
        }
    }
}
