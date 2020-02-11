using System.Collections.Generic;
using RendererExtensions;
using UnityEngine;

public class JTestScript : MonoBehaviour
{
    // Purpose: Test swapping shaders AND masks on a button press with minimal design effort
    private ShaderSwapper swapper;
    private ShaderLocater shaderLocater;
    private List<Portal> forwardPortals, backwardPortals;
    [SerializeField] private ShaderContainer materialContainer, maskContainer;
    [SerializeField] private GameObject[] rooms;
    [SerializeField] private int stencilMaxValue = 1;
    [TagSelector] [SerializeField] private string[] portalTags;
    private int stencilMaterialIterator, stencilMaskIterator;
    private List<List<Renderer>> objectsInRooms, materialObjects, maskObjects;
    public int roomIndex;

    void Awake()
    {
        swapper = new ShaderSwapper();
        shaderLocater = new ShaderLocater();
        forwardPortals = new List<Portal>();
        backwardPortals = new List<Portal>();
        shaderLocater.LoadPortals(forwardPortals,backwardPortals, portalTags);

        objectsInRooms = new List<List<Renderer>>();
        // Find objects in the given rooms and store them in a list
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


            
            //shaderLocater.UpdateShaderMatrix(materialObjects[roomIndex], shaderLocater.GetPortalTransform(forwardPortals, 1));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) // Change shader on geometry
        {
            stencilMaterialIterator++;
            if (stencilMaterialIterator == stencilMaxValue+1)
            {
                stencilMaterialIterator = 0;
            }
            ChangeShaderStencil(materialObjects[roomIndex], stencilMaterialIterator);
            Debug.Log("Changing stencil value to " + stencilMaskIterator + " for " + materialObjects[roomIndex].Count + " materials in room " + roomIndex);

            for (int i = 0; i < forwardPortals.Count; i++)
            {
                if (forwardPortals[i].GetPortalStencilValue() == stencilMaterialIterator - 1)
                {
                    shaderLocater.UpdateShaderMatrix(materialObjects[roomIndex], forwardPortals[i].GetPortalTransform());
                    Debug.Log("Changed stencil value for portal to " + stencilMaterialIterator);
                    break;
                }
            }
            //shaderLocater.UpdateShaderMatrix(materialObjects[roomIndex], shaderLocater.GetPortalTransform(backwardPortals, stencilMaterialIterator));

        }
        if (Input.GetKeyDown(KeyCode.N)) // Change shader on masks
        {
            stencilMaskIterator++;
            if (stencilMaskIterator == stencilMaxValue+1)
            {
                stencilMaskIterator = 0;
            }
            ChangeShaderStencil(maskObjects[roomIndex], stencilMaskIterator); 
            //shaderLocater.UpdateShaderMatrix(materialObjects[roomIndex], shaderLocater.GetPortalTransform(forwardPortals, stencilMaskIterator));
            Debug.Log("Changing stencil value to " + stencilMaskIterator + " for " + maskObjects[roomIndex].Count + " masks in room " + roomIndex);
        }

        if (Input.GetKeyDown(KeyCode.R)) // Change the room index
        {
            roomIndex++;
            if (roomIndex == objectsInRooms.Count)
            {
                roomIndex = 0;
            }
            ChangeShaderStencil(materialObjects[roomIndex], stencilMaterialIterator);
            ChangeShaderStencil(maskObjects[roomIndex], stencilMaskIterator);
        }

        shaderLocater.UpdateShaderMatrix(objectsInRooms[1], forwardPortals[0].GetPortalTransform());

    }

    // Brute force recursion - could be optimized by knowing which children to search in advance
    void RecursivelySearchChildrenForRenderers(Transform objToSearch, List<Renderer> list)
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

    public void ChangeShaderStencil(List<Renderer> objectsToSwap, int stencilValue)
    {
        for (int i = 0; i < objectsToSwap.Count; i++)
        {
            for (int j = 0; j < objectsToSwap[i].materials.Length; j++)
            {
                objectsToSwap[i].materials[j].SetInt("_StencilValue", stencilValue);
            }
        }
    }
}
