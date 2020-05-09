using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralLayoutGeneration : MonoBehaviour
{
    // Inspector variables
    public GameObject mainMenuCanvas;
    public int roomAmount = 10;
    [SerializeField] private GameObject[] startGrids, genericGrids, endGrids;
    [SerializeField] private GameObject[] smallSceneryObjects, mediumSceneryObjects, largeSceneryObjects, eventObjects, enemyObjects, endGameEventObjects;
    [SerializeField] private GameObject[] interactableObjects;
    [SerializeField] private GameObject portalPrefab, portalDoorPrefab, depthClearer, keyCard;
    [SerializeField] private Shader currentRoomMask, otherRoomMask;

    // Public non-inspector variables
    [HideInInspector] public List<GameObject> keysList;
    [HideInInspector] public Room currentRoom, previousRoom; // Room cannot currently be displayed in the inspector, requires a CustomInspectorDrawer implementation.
    [HideInInspector] public List<Room> rooms = new List<Room>(), genericRooms = new List<Room>(), eventRooms = new List<Room>();
    [HideInInspector] public List<List<Portal>> activeThroughPortals = new List<List<Portal>>();

    // Private variables
    private List<Vector2> possiblePortalPositions = new List<Vector2>();
    private List<Portal> portals = new List<Portal>();
    private List<DoorLock> portalDoors = new List<DoorLock>();
    private GameObject roomObject, keyCardToSpawn; // Functions as the index in rooms, tracking which room the player is in
    private int roomID, portalIterator, keycardIterator;
    private Transform portalParent;
    private List<Tile> gridTiles = new List<Tile>(), previousGridTiles = new List<Tile>(), walkableTiles = new List<Tile>(), doorEventTiles = new List<Tile>();
    private List<Tile> specificTypeTiles = new List<Tile>(), tilesToSpawnObjectOn = new List<Tile>(), tilesToSpawnObjectOnFlipped = new List<Tile>();
    private List<Vector2> portalTilePositions = new List<Vector2>();
    private List<List<Tile>> specificTypeZones = new List<List<Tile>>();
    private List<List<Vector2>> previousPortalZones = new List<List<Vector2>>();
    private List<List<List<Vector2>>> portalZones = new List<List<List<Vector2>>>();
    public event Action proceduralGenerationFinished; 
    private int keyCardID;
    private int differentRoomLayer, defaultLayer, interactionLayer, currentPortalLayer, differentPortalLayer;
    public event Action<Room, Portal> roomSwitched;
    public event Action<Portal> disabledPortal;
    private Transform playerCam;
    private PlayerCollisionHandler playerCollisionHandler;

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

    private void Awake()
    {
        defaultLayer = LayerMask.NameToLayer("Default");
        interactionLayer = LayerMask.NameToLayer("Interactable");
        differentRoomLayer = LayerMask.NameToLayer("DifferentRoom");
        currentPortalLayer = LayerMask.NameToLayer("CurrentRoomPortal");
        differentPortalLayer = LayerMask.NameToLayer("DifferentRoomPortal");
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
        playerCollisionHandler = FindObjectOfType<PlayerCollisionHandler>();
    }

    private void FixedUpdate()
    {
        if (mainMenuCanvas.activeSelf && Input.GetKeyDown(KeyCode.Y)) // PC Debug only
        {
            mainMenuCanvas.SetActive(false);
            ProcedurallyGenerateRooms();
            SwitchCurrentRoom();
        }
        //*/ Portal culling based on view cone and portal to portal direction
        for (int i = 0; i < activeThroughPortals.Count; i++)
        {
            bool isPortalVisible = math.dot((currentRoom.GetPortalsInRoom()[i].transform.position - playerCam.position).normalized, 
                                       currentRoom.GetPortalsInRoom()[i].transform.forward) >= 0;
            if (!playerCollisionHandler.inPortal)
            {
                if (isPortalVisible && !currentRoom.GetPortalsInRoom()[i].gameObject.activeSelf)
                {
                    currentRoom.GetPortalsInRoom()[i].SetActive(true);
                }
                else if (!isPortalVisible && currentRoom.GetPortalsInRoom()[i].gameObject.activeSelf)
                {
                    currentRoom.GetPortalsInRoom()[i].SetActive(false);
                    disabledPortal?.Invoke(currentRoom.GetPortalsInRoom()[i]);
                }
            }

            for (int j = 0; j < activeThroughPortals[i].Count; j++)
            {
                bool isThroughPortalVisible = math.dot((activeThroughPortals[i][j].transform.position - playerCam.position).normalized,
                                                  activeThroughPortals[i][j].transform.forward) >= 0;
                if (isThroughPortalVisible && currentRoom.GetPortalsInRoom()[i].gameObject.activeSelf)
                {
                    if (!activeThroughPortals[i][j].gameObject.activeSelf)
                    {
                        activeThroughPortals[i][j].gameObject.SetActive(true);
                    }
                }
                else if (activeThroughPortals[i][j].gameObject.activeSelf)
                {
                    activeThroughPortals[i][j].gameObject.SetActive(false);
                    disabledPortal?.Invoke(activeThroughPortals[i][j]);
                }
            }
        }
        //*/
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

        CreateRooms(1, startGrids, CustomRoomType.Start);
    }

    public void SwitchCurrentRoom()
    {
        SwitchCurrentRoom(rooms[0], null);
    }

    public void ProcedurallyGenerateRooms()
    {
        // TODO: If 3+ Zones, try creating another path, try diverging in the generation

        portalParent = new GameObject("Portals").transform;

        CreateRooms(roomAmount - 2, genericGrids, CustomRoomType.Generic);
        CreateRooms(1, endGrids, CustomRoomType.End);

        // Handle portal doors events
        for (int i = 0; i < portalDoors.Count; i++)
        {
            if (!portalDoors[i].isLocked)
            {
                int randomEvent = Random.Range(0, 9);
                Room portalDoorRoom = portalDoors[i].inRoom;
                bool caseSelected = false;
                int doorRoomID = portalDoorRoom.GetRoomID();

                if (randomEvent < 3)
                {
                    // Case 1 - Unlock with pressure plate
                    int checkLastXRooms = doorRoomID > 2 ? 3 : doorRoomID;

                    // Randomize input
                    List<Room> roomSlice = new List<Room>(4);
                    for (int j = 0; j < checkLastXRooms; j++)
                    {
                        EventObjectBase eventInRoom = rooms[doorRoomID - j].gameObject.GetComponentInChildren<EventObjectBase>(true);
                        if (!eventInRoom || eventInRoom.eventType.thisEventType != EventObjectType.ThisType.PressurePlate)
                        {
                            roomSlice.Add(rooms[doorRoomID - j]);
                        }
                    }
                    roomSlice.Randomize();

                    checkLastXRooms = roomSlice.Count;
                    for (int j = 0; j < checkLastXRooms; j++)
                    {
                        Grid doorRoomGrid = roomSlice[j].gameObject.GetComponent<Grid>();
                        doorEventTiles.Clear();
                        doorEventTiles.AddRange(doorRoomGrid.GetTilesAsList());
                        doorEventTiles.Randomize();
                        int tileCount = doorEventTiles.Count;
                        for (int k = tileCount - 1; k >= 0; k--)
                        {
                            if (!doorEventTiles[k].GetOccupied() && doorEventTiles[k].GetWalkable() && doorEventTiles[k].GetTileType() == TileGeneration.TileType.Event)
                            {
                                caseSelected = true;
                                List<Tile> spawner = new List<Tile>(1); // Very inefficient, should change later (SpawnObjectOnTile should be able to take just a single gameObject)
                                spawner.Add(doorEventTiles[k]);
                                GameObject pressurePlate = SpawnObjectOnTile(spawner, false, eventObjects[0], TileGeneration.TileType.Event, roomSlice[j], false);
                                // Pair door and pressure plate
                                portalDoors[i].RemoveButton();
                                portalDoors[i].Pair(DoorLock.DoorEvent.PressurePlate, pressurePlate);
                                break;
                            }
                        }
                        if (caseSelected)
                            break;
                    }
                    if (caseSelected)
                        continue;
                }

                // Case 2 - Unlock by shooting a target
                if (randomEvent >= 6)
                {
                    // Remove button since target should control the door lock
                    portalDoors[i].RemoveButton();

                    // Instantiate target and place it at the top front of the door
                    GameObject shootTarget = Instantiate(eventObjects[1], Vector3.zero, Quaternion.identity);
                    shootTarget.transform.parent = portalDoors[i].transform.parent;
                    shootTarget.transform.position = portalDoors[i].transform.position;
                    shootTarget.transform.localPosition = new Vector3(shootTarget.transform.localPosition.x, 1, -0.13f);
                    shootTarget.transform.eulerAngles = portalDoors[i].transform.rotation.eulerAngles;
                    shootTarget.transform.eulerAngles += new Vector3(0, 90, 0);

                    // Pair and assign room
                    portalDoors[i].Pair(DoorLock.DoorEvent.ShootTarget, shootTarget);
                    EventObjectBase shootTargetEvent = shootTarget.GetComponentInChildren<EventObjectBase>(true);
                    shootTargetEvent.AssignRoom(portalDoors[i].inRoom, false);
                    caseSelected = true;
                }
                if (caseSelected)
                    continue;

                // Case 3 - Unlock with keycard
                if (keysList.Count < rooms.Count)
                {
                    int roomToSpawnKeyCardIn = Random.Range(math.clamp(doorRoomID - 5, 1, doorRoomID), doorRoomID);
                    doorEventTiles.Clear();
                    doorEventTiles.AddRange(rooms[roomToSpawnKeyCardIn].gameObject.GetComponent<Grid>().GetTilesAsList());
                    int doorEventTilesCount = doorEventTiles.Count;
                    for (int j = doorEventTilesCount - 1; j >= 0; j--)
                    {
                        if (!doorEventTiles[j].GetWalkable() || doorEventTiles[j].GetOccupied())
                        {
                            doorEventTiles.RemoveAt(j);
                        }
                    }
                    int keyCardTileIndex = Random.Range(0, doorEventTiles.Count);
                    keyCardToSpawn = doorEventTiles[keyCardTileIndex].PlaceObject(keyCard, rooms[roomToSpawnKeyCardIn].gameObject.transform);
                    rooms[roomToSpawnKeyCardIn].AddObjectToRoom(keyCardToSpawn.transform, false);
                    keyCardToSpawn.GetComponentInChildren<InteractableObject>().inRoom = rooms[roomToSpawnKeyCardIn];
                    keysList.Add(keyCardToSpawn);
                    portalDoors[i].Pair(DoorLock.DoorEvent.KeyCard, keyCardToSpawn);
                    Color randomKeycardColor = Random.ColorHSV(0f, 0.9f, 0.5f, 1f, 0.8f, 1f, 1f, 1f);
                    Renderer[] keyCardRenders = keyCardToSpawn.GetComponentsInChildren<Renderer>(true);
                    for (int j = 0; j < keyCardRenders.Length; j++)
                    {
                        for (int k = 0; k < keyCardRenders[j].materials.Length; k++)
                        {
                            keyCardRenders[j].materials[k].SetColor("_MainColor", randomKeycardColor);
                        }
                    }
                    // Do the same for the door
                    Renderer[] keyCardScannerRenders = portalDoors[i].keycardScanner.GetComponentsInChildren<Renderer>(true);
                    for (int j = 0; j < keyCardScannerRenders.Length; j++)
                    {
                        for (int k = 0; k < keyCardScannerRenders[j].materials.Length; k++)
                        {
                            keyCardScannerRenders[j].materials[k].SetColor("_Emission", randomKeycardColor);
                        }
                    }
                }
                // Case 4 - Already unlocked (Do nothing)
            }
        }

        // Spawn objects
        //SpawnObjectType(genericRooms, TileGeneration.TileType.Event, eventObjects, null, 1, new Vector2(1, 1), false);
        SpawnObjectType(genericRooms, TileGeneration.TileType.Enemy, enemyObjects, null, Random.Range(2,6), new Vector2Int(1, 1), true);
        SpawnObjectType(genericRooms, TileGeneration.TileType.Scenery, largeSceneryObjects, null, 1, new Vector2Int(2, 2), true);
        SpawnObjectType(genericRooms, TileGeneration.TileType.Scenery, mediumSceneryObjects, null, 2, new Vector2Int(2, 1), true);
        SpawnObjectType(genericRooms, TileGeneration.TileType.Scenery, smallSceneryObjects, null, 2, new Vector2Int(1, 1), true);

        for (int j = 0; j < portals.Count; j++)
        {
            if (j > 0)
            {
                portals[j].SetActive(false);
            }
        }
        for (int i = 1; i < roomID + 1; i++)
        {
            rooms[i].SetLayer(differentRoomLayer, differentRoomLayer);
        }
        portals[0].SetLayer(currentPortalLayer);

        roomID = 0;
        currentRoom = rooms[roomID];
        proceduralGenerationFinished?.Invoke();
    }

    private void CreateRooms(int roomCount, GameObject[] gridInputList, CustomRoomType roomType)
    {
        // Iterates over the amount of rooms specified in the constructor
        int roomIterator = 0;
        int iterationCap = 0;
        while (roomIterator < roomCount && iterationCap < 1000)
        {
            iterationCap++;
            // Takes a random grid from the list of input grids, and stores the grid component
            int newIndex = Random.Range(0, gridInputList.Length);
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
                    rooms.Add(new Room(gridObject, roomID, grid));
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

                    List<Room> endRooms = new List<Room>(1);
                    endRooms.Add(rooms[roomID]);
                    SpawnObjectType(endRooms, TileGeneration.TileType.Event, endGameEventObjects, null, 1, new Vector2Int(2, 2), true);
                    SpawnObjectType(endRooms, TileGeneration.TileType.Event, endGameEventObjects, null, 1, new Vector2Int(1, 2), true);
                    SpawnObjectType(endRooms, TileGeneration.TileType.Event, endGameEventObjects, null, 1, new Vector2Int(1, 1), true);
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
                        genericRooms.Add(rooms[roomID]);
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
                        eventRooms.Add(rooms[roomID]);
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
            roomID++; // The id of the room, corresponding to the index in the list of rooms
            rooms.Add(new Room(gridObject, roomID, grid)); // Add the room object as a Room with a unique ID and a grid

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
            //*/
            #endregion
            
            //*// Choose what type of portal should spawn - Old door code
            int spawnDoor = Random.Range(0, 10);
            if (spawnDoor < 8 && roomID > 1)
            {
                portal = Instantiate(portalDoorPrefab, portalPosition.ToVector3XZ(), Quaternion.Euler(0, randomRotation, 0), portalParent);
                DoorLock portalDoor = portal.GetComponentInChildren<DoorLock>(true);
                portalDoor.inRoom = rooms[roomID - 1];
                portalDoor.lockEvent = DoorLock.DoorEvent.Unlocked;
                portalDoors.Add(portalDoor);
            }
            else
            {
                portal = Instantiate(portalPrefab, portalPosition.ToVector3XZ(), Quaternion.Euler(0, randomRotation, 0), portalParent);
            }
            //*/
            GameObject oppositePortal = Instantiate(portalPrefab, portalPosition.ToVector3XZ(), Quaternion.Euler(0, randomRotation - 180, 0), portalParent);
            portal.name = portal.name + "_" + portalIterator;
            oppositePortal.name = oppositePortal.name + "_" + (portalIterator + 1);
            rooms[roomID].gameObject.transform.parent = portal.transform;

            // Occupy tiles for both portals
            previousGridTiles.Clear();
            previousGridTiles.AddRange(rooms[roomID - 1].roomGrid.GetTilesAsList());
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

            Portal portalComponent = portal.AddComponent<Portal>();
            Portal oppositePortalComponent = oppositePortal.AddComponent<Portal>();
            portalComponent.AssignValues(rooms[roomID - 1], rooms[roomID], oppositePortalComponent, portalIterator);
            oppositePortalComponent.AssignValues(rooms[roomID], rooms[roomID - 1], portalComponent, portalIterator + 1);
            rooms[roomID].AddPortalInRoom(oppositePortalComponent);
            rooms[roomID].AddPortalToRoom(portalComponent);
            rooms[roomID - 1].AddPortalInRoom(portalComponent);
            rooms[roomID - 1].AddPortalToRoom(oppositePortalComponent);
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
                    rooms[roomID].AddObjectToRoom(oppositePortal.transform.GetChild(i), true);
                }
            }
            for (int i = 0; i < portal.transform.childCount; i++)
            {
                if (portal.transform.GetChild(i).CompareTag("PortalObjects"))
                {
                    rooms[roomID - 1].AddObjectToRoom(portal.transform.GetChild(i), true);
                }
            }

            // Set layer for room and portal
            portalComponent.SetLayer(differentPortalLayer);
            oppositePortalComponent.SetLayer(differentPortalLayer);

            if (rooms.Count > 3)
            {
                rooms[roomID].gameObject.SetActive(false);
            }
        }
        return possiblePortalPositions.Count > 0; // return whether or not portal tiles in current room are overlapping with portal tiles in previous rooms
    }

    #region Object spawning
    private void SpawnObjectType(List<Room> roomsToSpawnObjectsIn, TileGeneration.TileType objectTypeToSpawn, GameObject[] objectsToSpawn, GameObject specificObjectToSpawn, 
        int maxObjectsPerRoom, Vector2Int tilesPerObject, bool canCollideWithPlayer)
    {
        for (int i = 0; i < roomsToSpawnObjectsIn.Count; i++)
        {
            GameObject objectToSpawn;
            if (specificObjectToSpawn != null)
            {
                objectToSpawn = specificObjectToSpawn;
            }
            else
            {
                if (objectsToSpawn.Length == 0)
                    return;
                objectToSpawn = objectsToSpawn[Random.Range(0, objectsToSpawn.Length)];
            }
            Grid grid = roomsToSpawnObjectsIn[i].roomGrid;
            gridTiles.Clear();
            gridTiles.AddRange(grid.GetTilesAsList());
            Vector2 tileSize = new Vector2(gridTiles[0].GetTileSize(), gridTiles[0].GetTileSize());
            Vector2 objectSize = tilesPerObject * tileSize;
            Vector2 objectSizeFlipped = new Vector2(tilesPerObject.y, tilesPerObject.x) * tileSize;

            specificTypeTiles.Clear();
            int gridTilesCount = gridTiles.Count;
            for (int j = 0; j < gridTilesCount; j++)
            {
                if (!gridTiles[j].GetOccupied() && gridTiles[j].GetTileType() == objectTypeToSpawn)
                {
                    specificTypeTiles.Add(gridTiles[j]);
                }
            }
            if (specificTypeTiles.Count > 0)
            {
                specificTypeZones.Clear();
                specificTypeZones.AddRange(CustomUtilities.GetTilesAsZone(specificTypeTiles, gridTiles[0].transform.localScale.x));

                int objectsPerRoom = 0;
                Rect objectRect = new Rect(Vector2.zero, objectSize);
                Rect objectRectFlipped = new Rect(Vector2.zero, objectSizeFlipped);
                for (int j = 0; j < specificTypeZones.Count; j++)
                {
                    specificTypeZones[j].Randomize();   // Vary the placement of multiple objects of the same type in a room, so they don't spawn all together
                    for (int k = 0; k < specificTypeZones[j].Count - 1; k++)
                    {
                        if (objectsPerRoom == maxObjectsPerRoom)
                            break;
                        Tile currentTile = specificTypeZones[j][k];
                        if (currentTile.GetOccupied())
                            continue;
                        if (tileSize == objectSize)
                        {
                            List<Tile> tileSlice = new List<Tile>();
                            tileSlice.Add(currentTile);
                            SpawnObjectOnTile(tileSlice, false, objectToSpawn, objectTypeToSpawn, roomsToSpawnObjectsIn[i], canCollideWithPlayer);
                            objectsPerRoom++;
                            continue;
                        }
                        Rect tempRect = new Rect(Vector2.zero, tileSize)
                        {
                            center = currentTile.GetPosition()
                        };
                        objectRect.position = tempRect.position;
                        objectRectFlipped.position = tempRect.position;
                        int tilesThatFit = 1, tilesThatFitFlipped = 1;
                        tilesToSpawnObjectOn.Clear();
                        tilesToSpawnObjectOnFlipped.Clear();
                        tilesToSpawnObjectOn.Add(currentTile);
                        tilesToSpawnObjectOnFlipped.Add(currentTile);
                        for (int l = k + 1; l < specificTypeZones[j].Count; l++)
                        {
                            Tile evaluatedTile = specificTypeZones[j][l];
                            if (evaluatedTile.GetPosition().IsWithinRect(objectRect) && !evaluatedTile.GetOccupied())
                            {
                                tilesThatFit++;
                                tilesToSpawnObjectOn.Add(evaluatedTile);
                            }
                            else if (evaluatedTile.GetPosition().IsWithinRect(objectRectFlipped) && !evaluatedTile.GetOccupied())
                            {
                                tilesThatFitFlipped++;
                                tilesToSpawnObjectOnFlipped.Add(evaluatedTile);
                            }

                            if (tilesThatFit == tilesPerObject.x * tilesPerObject.y)
                            {
                                SpawnObjectOnTile(tilesToSpawnObjectOn, false, objectToSpawn, objectTypeToSpawn, roomsToSpawnObjectsIn[i], canCollideWithPlayer);
                                objectsPerRoom++;
                                break;
                            }
                            if (tilesThatFitFlipped == tilesPerObject.x * tilesPerObject.y)
                            {
                                SpawnObjectOnTile(tilesToSpawnObjectOnFlipped, true, objectToSpawn, objectTypeToSpawn, roomsToSpawnObjectsIn[i], canCollideWithPlayer);
                                objectsPerRoom++;
                                break;
                            }
                        }
                    }
                    if (objectsPerRoom == maxObjectsPerRoom)
                        break;
                }
            }
        }
    }

    private GameObject SpawnObjectOnTile(List<Tile> tilesToSpawnObjectOn, bool flipped, GameObject objectToSpawn, TileGeneration.TileType objectType, Room roomToSpawnIn, bool playerCollision)
    {
        GameObject objectOnTile = Instantiate(objectToSpawn, Vector3.zero, objectToSpawn.transform.rotation, roomToSpawnIn.gameObject.transform);
        InteractableObject interactableObject = objectOnTile.GetComponentInChildren<InteractableObject>();
        if (interactableObject == null || !interactableObject.isInteractable)
        {
            roomToSpawnIn.AddObjectToRoom(objectOnTile.transform, playerCollision);
        }
        else
        {
            interactableObject.AssignRoom(roomToSpawnIn, true);
        }
        Vector2 spawnObjectCenter = Vector2.zero;
        int tileCount = tilesToSpawnObjectOn.Count;
        for (int m = 0; m < tileCount; m++)
        {
            tilesToSpawnObjectOn[m].PlaceExistingObject(objectOnTile);
            spawnObjectCenter += tilesToSpawnObjectOn[m].GetPosition();
        }
        spawnObjectCenter /= tileCount;
        if (flipped)
        {
            objectOnTile.transform.localEulerAngles += new Vector3(0, 90.0f, 0);
        }
        objectOnTile.transform.position = spawnObjectCenter.ToVector3XZ();

        switch (objectType)
        {
            case TileGeneration.TileType.Event:
                EventObjectBase eventBase = objectOnTile.GetComponentInChildren<EventObjectBase>(true);
                if (eventBase != null)
                {
                    eventBase.room = roomToSpawnIn;
                    if (eventBase.eventType.thisEventType == EventObjectType.ThisType.PressurePlate)
                    {
                        List<Tile> availableTiles = new List<Tile>();
                        availableTiles.AddRange(rooms[Random.Range(roomToSpawnIn.GetRoomID() - 1, roomToSpawnIn.GetRoomID() + 1)].roomGrid.GetTilesAsList());
                        int availableTilesCount = availableTiles.Count;
                        for (int i = availableTilesCount - 1; i >= 0; i--)
                        {
                            if (!availableTiles[i].GetOccupied() && availableTiles[i].GetWalkable())
                            {
                                Tile availableTile = availableTiles[i];
                                availableTiles.Clear();
                                availableTiles.Add(availableTile);
                                break;
                            }
                        }
                        GameObject randomInteractableOject = interactableObjects[Random.Range(0, interactableObjects.Length)];
                        SpawnObjectOnTile(availableTiles, false, randomInteractableOject, TileGeneration.TileType.Empty, roomToSpawnIn, false);
                    }
                }
                else
                {
                    Debug.LogError("Tried spawning an event in room " + roomToSpawnIn.gameObject.name + " but it did not contain an EventObjectBase");
                }
                break;
            case TileGeneration.TileType.Enemy:
                Enemy enemyScript = objectOnTile.GetComponentInChildren<Enemy>();
                enemyScript.AssignRoom(roomToSpawnIn, true);
                objectOnTile.transform.LookAt(Vector3.zero);
                break;
        }
        return objectOnTile;
    }
    #endregion

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
        currentRoom.gameObject.transform.parent = null;
        currentRoom.UpdateRoomStencil(null, 0, 2000);
        if (currentPortal != null)
        {
            previousRoom.gameObject.transform.parent = currentPortal.transform;
        }
        previousRoom.SetLayer(differentRoomLayer, differentRoomLayer);
        currentRoom.SetLayer(defaultLayer, interactionLayer);

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
                        disabledPortal?.Invoke(previousRoomConnection.GetPortalsInRoom()[j]);
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
                CustomUtilities.UpdateStencils(currentPortal.gameObject, null, stencilValue, 2100);
                CustomUtilities.UpdateStencils(currentPortal.GetConnectedPortal().gameObject, null, stencilValue, 2100);
                previousRoom.UpdateRoomStencil(currentPortal.transform, stencilValue, 2300);
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
                Portal pIndex = p.GetConnectedRoom().GetPortalsInRoom()[j];
                if (pIndex.gameObject.activeSelf && pIndex.GetConnectedPortal() != p)
                {
                    activeThroughPortals[i].Add(pIndex);
                }
            }
            
            int activeThroughPortalsCount = activeThroughPortals[i].Count;
            for (int j = activeThroughPortalsCount - 1; j >= 0; j--)
            {
                Vector3 dir = (activeThroughPortals[i][j].transform.position - p.transform.position).normalized;
                if (math.dot(dir, p.transform.forward) < 0 || math.dot(dir, activeThroughPortals[i][j].transform.forward) < 0)
                {
                    activeThroughPortals[i][j].SetActive(false);
                    disabledPortal?.Invoke(activeThroughPortals[i][j]);
                    activeThroughPortals[i].RemoveAt(j);
                }
            }
            //*/

            stencilValue += stencilValue; // BitShift 1 to the left
        }
        roomSwitched?.Invoke(currentRoom, currentPortal);
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
        portal.SetActive(false);
        disabledPortal?.Invoke(portal);
        portal.SetLayer(differentPortalLayer);
        connectedPortal.SetActive(true);
        for (int i = 0; i < previousRoom.GetPortalsInRoom().Count; i++)
        {
            previousRoom.GetPortalsInRoom()[i].SetLayer(differentPortalLayer);
        }
        for (int i = 0; i < currentRoom.GetPortalsInRoom().Count; i++)
        {
            currentRoom.GetPortalsInRoom()[i].SetLayer(currentPortalLayer);
        }

        // Step 2: Update shader matrix for previous room with the connected portals transform
        previousRoom.gameObject.transform.parent = connectedPortal.transform;
        CustomUtilities.UpdateShaderMatrix(previousRoom.gameObject, connectedPortal.transform);
        roomSwitched?.Invoke(currentRoom, null);
    }
    #endregion
}
