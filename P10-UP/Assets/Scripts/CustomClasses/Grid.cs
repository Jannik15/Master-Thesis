using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private Tile[,] tiles;
    private List<List<Tile>> portalZones;
    [SerializeField] private int xTiles, yTiles;
    [SerializeField] private List<Tile> tileList;

    private void Awake()
    {
        if (tiles == null)
        {
            tiles = new Tile[xTiles, yTiles];
            int iterator = 0;
            for (int x = 0; x < xTiles; x++)
            {
                for (int y = 0; y < yTiles; y++)
                {
                    tiles[x, y] = tileList[iterator];
                    iterator++;
                }
            }
        }
    }

    public void Initialize(Tile[,] tiles)
    {
        this.tiles = tiles;
        xTiles = tiles.GetLength(0);
        yTiles = tiles.GetLength(1);
        tileList = new List<Tile>();
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                tileList.Add(tiles[x,y]);
            }
        }
    }

    public Tile[,] GetTilesAsArray()
    {
        return tiles;
    }
    public List<Tile> GetTilesAsList()
    {
        return tileList;
    }

    public Tile GetTile(int x, int y)
    {
        return tiles[x, y];
    }

    public Tile GetTile(int i)
    {
        return tileList[i];
    }

    public List<List<Tile>> GetPortalZones()
    {
        return portalZones;
    }

    public List<Tile> GetPortalZone(int index)
    {
        return portalZones[index];
    }

    public void SetPortalZones(List<List<Tile>> newPortalZones)
    {
        portalZones = newPortalZones;
    }
    public void SetPortalZone(List<Tile> newPortalZone, int index)
    {
        portalZones[index] = newPortalZone;
    }
}
