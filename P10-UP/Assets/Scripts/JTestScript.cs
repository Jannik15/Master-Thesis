using System.Collections.Generic;
using RendererExtensions;
using UnityEngine;

public class JTestScript : MonoBehaviour
{
    // Purpose: Test swapping shaders AND masks on a button press with minimal design effort
    private ShaderSwapper swapper;
    private ShaderLocater shaderLocater;
    [SerializeField] private ShaderContainer materialContainer, maskContainer;
    [SerializeField] private GameObject[] rooms;
    private List<List<Renderer>> objectsInRooms;
    private List<List<Renderer>> materialObjects;
    private List<List<Renderer>> maskObjects;
    public int roomIndex, materialIndex, maskIndex;

    void Awake()
    {
        swapper = new ShaderSwapper();
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
        }

        //for (int i = 0; i < materialObjects.Count; i++)
        //{
        //    for (int j = 0; j < materialObjects[i].Count; j++)
        //    {
        //        Debug.Log("MaterialObjects Index [" + i + "][" + j + "]: " + materialObjects[i][j].transform.name);
        //    }
        //}
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) // Change shader on geometry
        {
            materialIndex++;
            if (materialIndex == materialContainer.shaders.Length)
            {
                materialIndex = 0;
            }
            SwapShader(materialObjects[roomIndex], materialContainer.shaders[materialIndex]);
            Debug.Log("Swapping shader for " + materialObjects[roomIndex].Count + " materials in room " + roomIndex + " with the shader " + materialContainer.shaders[materialIndex]);

        }
        if (Input.GetKeyDown(KeyCode.N)) // Change shader on masks
        {
            maskIndex++;
            if (maskIndex == maskContainer.shaders.Length)
            {
                maskIndex = 0;
            }
            SwapShader(maskObjects[roomIndex], maskContainer.shaders[maskIndex]);
        }

        if (Input.GetKeyDown(KeyCode.R)) // Change the room index
        {
            roomIndex++;
            if (roomIndex == objectsInRooms.Count)
            {
                roomIndex = 0;
            }
            SwapShader(materialObjects[roomIndex], materialContainer.shaders[materialIndex]);
            SwapShader(maskObjects[roomIndex], maskContainer.shaders[maskIndex]);
        }
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
}
