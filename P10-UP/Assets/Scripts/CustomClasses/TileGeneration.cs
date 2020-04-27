using System;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

public class TileGeneration
{
    public enum TileType
    {
        Path,
        Portal,
        Scenery,
        Event,
        Enemy,
        Empty
    }

    private float cellSize;
    private Vector2 center;
    private bool isWalkable;
    private Material material;
    private GameObject tilePrefab;
    private TileType tileType;
    private Ceiling ceiling;

    public TileGeneration(float cellSize, Vector2 center, Ceiling ceiling)
    {
        this.cellSize = cellSize;
        this.center = center;
        this.ceiling = ceiling;
        tileType = TileType.Path;
        isWalkable = true;
    }

    public Vector2 GetCoordinates()
    {
        return center;
    }

    public Ceiling GetCeiling()
    {
        return ceiling;
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
            this.material = Resources.Load<Material>("EmptyTile");
            isWalkable = false;
        }
    }

    public void AssignTilePrefabOverride(GameObject tilePrefab)
    {
        this.tilePrefab = tilePrefab;
        if (tilePrefab != null)
        {
            material = tilePrefab.GetComponentInChildren<Renderer>().sharedMaterial;
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
#if UNITY_EDITOR
        if (tilePrefab != null)
        {
            return AssetPreview.GetAssetPreview(tilePrefab);
        }
#endif
        return material.mainTexture;
    }

    public Material GetMaterial()
    {
        return material;
    }

    public Tile InstantiateTile(Transform parent, int xIndex, int yIndex)
    {
        GameObject gameObject;
        if (tilePrefab == null)
        {
             gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
             gameObject.transform.position = new Vector3(center.x, 0.0f, center.y);
             gameObject.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
             gameObject.transform.parent = parent;
        }
        else
        {
            gameObject = MonoBehaviour.Instantiate(tilePrefab, new Vector3(center.x, 0.0f, center.y), Quaternion.identity, parent);
        }
        gameObject.name = "Tile [" + xIndex + ", " + yIndex + "]";
        gameObject.transform.localScale = new Vector3(cellSize, cellSize, cellSize);


        if (tileType == TileType.Empty)
        {
            gameObject.name = "Empty";
            Component[] allComponents = gameObject.GetComponentsInChildren(typeof(Component));
            for (int i = 0; i < allComponents.Length; i++)
            {
                if (allComponents[i].GetType() != typeof(Transform))
                {
                    MonoBehaviour.DestroyImmediate(allComponents[i]);
                }
            }
        }
        else if (material != null && tilePrefab == null)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.material = material;
        }

        if (ceiling.GetHeight() > 0)
        {
            ceiling.InstantiateCeiling(gameObject.transform, xIndex, yIndex);
        }

        gameObject.AddComponent<Tile>();
        Tile objectTile = gameObject.GetComponent<Tile>();
        objectTile.AssignAllValues(isWalkable, tileType);
        return objectTile;
    }
}
