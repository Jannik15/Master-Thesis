using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralLayoutGeneration : MonoBehaviour
{
    // Inspector variables
    public List<GameObject> grids;
    public List<GameObject> endGrids;
    public List<Room> rooms = new List<Room>();
    public List<GameObject> keysList;
    public int roomAmount = 10;
    public GameObject mainMenuCanvas;

    [SerializeField] private List<GameObject> sceneryObjects, endGameEventObjects;
    [SerializeField] private GameObject portalPrefab, portalDoorPrefab, depthClearer, keyCard;
    [SerializeField] private Shader currentRoomMask, otherRoomMask;
    [SerializeField] private LayerMask currentRoomLayer, differentRoomLayer, defaultLayer;

    // Public non-inspector variables
    [HideInInspector] public Room currentRoom, previousRoom; // Room cannot currently be displayed in the inspector, requires a CustomInspectorDrawer implementation.

    // Private variables
    private List<Vector2> possiblePortalPositions = new List<Vector2>();
    private List<Portal> portals = new List<Portal>();
    private GameObject roomObject, keyCardToSpawn; // Functions as the index in rooms, tracking which room the player is in
    private Transform portalParent;
    private int roomId, portalIterator;
    private List<Tile> gridTiles = new List<Tile>(), previousGridTiles = new List<Tile>(), keyCardViableTiles = new List<Tile>();
    private List<Vector2> portalTilesLocations = new List<Vector2>();
    private List<List<Vector2>> previousPortalZones = new List<List<Vector2>>();
    private List<List<List<Vector2>>> portalZones = new List<List<List<Vector2>>>();
    private int keyCardID;

    public void AdjustRoomAmount(int newRoomAmount)
    {
        roomAmount = newRoomAmount;
    }

    public enum CustomRoomType
    {
        Start,
        End,
        Generic,
        Event
    }

    private void Start() // To uncomment a demo block, simply put a '/' in front of the '/*'
    {
        /* Procedural generation
        ProcedurallyGenerateRooms();
        //*/

        //* Start room generation
        GenerateOptionsRoom();
        //*/

        //* Depth clearing
        Transform depthParent = depthClearer.transform.parent;
        depthClearer.GetComponent<Renderer>().material.SetInt("_StencilValue", 1);
        depthClearer.GetComponent<Renderer>().material.renderQueue = 2500;

        for (int i = 8; i <= 128 ; i *= i)
        {
            DuplicateDepthClearer(depthParent, i, 2200);
        }
        for (int i = 2; i < 32; i++)
        {
            if (i == 8 || i == 16)
                continue;
            DuplicateDepthClearer(depthParent, i, 2500);
        }
        //*/

        //SwitchCurrentRoom(rooms[0], null);
    }

    private void DuplicateDepthClearer(Transform depthParent, int stencilValue, int renderQueue)
    {
        GameObject newDepthClearer = Instantiate(depthClearer);
        newDepthClearer.transform.parent = depthParent;
        newDepthClearer.name = newDepthClearer.name.Split('_')[0] + "_" + stencilValue;
        newDepthClearer.GetComponent<Renderer>().material.SetInt("_StencilValue", stencilValue);
        newDepthClearer.GetComponent<Renderer>().material.renderQueue = renderQueue;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("A input registered. Checking collidable objects (" + currentRoom.playerCollisionObjectsInRoom.Count + ") in room " + currentRoom.gameObject.name + "...");
            for (int i = 0; i < currentRoom.playerCollisionObjectsInRoom.Count; i++)
            {
                Debug.Log("#" + i + " = " + currentRoom.playerCollisionObjectsInRoom[i].gameObject.name);
            }
        }
    }

    private void GenerateOptionsRoom()
    {
        //* Make sure Main Menu canvas is Open
        if (!mainMenuCanvas.activeSelf)
        {
            mainMenuCanvas.SetActive(true);
        }
        //*/

        CreateRooms(1, grids, CustomRoomType.Start);

    }

    public void SwitchCurrentRoom()
    {
        SwitchCurrentRoom(rooms[0], null);

    }

    public void ProcedurallyGenerateRooms()
    {
        // TODO: Stop portals from spawning too close to grid edge (if they do, they should be turned such that the edge is perpendicular to them)
        // TODO: If 3+ Zones, try creating another path, try diverging in the generation

        portalParent = new GameObject("Portals").transform;

        //CreateRooms(1, grids, CustomRoomType.Start);
        CreateRooms(roomAmount - 2, grids, CustomRoomType.Generic);
        CreateRooms(1, endGrids, CustomRoomType.End);

        for (int j = 0; j < portals.Count; j++)
        {
            if (j > 0)
            {
                portals[j].SetActive(false);
            }
        }
        for (int i = 1; i < roomId + 1; i++)
        {
            rooms[i].SetLayer(CustomUtilities.LayerMaskToLayer(differentRoomLayer), CustomUtilities.LayerMaskToLayer(differentRoomLayer));
        }
        roomId = 1;
        currentRoom = rooms[0];
    }

    private void CreateRooms(int roomCount, List<GameObject> gridInputList, CustomRoomType roomType)
    {
        // Iterates over the amount of rooms specified in the constructor
        int roomIterator = 0;
        int iterationCap = 0;
        while (roomIterator < roomCount && iterationCap < 1000)
        {
            iterationCap++;
            // Takes a random grid from the list of input grids, and stores the grid component
            int newIndex = Random.Range(0, gridInputList.Count);
            GameObject gridObject = Instantiate(gridInputList[newIndex]);
            Grid grid = gridObject.GetComponent<Grid>();
            gridTiles.Clear();
            gridTiles.AddRange(grid.GetTilesAsList());
            portalZones.Clear();

            // Pair rooms if portals from previous room overlaps with the current room, otherwise delete the room and try again
            int roomRotation = 0;
            bool success;
            switch (roomType)
            {
                case CustomRoomType.Start:
                    rooms.Add(new Room(gridObject, roomId + 1, grid));
                    portalTilesLocations.Clear();
                    for (int i = 0; i < gridTiles.Count; i++)
                    {
                        if (gridTiles[i].GetTileType() == TileGeneration.TileType.Portal)
                        {
                            portalTilesLocations.Add(gridTiles[i].GetPosition());
                        }
                    }
                    int portalTileCount = portalTilesLocations.Count;
                    for (int i = 0; i < portalTileCount - 1; i++)
                    {
                        for (int j = i + 1; j < portalTileCount; j++)
                        {
                            if (math.distancesq(portalTilesLocations[i], portalTilesLocations[j]) <= math.pow(0.5f, 2f))
                            {
                                portalTilesLocations.Add(Vector2.Lerp(portalTilesLocations[i], portalTilesLocations[j], 0.5f));
                            }
                        }
                    }
                    portalZones.Add(CustomUtilities.PortalZones(portalTilesLocations, gridTiles[0].transform.localScale.x));
                    break;
                case CustomRoomType.End:
                    success = CheckIfRoomsCanBePaired(gridObject, grid, ref roomRotation);
                    if (!success)
                    {
                        Destroy(gridObject);
                        continue;
                    }
                    for (int i = 0; i < gridTiles.Count; i++)
                    {
                        if (gridTiles[i].GetTileType() == TileGeneration.TileType.Event)
                        {
                            rooms[roomId].AddObjectToRoom(Instantiate(endGameEventObjects[Random.Range(0, endGameEventObjects.Count)], gridTiles[i].GetPosition().ToVector3XZ(), 
                                Quaternion.identity, rooms[roomId].gameObject.transform).transform, true);
                            break;
                        }
                    }
                    break;
                case CustomRoomType.Generic:
                    success = CheckIfRoomsCanBePaired(gridObject, grid, ref roomRotation);
                    if (!success)
                    {
                        Destroy(gridObject);
                        continue;
                    }
                    break;
                case CustomRoomType.Event:
                    success = CheckIfRoomsCanBePaired(gridObject, grid, ref roomRotation);
                    if (!success)
                    {
                        Destroy(gridObject);
                        continue;
                    }
                    break;
            }

            roomIterator++;
            previousPortalZones.Clear();
            previousPortalZones.AddRange(portalZones[roomRotation]);
        }
        if (iterationCap == 1000)
        {
            Debug.LogError("Iteration cap for while loop of type " + roomType + " reached 1000");
        }
    }

    private bool CheckIfRoomsCanBePaired(GameObject gridObject, Grid grid, ref int roomRotation)
    {
        for (int i = 0; i < 4; i++)
        {
            gridObject.transform.eulerAngles = new Vector3(0.0f, 90.0f * i, 0.0f);
            if (i != 0)
            {
                for (int j = 0; j < gridTiles.Count; j++)
                {
                    gridTiles[j].SetPosition(gridTiles[j].transform.position.GetXZVector2());
                }
            }
            // Clear reference lists and add grid tiles and portal tiles to the lists
            portalTilesLocations.Clear();
            for (int j = 0; j < gridTiles.Count; j++)
            {
                if (gridTiles[j].GetTileType() == TileGeneration.TileType.Portal)
                {
                    portalTilesLocations.Add(gridTiles[j].GetPosition());
                }
            }
            portalZones.Add(CustomUtilities.PortalZones(portalTilesLocations, gridTiles[0].transform.localScale.x));

            // Add positions on edges of tiles to portal zones
            for (int j = 0; j < portalZones[i].Count; j++)
            {
                int zoneLength = portalZones[i][j].Count;
                for (int k = 0; k < zoneLength - 1; k++)
                {
                    for (int l = k + 1; l < zoneLength; l++)
                    {
                        if (math.distancesq(portalZones[i][j][k], portalZones[i][j][l]) <= math.pow(gridTiles[0].transform.localScale.x, 2))
                        {
                            portalZones[i][j].Add(Vector2.Lerp(portalZones[i][j][k], portalZones[i][j][l], 0.5f));
                        }
                    }
                }
            }
        }

        possiblePortalPositions.Clear();
        int zoneUsed = -1;
        bool breakAll = false;
        for (int i = 0; i < portalZones.Count; i++) // This value is always 4, since the rooms can have 4 different rotations (0, 90, 180, 270)
        {
            for (int j = 0; j < portalZones[i].Count; j++)
            {
                for (int k = 0; k < portalZones[i][j].Count; k++) 
                {
                    for (int l = 0; l < previousPortalZones.Count; l++)
                    {
                        for (int m = 0; m < previousPortalZones[l].Count; m++)
                        {
                            if (previousPortalZones[l][m] == portalZones[i][j][k]) // The == operator tests for approximate equality
                            {
                                possiblePortalPositions.Add(portalZones[i][j][k]);
                                zoneUsed = j;
                            }
                        }
                    }
                }
                if (zoneUsed != -1) // Only store tiles from a single zone
                {
                    portalZones[i].RemoveAt(j); // Remove the used portal zone so it cannot be used for pairing with future rooms (would cause portal overlap)
                    roomRotation = i;
                    breakAll = true;
                    break;
                }
            }
            if (breakAll)
            {
                break;
            }
        }
        if (possiblePortalPositions.Count > 0) // Portal overlap exists, pairing is possible.
        {
            roomId++; // RoomId = stencil value of the room, and the index + 1 of the room in the roomList TODO: AFTER BITMASKS Change this to just be the rooms id starting from 0.
            rooms.Add(new Room(gridObject, roomId + 1, grid)); // Add the room object as a Room with a unique ID and a grid

            gridObject.transform.eulerAngles = new Vector3(0.0f, 90.0f * roomRotation, 0.0f);
            for (int j = 0; j < gridTiles.Count; j++)
            {
                gridTiles[j].SetPosition(gridTiles[j].transform.position.GetXZVector2()); // Set rotation to the paired rotation
            }

            // Create portals connecting the two rooms
            float randomRotation = Random.Range(0, 360);
            int randomPortalPosition = Random.Range(0, possiblePortalPositions.Count);
            GameObject portal;
            
            int spawnDoor = Random.Range(0, 10);
            if (spawnDoor < 3 && roomId > 1)
            {
                portal = Instantiate(portalDoorPrefab, possiblePortalPositions[randomPortalPosition].ToVector3XZ(), Quaternion.Euler(0, randomRotation, 0), portalParent);
            }
            else
            {
                portal = Instantiate(portalPrefab, possiblePortalPositions[randomPortalPosition].ToVector3XZ(), Quaternion.Euler(0, randomRotation, 0), portalParent);
            }
            GameObject oppositePortal = Instantiate(portalPrefab, possiblePortalPositions[randomPortalPosition].ToVector3XZ(),
                Quaternion.Euler(0, randomRotation - 180, 0), portalParent);
            portal.name = portal.name + "_" + portalIterator;
            oppositePortal.name = oppositePortal.name + "_" + (portalIterator + 1);

            // Occupy tiles for both portals
            previousGridTiles.Clear();
            previousGridTiles.AddRange(rooms[roomId - 1].roomGrid.GetTilesAsList());
            for (int i = 0; i < previousGridTiles.Count; i++)
            {
                if (math.distancesq(previousGridTiles[i].GetPosition(), possiblePortalPositions[randomPortalPosition]) <= math.pow(gridTiles[0].transform.localScale.x, 2))
                {
                    previousGridTiles[i].PlaceExistingObject(portal);
                }
            }
            for (int i = 0; i < gridTiles.Count; i++) // Occupying oppositePortalTiles with the opposite portal
            {
                if (math.distancesq(gridTiles[i].GetPosition(), possiblePortalPositions[randomPortalPosition]) <= math.pow(gridTiles[0].transform.localScale.x, 2))
                {
                    gridTiles[i].PlaceExistingObject(oppositePortal);
                }
            }

            if (spawnDoor < 3 && roomId > 1)
            {
                portal.GetComponentInChildren<KeyPad>().KeyPadID = keyCardID;
                portal.GetComponentInChildren<DoorLock>().isLocked = true;

                int roomToSpawnKeyCardIn = Random.Range(1, roomId);

                keyCardViableTiles.Clear();
                Grid keyCardGrid = rooms[roomToSpawnKeyCardIn].gameObject.GetComponent<Grid>();
                for (int i = 0; i < keyCardGrid.GetTilesAsList().Count; i++)
                {
                    if (keyCardGrid.GetTilesAsList()[i].GetWalkable() && !keyCardGrid.GetTilesAsList()[i].GetOccupied())
                    {
                        keyCardViableTiles.Add(keyCardGrid.GetTilesAsList()[i]);
                    }
                }

                int keyCardTileIndex = Random.Range(0, keyCardViableTiles.Count);
                keyCardToSpawn = keyCardViableTiles[keyCardTileIndex].PlaceObject(keyCard, rooms[roomId].gameObject.transform);
                
                rooms[roomToSpawnKeyCardIn].AddObjectToRoom(keyCardToSpawn.transform, true);
                keysList.Insert(keyCardID, keyCardToSpawn);
                keyCardToSpawn.GetComponentInChildren<KeyCard>().keyID = keyCardID;
                keyCardID ++;
            }

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
            for (int i = 0; i < oppositePortal.transform.childCount; i++)
            {
                if (oppositePortal.transform.GetChild(i).CompareTag("PortalObjects"))
                {
                    oppositePortal.transform.GetChild(i).ChangeRoom(null,rooms[roomId], false);
                }
            }
            for (int i = 0; i < portal.transform.childCount; i++)
            {
                if (portal.transform.GetChild(i).CompareTag("PortalObjects"))
                {
                    portal.transform.GetChild(i).ChangeRoom(null, rooms[roomId - 1], false);
                }
            }

            // Set layer for room and portal
            portal.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);
            oppositePortal.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);

            if (rooms.Count > 3)
            {
                rooms[roomId].gameObject.SetActive(false);
            }
        }

        return possiblePortalPositions.Count > 0; // return whether or not portal tiles in current room are overlapping with portal tiles in previous rooms
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
        previousRoom.SetLayer(CustomUtilities.LayerMaskToLayer(differentRoomLayer), CustomUtilities.LayerMaskToLayer(differentRoomLayer));
        for (int i = 0; i < previousRoom.GetPortalsInRoom().Count; i++)
        {
            if (previousRoom.GetPortalsInRoom()[i] != currentPortal)
            {
                previousRoom.GetPortalsInRoom()[i].gameObject.layer = CustomUtilities.LayerMaskToLayer(differentRoomLayer);
            }
        }
        currentRoom.SetLayer(CustomUtilities.LayerMaskToLayer(currentRoomLayer), CustomUtilities.LayerMaskToLayer(defaultLayer));
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

        int stencilValue = 8; // BitMask 00001000
        int otherRoomStencilValue = 1;
        for (int i = 0; i < currentRoom.GetPortalsInRoom().Count; i++)
        {
            if (currentRoom.GetPortalsInRoom()[i].GetConnectedPortal() != currentPortal)
            {
                CustomUtilities.UpdatePortalAndItsConnectedRoom(currentRoom.GetPortalsInRoom()[i], stencilValue, 0, 2000, currentRoomMask, true);
            }
            else
            {
                // Previous room
                CustomUtilities.UpdateStencils(previousRoom.gameObject, stencilValue, 2300);
                CustomUtilities.UpdateStencils(currentPortal.gameObject, stencilValue, 2100);
                CustomUtilities.UpdateStencils(currentPortal.GetConnectedPortal().gameObject, stencilValue, 2100);
                CustomUtilities.UpdateShaderMatrix(previousRoom.gameObject, currentPortal.transform);   // This might not be necessary, but i don't know if the portal rotation matters in the matrix
            }
            
            portalsInConnectedRoom.AddRange(currentRoom.GetPortalsInRoom()[i].GetConnectedRoom().GetPortalsInRoom());
            for (int j = 0; j < portalsInConnectedRoom.Count; j++)
            {
                if (portalsInConnectedRoom[j].GetConnectedPortal() != currentRoom.GetPortalsInRoom()[i])
                {
                    CustomUtilities.UpdatePortalAndItsConnectedRoom(portalsInConnectedRoom[j], otherRoomStencilValue, stencilValue, 2300, otherRoomMask, true);
                    otherRoomStencilValue++;
                    if (otherRoomStencilValue == 8 || otherRoomStencilValue == 16 || otherRoomStencilValue == 32 || otherRoomStencilValue == 128) // Must never be equal to any iteration of stencilValue
                    {
                        otherRoomStencilValue++;
                    }
                }
            }
            portalsInConnectedRoom.Clear();
            stencilValue *= stencilValue; // BitShift 1 to the left
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
