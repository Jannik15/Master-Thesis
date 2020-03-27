using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralLayoutGeneration : MonoBehaviour
{
    // Inspector variables
    public List<GameObject> grids;
    public List<GameObject> endGrids;
    public List<Room> rooms = new List<Room>();

    [SerializeField] private List<GameObject> sceneryObjects;
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private GameObject depthClearer;
    [SerializeField] private int roomAmount = 10;
    [SerializeField] private Shader currentRoomMask, otherRoomMask;
    [SerializeField] private LayerMask currentRoomLayer, differentRoomLayer;

    // Public non-inspector variables
    [HideInInspector] public Room currentRoom, previousRoom; // Room cannot currently be displayed in the inspector, requires a CustomInspectorDrawer implementation.

    // Private variables
    private List<Vector2> possiblePortalPositions = new List<Vector2>();
    private List<Portal> portals = new List<Portal>();
    private List<List<Portal>> portalsConnectedToPreviousRoom = new List<List<Portal>>();
    private GameObject roomObject; // Functions as the index in rooms, tracking which room the player is in
    private Transform portalParent;
    private int roomId, portalIterator;
    private List<Tile> gridTiles = new List<Tile>();
    private List<Vector2> portalTilesLocations = new List<Vector2>();
    private List<List<Vector2>> portalZones, previousPortalZones = new List<List<Vector2>>();

    public enum CustomRoomType
    {
        Start,
        End,
        Generic,
        Event
    }

    private void Start() // To uncomment a demo block, simply put a '/' in front of the '/*'
    {
        //* Procedural generation
        ProcedurallyGenerateRooms();
        //*/

        //* Depth clearing
        Transform depthParent = depthClearer.transform.parent;
        depthClearer.GetComponent<Renderer>().material.SetInt("_StencilValue", 1);
        depthClearer.GetComponent<Renderer>().material.renderQueue = 2500;

        for (int i = 2; i < 21; i++)
        {
            GameObject newDepthClearer = Instantiate(depthClearer);
            newDepthClearer.transform.parent = depthParent;
            newDepthClearer.name = newDepthClearer.name.Split('_')[0] + "_" + i;
            newDepthClearer.GetComponent<Renderer>().material.SetInt("_StencilValue", i);

            newDepthClearer.GetComponent<Renderer>().material.renderQueue = i % 5 == 0 ? 2200 : 2500;
        }
        //*/

        SwitchCurrentRoom(rooms[0], null);
    }

    private void ProcedurallyGenerateRooms()
    {
        // TODO: Stop portals from spawning too close to grid edge (if they do, they should be turned such that the edge is perpendicular to them)
        // TODO: If 3+ Zones, try creating another path, try diverging in the generation

        portalParent = new GameObject("Portals").transform;

        // Create start room
        CreateRooms(1, grids, CustomRoomType.Start);

        // Create generic rooms
        CreateRooms(roomAmount - 2, grids, CustomRoomType.Generic);

        #region Create end room
        int endIndex = Random.Range(0, endGrids.Count);
        GameObject gridObject = Instantiate(endGrids[endIndex]);
        Grid endGrid = gridObject.GetComponent<Grid>();
        gridTiles.Clear();
        gridTiles.AddRange(endGrid.GetTilesAsList());
        portalTilesLocations.Clear();

        for (int j = 0; j < gridTiles.Count; j++)
        {
            if (gridTiles[j].GetTileType() == TileGeneration.TileType.Portal)
            {
                portalTilesLocations.Add(gridTiles[j].GetPosition());
            }
        }
        portalZones = CustomUtilities.PortalZones(portalTilesLocations, gridTiles[0].transform.localScale.x);

        int tempZoneUsed = -1;
        for (int i = 0; i < portalZones.Count; i++)
        {
            for (int j = 0; j < portalZones[i].Count; j++)
            {
                for (int k = 0; k < previousPortalZones.Count; k++)
                {
                    if (previousPortalZones[k].Contains(portalZones[i][j]))
                    {
                        possiblePortalPositions.Add(portalZones[i][j]);
                        tempZoneUsed = i;
                    }
                }
            }
            if (tempZoneUsed != -1) // Only store tiles from a single zone
            {
                portalZones.RemoveAt(i);
                break;
            }
        }

        if (possiblePortalPositions.Count > 0)
        {
            roomId++;     // RoomId = stencil value of the room, and the index + 1 of the room in the roomList
            rooms.Add(new Room(gridObject, roomId + 1, endGrid));

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
            CustomUtilities.ChangeParentWithTag(oppositePortal.transform, rooms[roomId].gameObject.transform, "PortalObjects");
            CustomUtilities.ChangeParentWithTag(portal.transform, rooms[roomId - 1].gameObject.transform, "PortalObjects");

            // Set layer for room and portal
            rooms[roomId].SetLayer(CustomUtilities.LayerMaskToLayer(differentRoomLayer));
            portal.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);
            oppositePortal.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);

            possiblePortalPositions.Clear();
            rooms[roomId].gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("End room could not be paired. Instantiating anyway");
            for (int j = 0; j < gridTiles.Count; j++)
            {
                if (gridTiles[j].GetTileType() == TileGeneration.TileType.Portal)
                {
                    possiblePortalPositions.Add(gridTiles[j].GetPosition());
                }
            }

            roomId++;     // RoomId = stencil value of the room, and the index + 1 of the room in the roomList
            rooms.Add(new Room(gridObject, roomId + 1, endGrid));

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
            CustomUtilities.ChangeParentWithTag(oppositePortal.transform, rooms[roomId].gameObject.transform, "PortalObjects");
            CustomUtilities.ChangeParentWithTag(portal.transform, rooms[roomId - 1].gameObject.transform, "PortalObjects");

            // Set layer for room and portal
            rooms[roomId].SetLayer(CustomUtilities.LayerMaskToLayer(differentRoomLayer));
            portal.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);
            oppositePortal.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);

            possiblePortalPositions.Clear();
            rooms[roomId].gameObject.SetActive(false);
        }

        for (int i = 0; i < previousPortalZones.Count; i++) // TODO: Might be unnecessary - Test for optimization
        {
            previousPortalZones[i].Clear();
        }
        previousPortalZones.Clear();
        previousPortalZones.AddRange(portalZones);
        #endregion

        for (int j = 0; j < portals.Count; j++)
        {
            if (j > 0)
            {
                portals[j].SetActive(false);
            }
        }
        roomId = 1;
        currentRoom = rooms[0];
    }

    private void CreateRooms(int roomCount, List<GameObject> gridInputList, CustomRoomType roomType)
    {
        // Iterates over the amount of rooms specified in the constructor
        int roomIterator = 0;
        while (roomIterator < roomCount)
        {
            // Takes a random grid from the list of input grids, and stores the grid component
            int newIndex = Random.Range(0, gridInputList.Count);
            GameObject gridObject = Instantiate(gridInputList[newIndex]);
            Grid grid = gridObject.GetComponent<Grid>();

            // Clear reference lists and add grid tiles and portal tiles to the lists
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

            // Generate portalZones using portal tiles that are next to each other
            portalZones = CustomUtilities.PortalZones(portalTilesLocations, gridTiles[0].transform.localScale.x);

            // Pair rooms if portals from previous room overlaps with the current room, otherwise delete the room and try again
            if (rooms.Count > 0)
            {
                int zoneUsed = -1;
                for (int i = 0; i < portalZones.Count; i++) // TODO: Expand this method to include rotating rooms?
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
                        portalZones.RemoveAt(i); // Remove the used portal zone so it cannot be used for pairing with future rooms (would cause portal overlap)
                        break;
                    }
                }

                if (possiblePortalPositions.Count > 0) // Portal overlap exists, pairing is possible.
                {
                    roomId++; // RoomId = stencil value of the room, and the index + 1 of the room in the roomList TODO: AFTER BITMASKS Change this to just be the rooms id starting from 0.
                    rooms.Add(new Room(gridObject, roomId + 1, grid)); // Add the room object as a Room with a unique ID and a grid

                    for (int j = 0; j < gridTiles.Count; j++)
                    {
                        if (gridTiles[j].GetTileType() == TileGeneration.TileType.Scenery)
                        {
                            if (Random.Range(0, 2) < 1)
                            {
                                Instantiate(sceneryObjects[Random.Range(0, sceneryObjects.Count)], gridTiles[j].GetPosition().ToVector3XZ(), Quaternion.identity, rooms[roomId].gameObject.transform);
                            }
                        }
                    }

                    // Create portals connecting the two rooms
                    float randomRotation = Random.Range(0, 360);
                    Vector3 randomPosition = possiblePortalPositions[Random.Range(0, possiblePortalPositions.Count)]
                        .ToVector3XZ();
                    GameObject portal = Instantiate(portalPrefab, randomPosition,
                        Quaternion.Euler(0, randomRotation, 0), portalParent);
                    GameObject oppositePortal = Instantiate(portalPrefab, randomPosition,
                        Quaternion.Euler(0, randomRotation - 180, 0), portalParent);
                    portal.name = portal.name + "_" + portalIterator;
                    oppositePortal.name = oppositePortal.name + "_" + (portalIterator + 1);
                    Portal portalComponent = portal.AddComponent<Portal>();
                    Portal oppositePortalComponent = oppositePortal.AddComponent<Portal>();
                    portalComponent.AssignValues(rooms[roomId - 1], rooms[roomId], oppositePortalComponent,
                        portalIterator);
                    oppositePortalComponent.AssignValues(rooms[roomId], rooms[roomId - 1], portalComponent,
                        portalIterator + 1);
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
                    CustomUtilities.ChangeParentWithTag(oppositePortal.transform, rooms[roomId].gameObject.transform,
                        "PortalObjects");
                    CustomUtilities.ChangeParentWithTag(portal.transform, rooms[roomId - 1].gameObject.transform,
                        "PortalObjects");

                    // Set layer for room and portal
                    rooms[roomId].SetLayer(CustomUtilities.LayerMaskToLayer(differentRoomLayer));
                    portal.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);
                    oppositePortal.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);

                    possiblePortalPositions.Clear();
                    if (rooms.Count > 3)
                    {
                        rooms[roomId].gameObject.SetActive(false);
                    }
                }
                else // If false, no portal tiles were overlapping - undo generation and try another room
                {
                    Destroy(gridObject);
                    continue;
                }
            }
            else // If this is the first room being generated, simply add it to the list.
            {
                rooms.Add(new Room(gridObject, roomId + 1, grid));
            }

            roomIterator++;
            previousPortalZones.Clear();
            previousPortalZones.AddRange(portalZones);
        }
    }

    #region World switching on portal collision
    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="newCurrentRoom"></param>
    /// <param name="currentPortal"></param>
    public void SwitchCurrentRoom(Room newCurrentRoom, Portal currentPortal)
    {
        // Set current room and previous room | get the portals from the current room
        previousRoom = currentRoom;
        currentRoom = newCurrentRoom;
        CustomUtilities.UpdateStencils(currentRoom.gameObject, 0, 2000);
        previousRoom.SetLayer(CustomUtilities.LayerMaskToLayer(differentRoomLayer));
        for (int i = 0; i < previousRoom.GetPortalsInRoom().Count; i++)
        {
            if (previousRoom.GetPortalsInRoom()[i] != currentPortal)
            {
                previousRoom.GetPortalsInRoom()[i].gameObject.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);
            }
        }
        currentRoom.SetLayer(CustomUtilities.LayerMaskToLayer(currentRoomLayer));
        for (int i = 0; i < currentRoom.GetPortalsInRoom().Count; i++)
        {
            currentRoom.GetPortalsInRoom()[i].gameObject.layer = CustomUtilities.LayerMaskToLayer(currentRoomLayer);
        }

        #region Disable rooms connected to rooms that are connected to the previous room

        if (currentPortal != null)
        {
            for (int i = 0; i < previousRoom.GetPortalsInRoom().Count; i++)
            {
                if (previousRoom.GetPortalsInRoom()[i] != currentPortal)
                {
                    Room previousRoomConnection = previousRoom.GetPortalsInRoom()[i].GetConnectedRoom();

                    for (int j = 0; j < previousRoomConnection.GetPortalsInRoom().Count; j++)
                    {
                        if (previousRoomConnection.GetPortalsInRoom()[j].GetConnectedRoom() != previousRoom)
                        {
                            previousRoomConnection.GetPortalsInRoom()[j].GetConnectedRoom().gameObject.SetActive(false);
                        }
                        previousRoomConnection.GetPortalsInRoom()[j].SetActive(false);
                    }
                }
            }
        }
        #endregion

        List<Portal> portalsInConnectedRoom = new List<Portal>();
        #region Enable | Update stencils, render queue, shaders, Shader matrix, and layers for anything except the current room.

        for (int i = 0; i < currentRoom.GetPortalsInRoom().Count; i++)
        {
            if (currentRoom.GetPortalsInRoom()[i].GetConnectedPortal() != currentPortal)
            {
                CustomUtilities.UpdatePortalAndItsConnectedRoom(currentRoom.GetPortalsInRoom()[i], (i + 1) * 5, 2000, currentRoomMask, true);
            }
            else
            {
                // Previous room
                CustomUtilities.UpdateStencils(previousRoom.gameObject, (i + 1) * 5, 2300);
                CustomUtilities.UpdateStencils(currentPortal.gameObject, (i + 1) * 5, 2100);
                CustomUtilities.UpdateStencils(currentPortal.GetConnectedPortal().gameObject, (i + 1) * 5, 2100);
                CustomUtilities.UpdateShaderMatrix(previousRoom.gameObject, currentPortal.transform);   // This might not be necessary, but i don't know if the portal rotation matters in the matrix
            }
            
            portalsInConnectedRoom.AddRange(currentRoom.GetPortalsInRoom()[i].GetConnectedRoom().GetPortalsInRoom());
            for (int j = 0; j < portalsInConnectedRoom.Count; j++)
            {
                if (portalsInConnectedRoom[j].GetConnectedPortal() != currentRoom.GetPortalsInRoom()[i])
                {
                    CustomUtilities.UpdatePortalAndItsConnectedRoom(portalsInConnectedRoom[j], i * 5 + j + 1, 2300, otherRoomMask, true);
                }
            }
            portalsInConnectedRoom.Clear();
        }
        #endregion
    }
    
    /// <summary>
    /// Step 2/2 in switching world with portals. Should occur when player exits the portal.
    /// Enables the corresponding portal in the new room, and disables all portals in the previous room, in addition to updating their shader matrices.
    /// </summary>
    /// <param name="portal"></param>
    public void FinalizeRoomSwitch(Portal portal) // Successful world switch
    {
        // Step 1: Enable the connected portal and disable the current portal from the previous room
        Portal connectedPortal = portal.GetConnectedPortal();
        connectedPortal.SetActive(true);
        portal.SetActive(false);
        portal.gameObject.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);
        // Step 2: Update shader matrix for previous room with the connected portals transform
        CustomUtilities.UpdateShaderMatrix(previousRoom.gameObject, connectedPortal.transform);
    }
    #endregion
}
