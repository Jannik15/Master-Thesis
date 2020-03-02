using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private Tile[,] tiles;
    [SerializeField] private int xTiles, yTiles;
    [SerializeField] private List<Tile> tileList;

    private void Awake()
    {
        if (tiles == null)
        {
            tiles = new Tile[xTiles,yTiles];
            int iterator = 0;
            for (int x = 0; x < xTiles; x++)
            {
                for (int y = 0; y < yTiles; y++)
                {
                    tiles[x, y] = tileList[iterator];
                    Debug.Log(tiles[x,y].GetPosition());
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
}
