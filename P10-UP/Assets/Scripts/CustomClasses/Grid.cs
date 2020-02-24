using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private TileGeneration[,] tiles;
    private int gridSizeX, gridSizeY;
    private float cellSize;

    public Grid(int gridSizeX, int gridSizeY, float cellSize)
    {
        this.gridSizeX = gridSizeX;
        this.gridSizeY = gridSizeY;
        this.cellSize = cellSize;
        tiles = new TileGeneration[gridSizeX, gridSizeY];

        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            float x = cellSize / 2.0f - gridSizeX / 2.0f * cellSize + i * cellSize;
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                float y = cellSize / 2.0f - gridSizeY / 2.0f * cellSize + j * cellSize;
                tiles[i, j] = new TileGeneration(cellSize, new Vector2(x, y));
            }
        }
    }

    public GameObject GetGrid(string gridName)
    {
        GameObject gridObject = new GameObject(gridName);
        foreach (TileGeneration tile in tiles)
        {
            tile.InstantiateTile(gridObject.transform);
        }
        return gridObject;
    }

    public void InstantiateGrid()
    {
        GameObject gridObject = new GameObject("Grid");
        foreach (TileGeneration tile in tiles)
        {
            tile.InstantiateTile(gridObject.transform);
        }
    }

    public TileGeneration GetTile(int x, int y)
    {
        return tiles[x, y];
    }

    public TileGeneration[,] GetAllTiles()
    {
        return tiles;
    }
}
