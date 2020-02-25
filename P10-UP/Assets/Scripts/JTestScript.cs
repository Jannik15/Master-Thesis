using System.Collections.Generic;
using UnityEngine;

public class JTestScript : MonoBehaviour
{
    // Purpose: Test swapping shaders AND masks on a button press with minimal design effort
    private List<Portal> portals;
    [SerializeField] private GameObject[] rooms;
    [SerializeField] private int stencilMaxValue = 1;
    [TagSelector] [SerializeField] private string portalTag;
    private int stencilMaterialIterator, stencilMaskIterator;
    private List<List<Renderer>> objectsInRooms, materialObjects, maskObjects;
    public int roomIndex;

    void Awake()
    {
        //portals = CustomUtilities.InitializePortals(portalTag);
        transform.Reset();
        objectsInRooms = new List<List<Renderer>>();
        for (int i = 0; i < rooms.Length; i++)
        {
            objectsInRooms.Add(CustomUtilities.RenderersInObjectHierarchy(rooms[i].transform));
        }
    }

    void Update() // TODO: Save useful code as helper methods
    {
        if (Input.GetKeyDown(KeyCode.M)) // Change shader on geometry
        {
            stencilMaterialIterator++;
            if (stencilMaterialIterator == stencilMaxValue + 1)
            {
                stencilMaterialIterator = 0;
            }
            CustomUtilities.UpdateRoomStencil(materialObjects[roomIndex], stencilMaterialIterator);
            Debug.Log("Changing stencil value to " + stencilMaskIterator + " for " + materialObjects[roomIndex].Count + " materials in room " + roomIndex);

            for (int i = 0; i < portals.Count; i++)
            {
                if (portals[i].GetForwardStencilValue() == stencilMaterialIterator - 1)
                {
                    //CustomUtilities.UpdateShaderMatrix(materialObjects[roomIndex], portals[i].GetTransform());
                    Debug.Log("Changed stencil value for portal to " + stencilMaterialIterator);
                    break;
                }
            }

        }
        if (Input.GetKeyDown(KeyCode.N)) // Change shader on masks
        {
            stencilMaskIterator++;
            if (stencilMaskIterator == stencilMaxValue + 1)
            {
                stencilMaskIterator = 0;
            }
            CustomUtilities.UpdateRoomStencil(maskObjects[roomIndex], stencilMaskIterator);
            Debug.Log("Changing stencil value to " + stencilMaskIterator + " for " + maskObjects[roomIndex].Count + " masks in room " + roomIndex);
        }

        if (Input.GetKeyDown(KeyCode.R)) // Change the room index
        {
            roomIndex++;
            if (roomIndex == objectsInRooms.Count)
            {
                roomIndex = 0;
            }
            CustomUtilities.UpdateRoomStencil(materialObjects[roomIndex], stencilMaterialIterator);
            CustomUtilities.UpdateRoomStencil(maskObjects[roomIndex], stencilMaskIterator);
        }

        //CustomUtilities.UpdateShaderMatrix(objectsInRooms[0], portals[0].GetTransform());
        //CustomUtilities.UpdateShaderMatrix(objectsInRooms[1], portals[0].GetTransform());

    }
}
