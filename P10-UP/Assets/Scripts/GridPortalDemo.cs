using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridPortalDemo : MonoBehaviour
{
    [SerializeField] private GameObject portalPrefab;
    public List<Portal> portals = new List<Portal>();
    public List<GameObject> rooms = new List<GameObject>();
    public GameObject currentRoom, previousRoom; // Functions as the index in rooms, tracking which room the player is in
    private List<Tile> roomTiles = new List<Tile>(), otherRoomTiles = new List<Tile>();
    private List<Tile> portalTiles = new List<Tile>(), otherPortalTiles = new List<Tile>();
    private List<Vector2> possiblePortalPositions = new List<Vector2>();
    private Vector2 portalPlacement;
    private int roomIterator, portalIterator;

    private void Start()
    {
        GameObject[] roomPrefabs = Resources.LoadAll<GameObject>("Grids/");
        ProcedurallyLinearlyGenerateRooms(roomPrefabs);
        currentRoom = rooms[0];
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
