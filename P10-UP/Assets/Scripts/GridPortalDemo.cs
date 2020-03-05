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
    public List<GameObject> rooms = new List<GameObject>();
    public List<GameObject> grids;
    public GameObject currentRoom, previousRoom; // Functions as the index in rooms, tracking which room the player is in
    private Transform portalParent;
    private List<Tile> roomTiles = new List<Tile>(), otherRoomTiles = new List<Tile>();
    private List<Tile> portalTiles = new List<Tile>(), otherPortalTiles = new List<Tile>();
    private List<Vector2> possiblePortalPositions = new List<Vector2>();
    private int roomIterator, portalIterator;

    private void Start() // To uncomment a demo block, simply put a '/' in front of the '/*'
    {
        /* Demo 1
        ProcedurallyLinearlyGenerateRooms(grids);
        currentRoom = rooms[0];
        //*/

        //* Demo 2
        ProcedurallyGenerateRooms(grids);
        //*/
    }

    private void ProcedurallyLinearlyGenerateRooms(List<GameObject> allRooms)
    {
        for (int i = 0; i < allRooms.Count; i++)
        {
            // Create the rooms
            currentRoom = Instantiate(allRooms[i]);

            // Create the portals
            // - Step 1: Find all the portal tiles in Room n-1 and n if n > 0
            // - Step 2: Place two portals with opposite rotations on a portal tile that is overlapping in both rooms
            // - Step 3: Update shader matrix
            if (i > 0)
            {
                // Tile comparison
                if (roomTiles.Count == 0)
                {
                    roomTiles.AddRange(previousRoom.GetComponentsInChildren<Tile>());
                    for (int j = 0; j < roomTiles.Count; j++)
                    {
                        if (roomTiles[j].GetTileType() == TileGeneration.TileType.Portal)
                        {
                            portalTiles.Add(roomTiles[j]);
                        }
                    }
                }
                otherRoomTiles.AddRange(currentRoom.GetComponentsInChildren<Tile>());
                for (int j = 0; j < otherRoomTiles.Count; j++)
                {
                    if (otherRoomTiles[j].GetTileType() == TileGeneration.TileType.Portal)
                    {
                        otherPortalTiles.Add(otherRoomTiles[j]);
                    }
                }
                for (int j = 0; j < portalTiles.Count; j++)
                {
                    Vector2 portalTilePosition = portalTiles[j].GetPosition();
                    for (int k = 0; k < otherPortalTiles.Count; k++)
                    {
                        if (portalTilePosition == otherPortalTiles[k].GetPosition()) // tiles are overlapping
                        {
                            possiblePortalPositions.Add(portalTilePosition);
                            break;
                        }
                    }
                }

                if (possiblePortalPositions.Count > 0) // If false, no portal tiles were overlapping - undo generation and try another room
                {

                    float randomRotation = Random.Range(0, 360);
                    Vector3 randomPosition = possiblePortalPositions[Random.Range(0, possiblePortalPositions.Count)].ToVector3XZ(); // TODO: Avoid portal overlaps

                    GameObject portal = Instantiate(portalPrefab, randomPosition,Quaternion.Euler(0, randomRotation, 0));
                    GameObject oppositePortal = Instantiate(portalPrefab, randomPosition, Quaternion.Euler(0, randomRotation - 180, 0));
                    portal.AddComponent<Portal>();
                    oppositePortal.AddComponent<Portal>();
                    Portal portalComponent = portal.GetComponent<Portal>();
                    Portal oppositePortalComponent = oppositePortal.GetComponent<Portal>();
                    portalComponent.AssignValues(previousRoom, currentRoom, oppositePortal, portalIterator);
                    oppositePortalComponent.AssignValues(currentRoom, previousRoom,portal, portalIterator + 1);
                    portals.Add(portalComponent);
                    portals.Add(oppositePortalComponent);
                    portalIterator += 2;
                    possiblePortalPositions.Clear();

                    if (rooms.Count == 0)
                    {
                        CustomUtilities.UpdateStencils(previousRoom, roomIterator);
                        CustomUtilities.UpdateShaderMatrix(previousRoom, oppositePortalComponent.transform);
                        rooms.Add(previousRoom);
                        CustomUtilities.UpdateStencils(currentRoom, roomIterator + 1);
                        CustomUtilities.UpdateShaderMatrix(currentRoom, portal.transform);
                        rooms.Add(currentRoom);
                        roomIterator += 2;
                    }
                    else
                    {
                        CustomUtilities.UpdateStencils(currentRoom, roomIterator);
                        CustomUtilities.UpdateShaderMatrix(currentRoom, portal.transform);
                        rooms.Add(currentRoom);
                        roomIterator++;
                    }

                }
                else
                {
                    Destroy(currentRoom);
                    currentRoom = previousRoom;
                }

                // Clear data lists
                roomTiles.Clear(); // It is important to clear and add, instead of setting equal to the other list, otherwise the list will be a pointer
                portalTiles.Clear();
                roomTiles.AddRange(otherRoomTiles);
                portalTiles.AddRange(otherPortalTiles);

                otherRoomTiles.Clear();
                otherPortalTiles.Clear();
            }
            previousRoom = currentRoom;
        }

        for (int j = 0; j < portals.Count; j++)
        {
            if (j % 2 != 0)
            {
                portals[j].SetActive(false);
            }
        }
    }

    private void ProcedurallyGenerateRooms(List<GameObject> grids)
    {
        // TODO: Add banning of tiles - convert to portal zones and dont let the portal zones be used by the next grid for placement
        // TODO: Stop portals from spawning too close to grid edge (if they do, they should be turned such that the edge is perpendicular to them)
        // TODO: Add grids randomly only add grids if they have portal tiles corresponding to previous grid, then place portals and do the connection.
        // TODO: Look at defining portal zones. If 3+ Zones, try creating another path, try diverging in the generation

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
            // Create the portals
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

                if (possiblePortalPositions.Count > 0) // If false, no portal tiles were overlapping - undo generation and try another room
                {
                    float randomRotation = Random.Range(0, 360);
                    Vector3 randomPosition = possiblePortalPositions[Random.Range(0, possiblePortalPositions.Count)].ToVector3XZ(); // TODO: Avoid portal overlaps

                    GameObject portal = Instantiate(portalPrefab, randomPosition, Quaternion.Euler(0, randomRotation, 0), portalParent);
                    GameObject oppositePortal = Instantiate(portalPrefab, randomPosition, Quaternion.Euler(0, randomRotation - 180, 0), portalParent);
                    Portal portalComponent = portal.AddComponent<Portal>();
                    Portal oppositePortalComponent = oppositePortal.AddComponent<Portal>();
                    portalComponent.AssignValues(previousRoom,currentRoom, oppositePortal, portalIterator);
                    oppositePortalComponent.AssignValues(currentRoom, previousRoom, portal, portalIterator + 1);
                    portals.Add(portalComponent);
                    portals.Add(oppositePortalComponent);
                    portalIterator += 2;

                    CustomUtilities.UpdateStencils(previousRoom, roomIterator);
                    CustomUtilities.UpdateShaderMatrix(previousRoom, oppositePortalComponent.transform);
                    CustomUtilities.InstantiateMaterials(portal);
                    CustomUtilities.UpdateStencils(portal, roomIterator + 1);
                    roomIterator++;
                    CustomUtilities.UpdateStencils(currentRoom, roomIterator);
                    CustomUtilities.UpdateShaderMatrix(currentRoom, portal.transform);
                    CustomUtilities.InstantiateMaterials(oppositePortal);
                    CustomUtilities.UpdateStencils(oppositePortal, roomIterator - 1);
                    List<Portal> portalsInRoom = new List<Portal>();
                    portalsInRoom.Add(portalComponent);
                    rooms.Add(new Room(currentRoom, portalsInRoom, );
                    possiblePortalPositions.Clear();
                }
                else
                {
                    Destroy(currentRoom);
                    currentRoom = previousRoom;
                    //Debug.Log("No portal overlap found between " + currentRoom.name + " and " + previousRoom.name + ". Continuing...");
                    continue;
                }
            }
            else
            {
                rooms.Add(currentRoom);
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
        roomIterator = 0;
    }

    private List<List<Vector2>> PortalZones(List<Vector2> remainingPortals, float tileSize)
    {
        List<List<Vector2>> portalZones = new List<List<Vector2>>();
        portalZones.Add(new List<Vector2>());
        portalZones[0].Add(remainingPortals[0]);
        remainingPortals.RemoveAt(0);
        remainingPortals.Add(Vector3.zero);     // This is done due to a bug that crashes unity if the list becomes empty in the loop

        List<int> connections = new List<int>();
        int inputListSize = remainingPortals.Count;
        //for (int i = remainingPortals.Count - 1; i > 0; i--)
        //{
        //    remainingPortals.RemoveAt(i);
        //    //connections.Clear();
        //}
        for (int i = 0; i < remainingPortals.Count; i++)
        {
            for (int j = 0; j < portalZones.Count; j++)
            {
                for (int k = 0; k < portalZones[j].Count; k++)
                {
                    if (Vector2.Distance(remainingPortals[i], portalZones[j][k]) <= tileSize)
                    {
                        connections.Add(j);
                    }
                }
            }

            if (connections.Count > 1)
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
        CustomUtilities.UpdateStencils(currentRoom, CustomUtilities.GetStencil(portal.GetConnectedRoom()));
        CustomUtilities.UpdateShaderMatrix(currentRoom, portal.transform);
        CustomUtilities.UpdateStencils(portal.GetConnectedRoom(), 0);
        currentRoom = portal.GetConnectedRoom();

        portals[roomIterator].gameObject.SetActive(true);
    }

    public void UndoSwitchWorld()
    {
        CustomUtilities.UpdateStencils(currentRoom, CustomUtilities.GetStencil(previousRoom));
        CustomUtilities.UpdateStencils(previousRoom, 0);
        currentRoom = previousRoom;


        portals[roomIterator].gameObject.SetActive(true);
    }
}
