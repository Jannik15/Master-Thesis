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
    [SerializeField] private int roomAmount = 10;
    public List<Portal> portals = new List<Portal>();
    public List<Room> rooms = new List<Room>();
    public List<GameObject> grids;
    public GameObject currentRoom, previousRoom; // Functions as the index in rooms, tracking which room the player is in
    private Transform portalParent;
    private List<Tile> roomTiles = new List<Tile>(), otherRoomTiles = new List<Tile>();
    private List<Tile> portalTiles = new List<Tile>(), otherPortalTiles = new List<Tile>();
    private List<Vector2> possiblePortalPositions = new List<Vector2>();
    private int roomId, portalIterator;

    private void Start() // To uncomment a demo block, simply put a '/' in front of the '/*'
    {
        //* Demo 1
        ProcedurallyGenerateRooms(grids);
        //*/
        CustomUtilities.UpdateStencils(rooms[0].room,0);
        CustomUtilities.UpdateShaderMatrix(rooms[1].room, rooms[1].GetPortalsToRoom()[0].transform);
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
            currentRoom = Instantiate(grids[index]);
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
                    rooms.Add(new Room(currentRoom, roomId + 1));
                    float randomRotation = Random.Range(0, 360);
                    Vector3 randomPosition = possiblePortalPositions[Random.Range(0, possiblePortalPositions.Count)].ToVector3XZ();

                    // Create portals connecting the two rooms
                    GameObject portal = Instantiate(portalPrefab, randomPosition, Quaternion.Euler(0, randomRotation, 0), portalParent);
                    GameObject oppositePortal = Instantiate(portalPrefab, randomPosition, Quaternion.Euler(0, randomRotation - 180, 0), portalParent);
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
                    CustomUtilities.UpdateStencils(currentRoom, roomId + 1);
                    CustomUtilities.UpdateStencils(previousRoom, roomId);
                    CustomUtilities.UpdateStencils(portal, roomId + 1);
                    CustomUtilities.UpdateStencils(oppositePortal, roomId);
                    possiblePortalPositions.Clear();
                }
                else // If false, no portal tiles were overlapping - undo generation and try another room
                {
                    Destroy(currentRoom);
                    currentRoom = previousRoom;
                    continue;
                }
            }
            else
            {
                rooms.Add(new Room(currentRoom, roomId + 1)); // TODO: Change this to 1, so we can just use ID as stencil value, then keep 0 for current?
            }

            for (int i = 0; i < previousPortalZones.Count; i++)
            {
                previousPortalZones[i].Clear();
            }
            previousPortalZones.Clear();
            previousPortalZones.AddRange(portalZones);
            previousRoom = currentRoom;
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
        roomId = 0;
        currentRoom = rooms[roomId].room;
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
                    Vector2 remainingPortalsPos = remainingPortals[i];
                    Vector2 portalZonePos = portalZones[j][k];
                    float distance = Vector2.Distance(remainingPortalsPos, portalZonePos);
                    //if (Vector2.Distance(remainingPortals[i], portalZones[j][k]) <= tileSize)
                    if (distance <= tileSize)
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
        roomId = portal.GetConnectedRoom().GetRoomId();
        currentRoom = rooms[roomId].room;

        Debug.Log("World switch registered. Current room id is: " + roomId + " with name " + currentRoom.name + " == " + rooms[roomId].room.name);
        Debug.Log("Previous room name is " + previousRoom.name + " with a");
        CustomUtilities.UpdateStencils(previousRoom, CustomUtilities.GetStencil(rooms[roomId].room));
        CustomUtilities.UpdateShaderMatrix(portal.GetRoom().room, portal.transform);
        CustomUtilities.UpdateStencils(rooms[roomId].room, 0);
        for (int i = 0; i < rooms[roomId].GetPortalsInRoom().Count; i++)
        {
            if (rooms[roomId].GetPortalsInRoom()[i] != portal.GetConnectedPortal())
            {
                rooms[roomId].GetPortalsInRoom()[i].SetActive(true);
            }
        }
    }

    public void FinalizeWorldSwitch(Portal portal)
    {
        portal.SetActive(false);
        Portal connectedPortal = portal.GetConnectedPortal();
        connectedPortal.SetActive(true);
        CustomUtilities.UpdateShaderMatrix(connectedPortal.GetConnectedRoom().room, connectedPortal.transform);
    }

    public void UndoSwitchWorld(Portal portal)
    {
        CustomUtilities.UpdateStencils(currentRoom, CustomUtilities.GetStencil(previousRoom));
        CustomUtilities.UpdateStencils(previousRoom, 0);
        currentRoom = previousRoom;

        for (int i = 0; i < rooms[roomId].GetPortalsInRoom().Count; i++)
        {
            rooms[roomId].GetPortalsInRoom()[i].SetActive(false);
        }
        roomId = portal.GetRoom().GetRoomId();
    }
}
