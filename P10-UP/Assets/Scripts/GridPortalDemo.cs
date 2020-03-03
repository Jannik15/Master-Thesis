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
    public List<Portal> portals = new List<Portal>();
    public List<GameObject> rooms = new List<GameObject>();
    public List<GameObject> grids;
    public GameObject currentRoom, previousRoom; // Functions as the index in rooms, tracking which room the player is in
    private List<Tile> roomTiles = new List<Tile>(), otherRoomTiles = new List<Tile>();
    private List<Tile> portalTiles = new List<Tile>(), otherPortalTiles = new List<Tile>();
    private List<Vector2> possiblePortalPositions = new List<Vector2>();
    private int roomIterator, portalIterator;

    private void Start() // To uncomment a demo block, simply put a '/' in front of the '/*'
    {
        /* Demo 1
        GameObject[] roomPrefabs = Resources.LoadAll<GameObject>("Grids/");
        ProcedurallyLinearlyGenerateRooms(roomPrefabs);
        currentRoom = rooms[0];
        //*/

        //* Demo 2
        ProcedurallyGenerateRooms(grids);
        //*/
    }

    private void ProcedurallyLinearlyGenerateRooms(GameObject[] allRooms)
    {
        for (int i = 0; i < allRooms.Length; i++)
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
                    portalComponent.AssignValues(currentRoom, oppositePortal, portalIterator);
                    oppositePortalComponent.AssignValues(previousRoom,portal, portalIterator + 1);
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
        // TODO: Add grids randomly only add grids if they have portal tiles corresponding to previous grid, then place portals and do the connection.
        // TODO: Look at defining portal zones. If 3+ Zones, try creating another path, try diverging in the generation
        int gridLength = grids.Count;
        for (int i = 0; i < gridLength; i++)
        {
            int index = Random.Range(0, grids.Count);
            Instantiate(grids[index]);

            
            
            Grid grid = grids[index].GetComponent<Grid>();
            Tile[,] gridTiles = grid.GetTilesAsArray();
            
            
            
            for (int x = 0; x < gridTiles.GetLength(0); x++)
            {
                for (int y = 0; y < gridTiles.GetLength(1); y++)
                {
                    if (gridTiles[x, y].GetTileType() == TileGeneration.TileType.Portal)
                    {
                        CheckSurroundingTiles(gridTiles, x, y);
                    }
                }
            }
            grids.RemoveAt(index);
        }
    }

    private List<List<Tile>> PortalZones(List<Tile> remainingPortals)
    {
        List<List<Tile>> portalZones = new List<List<Tile>>();
        float tileSize = remainingPortals[0].transform.localScale.x;

        portalZones[0] = new List<Tile>();
        portalZones[0].Add(remainingPortals[0]);
        remainingPortals.RemoveAt(0);

        List<int> connections = new List<int>();
        while (remainingPortals.Count > 0)
        {
            bool portalAssignedToZone = false;
            for (int j = 0; j < portalZones.Count; j++)
            {
                for (int k = 0; k < portalZones[j].Count; k++)
                {
                    if (Vector2.Distance(remainingPortals[0].GetPosition(), portalZones[j][k].GetPosition()) <= tileSize)
                    {
                        portalZones[j].Add(remainingPortals[0]);
                        remainingPortals.RemoveAt(0);
                        portalAssignedToZone = true;
                        connections.Add(j);
                    }
                }
            }

            if (!portalAssignedToZone)
            {
                connections = connections.Distinct().ToList();
                if (connections.Count > 1)
                {   
                    connections.Sort();
                    for (int i = 1; i < connections.Count ; i++)
                    {
                        portalZones[connections[0]].AddRange(portalZones[connections[i]]);
                        portalZones[connections[i]] = null;
                    }
                }
                else if (connections.Count == 0)
                {
                    portalZones[portalZones.Count] = new List<Tile>();
                    portalZones[portalZones.Count].Add(remainingPortals[0]);
                    remainingPortals.RemoveAt(0);
                }
            }
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
    }

    public void UndoSwitchWorld()
    {
        CustomUtilities.UpdateStencils(currentRoom, CustomUtilities.GetStencil(previousRoom));
        CustomUtilities.UpdateStencils(previousRoom, 0);
        currentRoom = previousRoom;
    }
}
