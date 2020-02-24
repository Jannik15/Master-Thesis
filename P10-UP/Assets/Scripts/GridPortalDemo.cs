using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPortalDemo : MonoBehaviour
{
    [SerializeField] private GameObject portalPrefab;
    public List<GameObject> portals = new List<GameObject>();
    public List<GameObject> rooms = new List<GameObject>();
    public int currentRoom; // Functions as the index in rooms, tracking which room the player is in
    private List<Tile> roomTiles = new List<Tile>();
    private List<Tile> portalTiles = new List<Tile>();
    private Vector2 portalPlacement;

    void Start()
    {
        GameObject[] roomPrefabs = Resources.LoadAll<GameObject>("Grids/");
        for (int i = 0; i < roomPrefabs.Length; i++)
        {
            rooms.Add(Instantiate(roomPrefabs[i]));
            CustomUtilities.UpdateRoomStencil(rooms[i], i);
        }
        roomTiles.AddRange(rooms[0].GetComponentsInChildren<Tile>());
        for (int j = 0; j < roomTiles.Count; j++)
        {
            if (roomTiles[j].GeTileType() == TileGeneration.TileType.Portal)
            {
                portalTiles.Add(roomTiles[j]);
            }
        }
        
        portals.Add(Instantiate(portalPrefab, portalTiles[Random.Range(0, portalTiles.Count)].transform.position, Quaternion.Euler(0,Random.Range(0,360),0)));


    }

    public void SwitchWorld(Portal portal)
    {
        CustomUtilities.UpdateRoomStencil(rooms[currentRoom], 0);    // Previous room to current rooms stencil value
        //CustomUtilities.UpdateRoomStencil(, 0);    // Current rooms stencil value to 0
    }
}
