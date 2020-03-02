using System;
using System.Linq.Expressions;
using UnityEngine;

public class TileGeneration
{
    private float cellSize;
    private Vector2 center;
    private bool isWalkable;
    private Material material;
    private TileType tileType;

    public enum TileType {
        Path,
        Portal,
        Scenery,
        Event,
        Empty
    }

    public TileGeneration(float cellSize, Vector2 center)
    {
        this.cellSize = cellSize;
        this.center = center;
        tileType = TileType.Scenery;
        isWalkable = true;
    }

    public Vector2 GetCoordinates()
    {
        return center;
    }

    public TileType GetTileType()
    {
        return tileType;
    }

    public void SetTileType(TileType type)
    {
        tileType = type;
        if (type == TileType.Empty)
        {
            //material
        }
    }

    public void AssignMaterial(Material material)
    {
        this.material = material;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public void SetWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }

    public Texture GetMaterialTexture()
    {
        return material.mainTexture;
    }

    public Material GetMaterial()
    {
        return material;
    }

    public Tile InstantiateTile(Transform parent)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);

        gameObject.name = "Tile [" + center.x + "; " + center.y + "]";
        gameObject.transform.position = new Vector3(center.x, 0.0f, center.y);
        gameObject.transform.eulerAngles = new Vector3(90.0f,0.0f,0.0f);
        gameObject.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
        gameObject.transform.parent = parent;

        if (tileType == TileType.Empty)
        {
            gameObject.name = "Empty";
            MonoBehaviour.DestroyImmediate(gameObject.GetComponent<MeshRenderer>());
            MonoBehaviour.DestroyImmediate(gameObject.GetComponent<MeshFilter>());
            MonoBehaviour.DestroyImmediate(gameObject.GetComponent<MeshCollider>());

        }
        else if (material != null)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.material = material;
        }

        gameObject.AddComponent<Tile>();
        Tile objectTile = gameObject.GetComponent<Tile>();
        objectTile.AssignAllValues(isWalkable, tileType);
        return objectTile;
    }
}
