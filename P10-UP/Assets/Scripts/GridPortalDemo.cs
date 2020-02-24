using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;
using UnityEngine;

public class GridPortalDemo : MonoBehaviour
{
    [SerializeField] private GameObject portalPrefab;
    public List<Portal> portals = new List<Portal>();
    public List<GameObject> rooms = new List<GameObject>();
    public GameObject currentRoom; // Functions as the index in rooms, tracking which room the player is in
    private List<Tile> roomTiles = new List<Tile>();
    private List<Tile> portalTiles = new List<Tile>();
    private Vector2 portalPlacement;

    void Start()
    {
        GameObject[] roomPrefabs = Resources.LoadAll<GameObject>("Grids/");
        for (int i = 0; i < roomPrefabs.Length; i++)
        {
            rooms.Add(Instantiate(roomPrefabs[i]));
            CustomUtilities.UpdateStencils(rooms[i], i);
        }
        roomTiles.AddRange(rooms[0].GetComponentsInChildren<Tile>());
        for (int j = 0; j < roomTiles.Count; j++)
        {
            if (roomTiles[j].GeTileType() == TileGeneration.TileType.Portal)
            {
                portalTiles.Add(roomTiles[j]);
            }
        }
        
        portals.Add(new Portal(Instantiate(portalPrefab, portalTiles[Random.Range(0, portalTiles.Count)].transform.position, Quaternion.Euler(0,Random.Range(0,360),0)),rooms[1]));


    }

    /// <summary>
    /// Update the current rooms stencil value to the new rooms, then set the new rooms stencil value to 0 and set the current room to the new room.
    /// </summary>
    /// <param name="portal"></param>
    public void SwitchWorld(Portal portal)
    {
        CustomUtilities.UpdateStencils(currentRoom, CustomUtilities.GetStencil(portal.GetConnectedRoom()));
        CustomUtilities.UpdateStencils(portal.GetConnectedRoom(), 0);
        currentRoom = portal.GetConnectedRoom();
    }
}
