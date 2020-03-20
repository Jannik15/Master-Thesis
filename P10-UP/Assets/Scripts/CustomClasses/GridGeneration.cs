using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGeneration
{
    private TileGeneration[,] tiles;
    private WallGeneration[,] walls;

    public GridGeneration(int gridSizeX, int gridSizeY, float cellSize, float defaultWallHeight, float wallThickness)
    {
        tiles = new TileGeneration[gridSizeX, gridSizeY];
        walls = new WallGeneration[gridSizeX + 1,gridSizeY + 2];

        for (int yIndex = tiles.GetLength(0); yIndex > 0; yIndex--)
        {
            float y = cellSize / 2.0f - gridSizeY / 2.0f * cellSize + (yIndex - 1) * cellSize;
            for (int xIndex = 0; xIndex < tiles.GetLength(1); xIndex++)
            {
                float x = cellSize / 2.0f - gridSizeX / 2.0f * cellSize + xIndex * cellSize;
                tiles[xIndex, gridSizeY - yIndex] = new TileGeneration(cellSize, new Vector2(x, y));

                walls[xIndex, gridSizeY - yIndex] = new WallGeneration(new Vector2(x, y + cellSize / 2.0f), new Vector3(cellSize, defaultWallHeight, wallThickness));
                walls[xIndex, gridSizeY - yIndex + 1] = new WallGeneration(new Vector2(x - cellSize / 2.0f, y), new Vector3(wallThickness, defaultWallHeight, cellSize));
                if (xIndex == tiles.GetLength(1) - 1)
                {
                    walls[xIndex + 1, gridSizeY - yIndex + 1] = new WallGeneration(new Vector2(x + cellSize / 2.0f, y), new Vector3(wallThickness, defaultWallHeight, cellSize));
                }
                if (yIndex == 1)
                {
                    walls[xIndex, gridSizeY - yIndex + 2] = new WallGeneration(new Vector2(x, y - cellSize / 2.0f), new Vector3(cellSize, defaultWallHeight, wallThickness));
                }
            }
        }
    }

    public GameObject GetGrid(string gridName)
    {
        GameObject gridObject = new GameObject(gridName);
        Tile[,] tileArray = new Tile[tiles.GetLength(0),tiles.GetLength(1)];
        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                tileArray[x,y] = tiles[x, y].InstantiateTile(gridObject.transform, x, y);
            }
        }
        Grid grid = gridObject.AddComponent<Grid>();
        grid.Initialize(tileArray);
        return gridObject;
    }

    public void InstantiateGrid()
    {
        GameObject gridObject = new GameObject("Grid");
        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                tiles[x, y].InstantiateTile(gridObject.transform, x, y);
            }
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

    public WallGeneration[,] GetAllWalls()
    {
        return walls;
    }
}
