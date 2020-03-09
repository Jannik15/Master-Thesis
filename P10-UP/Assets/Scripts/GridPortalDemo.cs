using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Platform;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class GridPortalDemo : MonoBehaviour
{
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private GameObject depthClearer;
    [SerializeField] private int roomAmount = 10;
    public List<Portal> portals = new List<Portal>();
    public List<Room> rooms = new List<Room>();
    public List<GameObject> grids;
    public GameObject currentRoomObject, previousRoomObject; // Functions as the index in rooms, tracking which room the player is in
    public Room currentRoom, previousRoom;
    private Transform portalParent;
    private List<Tile> roomTiles = new List<Tile>(), otherRoomTiles = new List<Tile>();
    private List<Tile> portalTiles = new List<Tile>(), otherPortalTiles = new List<Tile>();
    private List<Vector2> possiblePortalPositions = new List<Vector2>();
    private int roomId, portalIterator;
    public GameObject[] debugObjects;

    private void Start() // To uncomment a demo block, simply put a '/' in front of the '/*'
    {
        //* Demo 1
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

        /* Debuggers
        for (int i = 0; i < rooms.Count; i++)
        {
            Debug.Log("Room #" + i + " | Name: " + rooms[i].room.name + " | ID: " + rooms[i].GetRoomId() + " | Portals in room: " + rooms[i].GetPortalsInRoom().Count + " | Portals to room: " + rooms[i].GetPortalsToRoom().Count);
            for (int j = 0; j < rooms[i].GetPortalsInRoom().Count; j++)
            {
                Debug.Log("- PortalInRoom #" + j + " | Name: " + rooms[i].GetPortalsInRoom()[j].name + " | In room: " + rooms[i].GetPortalsInRoom()[j].GetRoom().room.name + " | Looking at: " + rooms[i].GetPortalsInRoom()[j].GetConnectedRoom().room.name + " | Fwd Stencil value: " + rooms[i].GetPortalsInRoom()[j].GetForwardStencilValue() + " | Bwd Stencil value: " + rooms[i].GetPortalsInRoom()[j].GetBackwardStencilValue());
            }
            for (int j = 0; j < rooms[i].GetPortalsToRoom().Count; j++)
            {
                Debug.Log("- PortalToRoom #" + j + " | Name: " + rooms[i].GetPortalsInRoom()[j].name + " | In room: " + rooms[i].GetPortalsInRoom()[j].GetRoom().room.name + " | Looking at: " + rooms[i].GetPortalsInRoom()[j].GetConnectedRoom().room.name + " | Fwd Stencil value: " + rooms[i].GetPortalsInRoom()[j].GetForwardStencilValue() + " | Bwd Stencil value: " + rooms[i].GetPortalsInRoom()[j].GetBackwardStencilValue());
            }
            Debug.Log(" ---------- Room analysis done ----------");
        }
        //*/
    }
    
    private void ProcedurallyGenerateRooms(List<GameObject> grids)
    {
        // TODO: Add banning of tiles - convert to portal zones and dont let the portal zones be used by the next grid for placement
        // TODO: Stop portals from spawning too close to grid edge (if they do, they should be turned such that the edge is perpendicular to them)
        // TODO: If 3+ Zones, try creating another path, try diverging in the generation

        portalParent = new GameObject("Portals").transform;

        List<Tile> gridTiles = new List<Tile>();
        List<Vector2> portalTilesLocations = new List<Vector2>();
        List<List<Vector2>> portalZones, previousPortalZones = new List<List<Vector2>>();

        while (rooms.Count < roomAmount)
        {
            int index = Random.Range(0, grids.Count);
            currentRoomObject = Instantiate(grids[index]);
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
            portalZones = PortalZones(portalTilesLocations, gridTiles[0].transform.localScale.x);
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
                    roomId++;     // Room iterator = current room, room iterator - 1 = previous room
                    rooms.Add(new Room(currentRoomObject, roomId + 1));

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

                    // Set stencil values for rooms and portals, and update shader matrix with the new portal locations
                    CustomUtilities.UpdateStencils(currentRoomObject, roomId + 1, true);
                    CustomUtilities.UpdateStencils(previousRoomObject, roomId, true);
                    //CustomUtilities.UpdateStencils(portal, roomId + 1);
                    //CustomUtilities.UpdateStencils(oppositePortal, roomId);
                    possiblePortalPositions.Clear();
                }
                else // If false, no portal tiles were overlapping - undo generation and try another room
                {
                    Destroy(currentRoomObject);
                    currentRoomObject = previousRoomObject;
                    continue;
                }
            }
            else
            {
                rooms.Add(new Room(currentRoomObject, roomId + 1)); // TODO: Change this to 1, so we can just use ID as stencil value, then keep 0 for current?
            }

            for (int i = 0; i < previousPortalZones.Count; i++) // TODO: Might be unnecessary
            {
                previousPortalZones[i].Clear();
            }
            previousPortalZones.Clear();
            previousPortalZones.AddRange(portalZones);
            previousRoomObject = currentRoomObject;
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
        currentRoomObject = currentRoom.room;
    }

    private List<List<Vector2>> PortalZones(List<Vector2> remainingPortals, float tileSize)
    {
        List<List<Vector2>> portalZones = new List<List<Vector2>>();
        portalZones.Add(new List<Vector2>());
        portalZones[0].Add(remainingPortals[0]);
        remainingPortals.RemoveAt(0);

        List<int> connections = new List<int>();
        for (int i = 0; i < remainingPortals.Count; i++)
        {
            for (int j = 0; j < portalZones.Count; j++)
            {
                for (int k = 0; k < portalZones[j].Count; k++)
                {
                    if (Vector2.Distance(remainingPortals[i], portalZones[j][k]) <= tileSize) // TODO: Convert to Distancesq <= tileSize * tileSize and test
                    {
                        connections.Add(j);
                    }
                }
            }

            if (connections.Count > 0)
            {
                connections = connections.Distinct().ToList();
                if (connections.Count > 1)
                {
                    connections.Sort();
                    portalZones[connections[0]].Add(remainingPortals[i]);
                    for (int j = 1; j < connections.Count; j++)
                    {
                        portalZones[connections[0]].AddRange(portalZones[connections[j]]);
                        portalZones.RemoveAt(connections[j]);
                    }
                }
                else // (connections.Count == 1)
                {
                    portalZones[connections[0]].Add(remainingPortals[i]);
                }
            }
            else //(connections.Count == 0)
            {
                portalZones.Add(new List<Vector2>());
                portalZones[portalZones.Count - 1].Add(remainingPortals[i]);
            }
            connections.Clear();
        }
        return portalZones;
    }

    /// <summary>
    /// [DEPRECATED] Returns a list of surrounding tiles to the input tiles (horizontally and vertically)
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    /// <returns></returns>
    private List<Tile> CheckSurroundingTiles(Tile[,] tiles, int xIndex, int yIndex)
    {
        List<Tile> surroundingTiles = new List<Tile>();
        List<Tile> portalTiles = new List<Tile> {tiles[xIndex, yIndex]};
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

    /// <summary>
    /// Update the current rooms stencil value to the new rooms, then set the new rooms stencil value to 0 and set the current room to the new room.
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
        // Step 3: Update shader matrix for previous room with the current portals transform (backward stencils)
        CustomUtilities.UpdateShaderMatrix(previousRoom.room, portal.transform);
    }

    public void FinalizeWorldSwitch(Portal portal) // Successful world switch
    {
        // Step 1: Enable the connected portal and disable all portals in the previous room
        Portal connectedPortal = portal.GetConnectedPortal();
        connectedPortal.SetActive(true);
        for (int i = 0; i < previousRoom.GetPortalsInRoom().Count; i++)
        {
            previousRoom.GetPortalsInRoom()[i].SetActive(false);
        }

        // Step 2: Update shader matrix for previous room with the connected portals transform
        CustomUtilities.UpdateShaderMatrix(previousRoom.room, connectedPortal.transform);
    }

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
}
