using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class ProceduralLayoutGeneration : MonoBehaviour
{
    // Inspector variables
    public List<GameObject> grids;
    public List<GameObject> endGrids;
    public List<GameObject> keysList;
    public int roomAmount = 10;
    public GameObject mainMenuCanvas;

    [SerializeField] private GameObject[] sceneryObjects, eventObjects, enemyObjects, endGameEventObjects;
    [SerializeField] private GameObject portalPrefab, portalDoorPrefab, depthClearer, keyCard;
    [SerializeField] private Shader currentRoomMask, otherRoomMask;
    [SerializeField] private LayerMask currentRoomLayer, differentRoomLayer, defaultLayer;

    // Public non-inspector variables
    [HideInInspector] public Room currentRoom, previousRoom; // Room cannot currently be displayed in the inspector, requires a CustomInspectorDrawer implementation.
    [HideInInspector] public List<Room> rooms = new List<Room>(), genericRooms = new List<Room>(), eventRooms = new List<Room>();

    // Private variables
    private List<Vector2> possiblePortalPositions = new List<Vector2>();
    private List<Portal> portals = new List<Portal>();
    private List<List<Portal>> activeThroughPortals = new List<List<Portal>>();
    private GameObject roomObject, keyCardToSpawn; // Functions as the index in rooms, tracking which room the player is in
    private int roomId, portalIterator, keycardIterator;
    private Transform playerCam, portalParent;
    private List<Tile> gridTiles = new List<Tile>(), previousGridTiles = new List<Tile>(), walkableTiles = new List<Tile>(), keyCardViableTiles = new List<Tile>();
    private List<Tile> specificTypeTiles = new List<Tile>();
    private List<Vector2> portalTilePositions = new List<Vector2>();
    private List<List<Tile>> specificTypeZones = new List<List<Tile>>();
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

    private void FixedUpdate()
    {
        for (int i = 0; i < activeThroughPortals.Count; i++)
        {
            for (int j = 0; j < activeThroughPortals[i].Count; j++)
            {
                Vector3 cameraDirToPortal = (currentRoom.GetPortalsInRoom()[i].transform.position - playerCam.position).normalized;
                if (math.dot(cameraDirToPortal, currentRoom.GetPortalsInRoom()[i].transform.forward) >= 0)
                {
                    if (!activeThroughPortals[i][j].gameObject.activeSelf)
                    {
                        activeThroughPortals[i][j].gameObject.SetActive(true);
                    }
                }
                else if (activeThroughPortals[i][j].gameObject.activeSelf)
                {
                    activeThroughPortals[i][j].gameObject.SetActive(false);
                }
            }
        }
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

        for (int i = 8; i <= 128 ; i += i)
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
        playerCam = Camera.main.transform;
    }

    private void DuplicateDepthClearer(Transform depthParent, int stencilValue, int renderQueue)
    {
        GameObject newDepthClearer = Instantiate(depthClearer);
        newDepthClearer.transform.parent = depthParent;
        newDepthClearer.name = newDepthClearer.name.Split('_')[0] + "_" + stencilValue;
        newDepthClearer.GetComponent<Renderer>().material.SetInt("_StencilValue", stencilValue);
        newDepthClearer.GetComponent<Renderer>().material.renderQueue = renderQueue;
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
        // TODO: If 3+ Zones, try creating another path, try diverging in the generation

        portalParent = new GameObject("Portals").transform;

        //CreateRooms(1, grids, CustomRoomType.Start);
        CreateRooms(roomAmount - 2, grids, CustomRoomType.Generic);
        CreateRooms(1, endGrids, CustomRoomType.End);

        // Spawn objects
        //SpawnObjectType(genericRooms, TileGeneration.TileType.Event, eventObjects, 2);
        //SpawnObjectType(genericRooms, TileGeneration.TileType.Enemy, enemyObjects, 5);
        SpawnObjectType(genericRooms, TileGeneration.TileType.Scenery, sceneryObjects, 4, 4);

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
        roomId = 0;
        currentRoom = rooms[roomId];
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
                    rooms.Add(new Room(gridObject, roomId, grid));
                    portalTilePositions.Clear();
                    for (int i = 0; i < gridTiles.Count; i++)
                    {
                        if (gridTiles[i].GetTileType() == TileGeneration.TileType.Portal)
                        {
                            portalTilePositions.Add(gridTiles[i].GetPosition());
                        }
                    }
                    portalZones.Add(CustomUtilities.GetTilesAsZone(portalTilePositions, gridTiles[0].transform.localScale.x));
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
                            rooms[roomId].AddObjectToRoom(Instantiate(endGameEventObjects[Random.Range(0, endGameEventObjects.Length)], gridTiles[i].GetPosition().ToVector3XZ(), 
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
                    else
                    {
                        genericRooms.Add(rooms[roomId]);
                    }
                    break;
                case CustomRoomType.Event:
                    success = CheckIfRoomsCanBePaired(gridObject, grid, ref roomRotation);
                    if (!success)
                    {
                        Destroy(gridObject);
                        continue;
                    }
                    else
                    {
                        eventRooms.Add(rooms[roomId]);
                    }
                    break;
            }
            roomIterator++;
            previousPortalZones.Clear();
            previousPortalZones.AddRange(portalZones[roomRotation]);
        }
        if (iterationCap == 1000) // Avoid infinite loop for debugging purposes
        {
            Debug.LogError("Iteration cap for while loop of type " + roomType + " reached 1000");
        }
    }

    private bool CheckIfRoomsCanBePaired(GameObject gridObject, Grid grid, ref int roomRotation)
    {
        // Store portal tiles as as zones for each rotation of the room (0, 90, 180, 270 degrees)
        for (int i = 0; i < 4; i++)
        {
            // Clear reference lists and add grid tiles and portal tiles to the lists
            portalTilePositions.Clear();
            gridObject.transform.eulerAngles = new Vector3(0.0f, 90.0f * i, 0.0f);
            for (int j = 0; j < gridTiles.Count; j++)
            {
                if (i != 0)
                {
                    gridTiles[j].SetPosition(gridTiles[j].transform.position.GetXZVector2());
                }
                if (gridTiles[j].GetTileType() == TileGeneration.TileType.Portal)
                {
                    portalTilePositions.Add(gridTiles[j].GetPosition());
                }
            }
            portalZones.Add(CustomUtilities.GetTilesAsZone(portalTilePositions, gridTiles[0].transform.localScale.x));
        }

        // Check if the tiles in the previous portal zones overlap with tiles in the new portal zones.
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
                    // Remove the used portal zone so it cannot be used for pairing with future rooms (would cause portal overlap)
                    portalZones[i].RemoveAt(j);
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
            roomId++; // The id of the room, corresponding to the index in the list of rooms
            rooms.Add(new Room(gridObject, roomId, grid)); // Add the room object as a Room with a unique ID and a grid

            gridObject.transform.eulerAngles = new Vector3(0.0f, 90.0f * roomRotation, 0.0f);
            for (int j = 0; j < gridTiles.Count; j++)
            {
                gridTiles[j].SetPosition(gridTiles[j].transform.position.GetXZVector2()); // Set rotation to the paired rotation
            }

            // Add positions between portal tiles to possible portal locations

            int portalTileCount = possiblePortalPositions.Count;
            for (int i = 0; i < portalTileCount - 1; i++)
            {
                for (int j = i + 1; j < portalTileCount; j++)
                {
                    if (math.distancesq(possiblePortalPositions[i], possiblePortalPositions[j]) <= math.pow(gridTiles[0].transform.localScale.x, 2))
                    {
                        possiblePortalPositions.Add(Vector2.Lerp(possiblePortalPositions[i], possiblePortalPositions[j], 0.5f));
                    }
                }
            }
            // Create portals connecting the two rooms
            int randomPortalPosition = Random.Range(0, possiblePortalPositions.Count);
            Vector2 portalPosition = possiblePortalPositions[randomPortalPosition];
            GameObject portal;

            #region Determine Portal rotation
            float randomRotation = Random.Range(0, 360);
            //* 
            walkableTiles.Clear();
            for (int i = 0; i < gridTiles.Count; i++)
            {
                if (gridTiles[i].GetWalkable())
                {
                    walkableTiles.Add(gridTiles[i]);
                }
            }

            Vector2[] portalPositionCheck = new Vector2[4];
            portalPositionCheck[0] = new Vector2(portalPosition.x, portalPosition.y + walkableTiles[0].transform.lossyScale.z / 1.25f); // Up
            portalPositionCheck[1] = new Vector2(portalPosition.x - walkableTiles[0].transform.lossyScale.x / 1.25f, portalPosition.y); // Left
            portalPositionCheck[2] = new Vector2(portalPosition.x + walkableTiles[0].transform.lossyScale.x / 1.25f, portalPosition.y); // Right
            portalPositionCheck[3] = new Vector2(portalPosition.x, portalPosition.y - walkableTiles[0].transform.lossyScale.z / 1.25f); // Down
            bool[] portalPositionValidation = {false, false, false, false};

            for (int i = 0; i < portalPositionCheck.Length; i++)
            {
                for (int j = 0; j < walkableTiles.Count; j++)
                {
                    if (portalPositionCheck[i].IsWithinRect(walkableTiles[j].GetTileAsRect()))
                    {
                        portalPositionValidation[i] = true;
                        break;
                    }
                }
            }

            int falseAmount = 0;
            for (int i = 0; i < portalPositionValidation.Length; i++)
            {
                if (portalPositionValidation[i] == false)
                {
                    falseAmount++;
                }
            }

            if (falseAmount > 0) // On edge
            {
                if (falseAmount > 1) // 2 False tiles - In corner 
                {
                    if (portalPositionValidation[0] && portalPositionValidation[1] || portalPositionValidation[2] && portalPositionValidation[3])
                    {
                        randomRotation = Random.Range(0, 2) > 0 ? 45 : 225;
                    }
                    else
                    { 
                        randomRotation = Random.Range(0, 2) > 0 ? 135 : 315;
                    }
                }
                else // 1 False tile - should be perpendicular to the false tile
                {
                    if (!portalPositionValidation[0] || !portalPositionValidation[3])
                    {
                        randomRotation = Random.Range(0, 2) > 0 ? 90 : 270;
                    }
                    else // if (portalPositionValidation[1] || portalPositionValidation[2])
                    {
                        randomRotation = Random.Range(0, 2) > 0 ? 0 : 180;
                    }
                }
            }
            //Debug.Log("For grid " + gridObject.name + " false amount = " + falseAmount + ", so random rotation is set to " + randomRotation + "\n" + "{" + portalPositionCheck[0].ToString("0.00") + "=" + portalPositionValidation[0] + ", " + portalPositionCheck[1].ToString("0.00") + "=" + portalPositionValidation[1] + ", " + portalPositionCheck[2].ToString("0.00") + "=" + portalPositionValidation[2] + ", " + portalPositionCheck[3].ToString("0.00") + "=" + portalPositionValidation[3] + " }");
            //*/
#endregion

            // Choose what type of portal should spawn
            int spawnDoor = Random.Range(0, 10);
            int doorLockState = Random.Range(0, 2);
            if (spawnDoor < 3 && roomId > 1)
            {
                portal = Instantiate(portalDoorPrefab, portalPosition.ToVector3XZ(),
                    Quaternion.Euler(0, randomRotation, 0), portalParent);
                if (doorLockState == 0 && keycardIterator <= 9)
                {
                    portal.GetComponentInChildren<DoorLock>().isLocked = true;
                    keycardIterator++;
                } else
                {
                    portal.GetComponentInChildren<DoorLock>().isLocked = false;
                }
            }
            else
            {
                portal = Instantiate(portalPrefab, portalPosition.ToVector3XZ(), Quaternion.Euler(0, randomRotation, 0), portalParent);
            }
            GameObject oppositePortal = Instantiate(portalPrefab, portalPosition.ToVector3XZ(), Quaternion.Euler(0, randomRotation - 180, 0), portalParent);
            portal.name = portal.name + "_" + portalIterator;
            oppositePortal.name = oppositePortal.name + "_" + (portalIterator + 1);

            // Occupy tiles for both portals
            previousGridTiles.Clear();
            previousGridTiles.AddRange(rooms[roomId - 1].roomGrid.GetTilesAsList());
            for (int i = 0; i < previousGridTiles.Count; i++)
            {
                if (math.distance(previousGridTiles[i].GetPosition(), portalPosition) <= gridTiles[0].transform.localScale.x / 1.5f)
                {
                    previousGridTiles[i].PlaceExistingObject(portal);
                }
            }

            for (int i = 0; i < gridTiles.Count; i++) // Occupying oppositePortalTiles with the opposite portal
            {
                if (math.distance(gridTiles[i].GetPosition(), portalPosition) <= gridTiles[0].transform.localScale.x / 1.5f)
                {
                    gridTiles[i].PlaceExistingObject(oppositePortal);
                }
            }

            if (spawnDoor < 3 && roomId > 1 && portal.GetComponentInChildren<DoorLock>().isLocked == true)
            {
                portal.GetComponentInChildren<KeyPad>().KeyPadID = keyCardID;
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
                keyCardToSpawn = keyCardViableTiles[keyCardTileIndex]
                    .PlaceObject(keyCard, rooms[roomId].gameObject.transform);

                rooms[roomToSpawnKeyCardIn].AddObjectToRoom(keyCardToSpawn.transform, true);
                keysList.Insert(keyCardID, keyCardToSpawn);
                keyCardToSpawn.GetComponentInChildren<KeyCard>().keyID = keyCardID;
                keyCardID++;
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
                    oppositePortal.transform.GetChild(i).ChangeRoom(null, rooms[roomId], false);
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

    private void SpawnObjectType(List<Room> roomsToSpawnObjectsIn, TileGeneration.TileType objectTypeToSpawn, GameObject[] objectsToSpawn, int maxObjectsPerRoom, int tilesPerObject)
    {
        if (objectsToSpawn.Length > 0)
        {
            List<Tile> tilesToPlaceObjectOn = new List<Tile>();
            for (int i = 0; i < roomsToSpawnObjectsIn.Count; i++)
            {
                Grid grid = roomsToSpawnObjectsIn[i].roomGrid;
                gridTiles.Clear();
                gridTiles.AddRange(grid.GetTilesAsList());
                Vector2 tileSize = new Vector2(gridTiles[0].GetTileSize(), gridTiles[0].GetTileSize());
                Vector2 objectSize = new Vector2(tilesPerObject, tilesPerObject) * tileSize;

                specificTypeTiles.Clear();
                int gridTilesCount = gridTiles.Count;
                for (int j = 0; j < gridTilesCount; j++)
                {
                    if (!gridTiles[i].GetOccupied() && gridTiles[i].GetTileType() == objectTypeToSpawn)
                    {
                        specificTypeTiles.Add(gridTiles[i]);
                    }
                }

                if (specificTypeTiles.Count > 0)
                {
                    specificTypeZones.Clear();
                    specificTypeZones.AddRange(CustomUtilities.GetTilesAsZone(specificTypeTiles, gridTiles[0].transform.localScale.x));

                    int objectsPerRoom = 0;
                    Rect objectRect = new Rect(Vector2.zero, objectSize);
                    for (int j = 0; j < specificTypeZones.Count; j++)
                    {
                        for (int k = 0; k < specificTypeZones[j].Count - 1; k++)
                        {
                            if (tileSize == objectSize)
                            {
                                specificTypeZones[j][k].PlaceObject(objectsToSpawn[Random.Range(0, objectsToSpawn.Length)], roomsToSpawnObjectsIn[i].gameObject.transform);
                                objectsPerRoom++;
                                if (objectsPerRoom >= maxObjectsPerRoom)
                                    break;
                                continue;
                            }
                            Vector2 currentTilePos = specificTypeZones[j][k].GetPosition();
                            Rect tempRect = new Rect(Vector2.zero, tileSize)
                            {
                                center = currentTilePos
                            };
                            objectRect.position = tempRect.position;
                            int tilesThatFit = 1;
                            tilesToPlaceObjectOn.Add(specificTypeZones[j][k]);
                            tilesToPlaceObjectOn.Clear();
                            for (int l = k + 1; l < specificTypeZones[j].Count; l++)
                            {
                                if (specificTypeZones[j][l].GetPosition().IsWithinRect(objectRect))
                                {
                                    tilesThatFit++;
                                    tilesToPlaceObjectOn.Add(specificTypeZones[j][l]);
                                }

                                if (tilesThatFit == tilesPerObject)
                                {

                                    break;
                                }
                            }
                            specificTypeZones[j][k].PlaceObject(objectsToSpawn[Random.Range(0, objectsToSpawn.Length)], roomsToSpawnObjectsIn[i].gameObject.transform);
                            objectsPerRoom++;
                            if (objectsPerRoom >= maxObjectsPerRoom)
                                break;
                            // TODO: Segment spawning based on zones and whether they form large enough areas to spawn objects that require it
                        }
                        if (objectsPerRoom >= maxObjectsPerRoom)
                            break;
                    }
                }
            }
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
        currentRoom.UpdateRoomStencil(null, 0, 2000);
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
        activeThroughPortals.Clear();
        for (int i = 0; i < currentRoom.GetPortalsInRoom().Count; i++)
        {
            Portal p = currentRoom.GetPortalsInRoom()[i];
            if (p.GetConnectedPortal() != currentPortal)
            {
                CustomUtilities.UpdatePortalAndItsConnectedRoom(p, stencilValue, 0, 2000, currentRoomMask, true);
            }
            else
            {
                // Previous room
                previousRoom.UpdateRoomStencil(currentPortal.transform, stencilValue, 2300);
                CustomUtilities.UpdateStencils(currentPortal.gameObject, null, stencilValue, 2100);
                CustomUtilities.UpdateStencils(currentPortal.GetConnectedPortal().gameObject, null, stencilValue, 2100);
            }
            
            portalsInConnectedRoom.AddRange(p.GetConnectedRoom().GetPortalsInRoom());
            for (int j = 0; j < portalsInConnectedRoom.Count; j++)
            {
                if (portalsInConnectedRoom[j].GetConnectedPortal() != p)
                {
                    CustomUtilities.UpdatePortalAndItsConnectedRoom(portalsInConnectedRoom[j] , otherRoomStencilValue, stencilValue, 2300, otherRoomMask, true);
                    otherRoomStencilValue++;
                    if (otherRoomStencilValue == 8 || otherRoomStencilValue == 16 || otherRoomStencilValue == 32 || otherRoomStencilValue == 64 || otherRoomStencilValue == 128) // Must never be equal to any iteration of stencilValue
                    {
                        otherRoomStencilValue++;
                    }
                }
            }
            portalsInConnectedRoom.Clear();
            //* ThroughPortal culling based on view
            activeThroughPortals.Add(new List<Portal>());
            for (int j = 0; j < p.GetConnectedRoom().GetPortalsInRoom().Count; j++)
            {
                if (p.GetConnectedRoom().GetPortalsInRoom()[j].GetConnectedPortal() != p)
                {
                    activeThroughPortals[i].Add(p.GetConnectedRoom().GetPortalsInRoom()[j]);
                }
            }
            
            int activeThroughPortalsCount = activeThroughPortals[i].Count;
            for (int j = activeThroughPortalsCount - 1; j >= 0; j--)
            {
                Vector3 dir = (activeThroughPortals[i][j].transform.position - p.transform.position).normalized;
                if (math.dot(dir, p.transform.forward) < 0 || math.dot(dir, activeThroughPortals[i][j].transform.forward) < 0)
                {
                    activeThroughPortals[i][j].gameObject.SetActive(false);
                    activeThroughPortals[i].RemoveAt(j);
                }
            }
            //*/

            stencilValue += stencilValue; // BitShift 1 to the left
        }
        #endregion
    }
    
    /// <summary>
    /// Step 2/2 in switching world with portals. Should occur when player exits the portal.
    /// Enables the corresponding portal in the new room, and disables all portals in the previous room, in addition to updating their shader matrices.
    /// </summary>
    /// <param name="portal"></param>
    public void FinalizeRoomSwitch(Portal portal)
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
