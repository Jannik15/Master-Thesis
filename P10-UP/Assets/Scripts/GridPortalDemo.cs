using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridPortalDemo : MonoBehaviour
{
    // Inspector variables
    public List<GameObject> grids;
    public List<Room> rooms = new List<Room>();
    public List<Portal> portals = new List<Portal>();
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private GameObject depthClearer;
    [SerializeField] private int roomAmount = 10;

    // Public non-inspector variables
    [HideInInspector] public Room currentRoom, previousRoom; // Room cannot currently be displayed in the inspector, requires a CustomInspectorDrawer implementation.

    // Private variables
    private List<Vector2> possiblePortalPositions = new List<Vector2>();
    private GameObject roomObject; // Functions as the index in rooms, tracking which room the player is in
    private Transform portalParent;
    private int roomId, portalIterator;

    private void Start() // To uncomment a demo block, simply put a '/' in front of the '/*'
    {
        //* Procedural generation
        ProcedurallyGenerateRooms(grids);
        //*/

        //* Depth clearing
        Transform depthParent = depthClearer.transform.parent;
        for (int i = 1; i < rooms.Count; i++)
        {
            GameObject newDepthClearer = Instantiate(depthClearer);
            newDepthClearer.transform.parent = depthParent;
            newDepthClearer.name = newDepthClearer.name + "_" + (i + 1);
            newDepthClearer.GetComponent<Renderer>().material.SetInt("_StencilValue", i + 1);
        }
        //*/
        CustomUtilities.UpdateStencils(rooms[0].room,0, true);
        CustomUtilities.UpdateShaderMatrix(rooms[1].room, rooms[1].GetPortalsToRoom()[0].transform);
    }
    
    private void ProcedurallyGenerateRooms(List<GameObject> grids)
    {
        // TODO: Stop portals from spawning too close to grid edge (if they do, they should be turned such that the edge is perpendicular to them)
        // TODO: If 3+ Zones, try creating another path, try diverging in the generation

        portalParent = new GameObject("Portals").transform;

        List<Tile> gridTiles = new List<Tile>();
        List<Vector2> portalTilesLocations = new List<Vector2>();
        List<List<Vector2>> portalZones, previousPortalZones = new List<List<Vector2>>();

        while (rooms.Count < roomAmount)
        {
            int index = Random.Range(0, grids.Count);
            roomObject = Instantiate(grids[index]);
            Grid grid = grids[index].GetComponent<Grid>();
            gridTiles.Clear();
            gridTiles.AddRange(grid.GetTilesAsList());
            portalTilesLocations.Clear();

            for (int j = 0; j < gridTiles.Count; j++)
            {
                if (gridTiles[j].GetTileType() == TileGeneration.TileType.Portal)
                {
                    portalTilesLocations.Add(gridTiles[j].GetPosition());
                }
            }
            portalZones = CustomUtilities.PortalZones(portalTilesLocations, gridTiles[0].transform.localScale.x);
            // Create the portals TODO: Update
            // - Step 1: Find all the portal tiles in Room n-1 and n if n > 0
            // - Step 2: Place two portals with opposite rotations on a portal tile that is overlapping in both rooms
            // - Step 3: Update shader matrix
            if (rooms.Count > 0)
            {
                int zoneUsed = -1;
                for (int i = 0; i < portalZones.Count; i++)
                {
                    for (int j = 0; j < portalZones[i].Count; j++)
                    {
                        for (int k = 0; k < previousPortalZones.Count; k++)
                        {
                            if (previousPortalZones[k].Contains(portalZones[i][j]))
                            {
                                possiblePortalPositions.Add(portalZones[i][j]);
                                zoneUsed = i;
                            }
                        }
                    }
                    if (zoneUsed != -1) // Only store tiles from a single zone
                    {
                        portalZones.RemoveAt(i);
                        break;
                    }
                }

                if (possiblePortalPositions.Count > 0)
                {
                    roomId++;     // RoomId = stencil value of the room, and the index + 1 of the room in the roomList
                    rooms.Add(new Room(roomObject, roomId + 1));

                    // Create portals connecting the two rooms
                    float randomRotation = Random.Range(0, 360);
                    Vector3 randomPosition = possiblePortalPositions[Random.Range(0, possiblePortalPositions.Count)].ToVector3XZ();
                    GameObject portal = Instantiate(portalPrefab, randomPosition, Quaternion.Euler(0, randomRotation, 0), portalParent);
                    GameObject oppositePortal = Instantiate(portalPrefab, randomPosition, Quaternion.Euler(0, randomRotation - 180, 0), portalParent);
                    portal.name = portal.name + "_" + portalIterator;
                    oppositePortal.name = oppositePortal.name + "_" + (portalIterator + 1);
                    Portal portalComponent = portal.AddComponent<Portal>();
                    Portal oppositePortalComponent = oppositePortal.AddComponent<Portal>();
                    portalComponent.AssignValues(rooms[roomId - 1], rooms[roomId], oppositePortalComponent, portalIterator);
                    oppositePortalComponent.AssignValues(rooms[roomId], rooms[roomId - 1], portalComponent, portalIterator + 1);
                    rooms[roomId].AddPortalInRoom(oppositePortalComponent);
                    rooms[roomId].AddPortalToRoom(portalComponent);
                    rooms[roomId - 1].AddPortalInRoom(portalComponent);
                    rooms[roomId - 1].AddPortalToRoom(oppositePortalComponent);
                    portalIterator += 2;
                    portals.Add(portalComponent);
                    portals.Add(oppositePortalComponent);
                    CustomUtilities.InstantiateMaterials(portal);
                    CustomUtilities.InstantiateMaterials(oppositePortal);

                    // Change the parent for objects surrounding portals to the rooms, so the stencils can be changed 
                    CustomUtilities.ChangeParentWithTag(oppositePortal.transform, rooms[roomId].room.transform, "PortalObjects");
                    CustomUtilities.ChangeParentWithTag(portal.transform, rooms[roomId - 1].room.transform, "PortalObjects");

                    // Set stencil values for rooms and portals, and update shader matrix with the new portal locations
                    CustomUtilities.UpdateStencils(rooms[roomId].room, roomId + 1, true);
                    CustomUtilities.UpdateStencils(rooms[roomId - 1].room, roomId, true);

                    possiblePortalPositions.Clear();
                }
                else // If false, no portal tiles were overlapping - undo generation and try another room
                {
                    Destroy(roomObject);
                    roomObject = rooms[roomId].room;
                    continue;
                }
            }
            else
            {
                rooms.Add(new Room(roomObject, roomId + 1));
            }

            for (int i = 0; i < previousPortalZones.Count; i++) // TODO: Might be unnecessary - Test for optimization
            {
                previousPortalZones[i].Clear();
            }
            previousPortalZones.Clear();
            previousPortalZones.AddRange(portalZones);
        }

        for (int j = 0; j < portals.Count; j++)
        {
            //if (j % 2 != 0)
            //{
            //    portals[j].SetActive(false);
            //}
            if (j > 0)
            {
                portals[j].SetActive(false);
            }
        }
        roomId = 1;
        currentRoom = rooms[roomId - 1];
    }

    #region World switching on portal collision
    /// <summary>
    /// Step 1/2 in switching world with portals. Should occur when player enters a portal.
    /// Updates the current rooms stencil value to the new rooms, then set the new rooms stencil value to 0 and set the current room to the new room.
    /// </summary>
    /// <param name="portal"></param>
    public void SwitchWorld(Portal portal)
    {
        previousRoom = currentRoom;
        currentRoom = portal.GetConnectedRoom();

        Debug.Log("World switch registered. Current room id is: " + currentRoom.GetRoomId() + " with name " + currentRoom.room.name);
        // Step 1: Change previous room stencil from 0 to its room id, and current room stencil to 0
        CustomUtilities.UpdateStencils(previousRoom.room, previousRoom.GetRoomId(), true);
        CustomUtilities.UpdateStencils(currentRoom.room, 0, true);
        // Step 2: Enable portals in new room that are not at the same position as the current portal, and update the shader matrix of the room they are connected with
        List<Portal> portalsInRoom = currentRoom.GetPortalsInRoom();
        for (int i = 0; i < portalsInRoom.Count; i++)
        {
            if (portalsInRoom[i] != portal.GetConnectedPortal())
            {
                portalsInRoom[i].SetActive(true);
                CustomUtilities.UpdateShaderMatrix(portalsInRoom[i].GetConnectedRoom().room, portalsInRoom[i].transform);
            }
        }
        //Step 3: Disable portals in the previous room, except for the current portal
        for (int i = 0; i < previousRoom.GetPortalsInRoom().Count; i++)
        {
            if (previousRoom.GetPortalsInRoom()[i] != portal)
            {
                previousRoom.GetPortalsInRoom()[i].SetActive(false);
            }
        }
        // Step 4: Update shader matrix for previous room with the current portals transform (backward stencils)
        CustomUtilities.UpdateShaderMatrix(previousRoom.room, portal.transform);


    }

    /// <summary>
    /// Step 2/2 in switching world with portals. Should occur when player exits the portal.
    /// Enables the corresponding portal in the new room, and disables all portals in the previous room, in addition to updating their shader matrices.
    /// </summary>
    /// <param name="portal"></param>
    public void FinalizeWorldSwitch(Portal portal) // Successful world switch
    {
        // Step 1: Enable the connected portal and disable the current portal from the previous room
        Portal connectedPortal = portal.GetConnectedPortal();
        connectedPortal.SetActive(true);
        portal.SetActive(false);
        // Step 2: Update shader matrix for previous room with the connected portals transform
        CustomUtilities.UpdateShaderMatrix(previousRoom.room, connectedPortal.transform);
    }

    /// <summary>
    /// Undoes step 1/2 in switching world with portals. Should occur when player incorrectly exits portal (same side as where they entered).
    /// </summary>
    /// <param name="portal"></param>
    public void UndoSwitchWorld(Portal portal)
    {
        // Step 1: Revert stencil change made in SwitchWorld()
        CustomUtilities.UpdateStencils(currentRoom.room, currentRoom.GetRoomId(), true);
        CustomUtilities.UpdateStencils(previousRoom.room, 0, true);
        // Step 2: Disable the portals in the current room
        for (int i = 0; i < currentRoom.GetPortalsInRoom().Count; i++)
        {
            currentRoom.GetPortalsInRoom()[i].SetActive(false);
        }
        // Step 3: Set the current room to the previous room and enable the portals
        currentRoom = previousRoom;
        List<Portal> portalsInRoom = currentRoom.GetPortalsInRoom();
        for (int i = 0; i < portalsInRoom.Count; i++)
        {
            portalsInRoom[i].SetActive(true);
            CustomUtilities.UpdateShaderMatrix(portalsInRoom[i].GetConnectedRoom().room, portalsInRoom[i].transform);
        }
    }
    #endregion
}
