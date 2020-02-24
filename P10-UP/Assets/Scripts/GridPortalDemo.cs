using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPortalDemo : MonoBehaviour
{
    [SerializeField] private GameObject portalPrefab;
    public List<Portal> portals = new List<Portal>();
    public List<GameObject> rooms = new List<GameObject>();
    public int currentRoom;
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
        
        Instantiate(portalPrefab, portalTiles[Random.Range(0, portalTiles.Count)].transform.position, Quaternion.Euler(0,Random.Range(0,360),0), rooms[0].transform);


    }

    public void SwitchWorld()
    {

    }
}
