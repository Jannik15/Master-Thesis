using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool isWalkable, isOccupied;
    [SerializeField] TileGeneration.TileType type;
    [SerializeField] private Vector2 position;
    [SerializeField] private float cellSize;
    [SerializeField] private GameObject objectOnTile;
    private Rect rect;

    public Tile(Vector2 position, TileGeneration.TileType type, bool isWalkable, bool isOccupied) // Only used for between tile positions
    {
        this.position = position;
        this.type = type;
        this.isWalkable = isWalkable;
        this.isOccupied = isOccupied;
    }

    public void AssignAllValues(bool isWalkable, TileGeneration.TileType type)
    {
        this.isWalkable = isWalkable;
        this.type = type;
        position = new Vector2(transform.position.x, transform.position.z);
        cellSize = transform.lossyScale.x;
    }

    public TileGeneration.TileType GetTileType()
    {
        return type;
    }

    public Vector2 GetPosition()
    {
        return position;
    }
    public void SetPosition(Vector2 position)
    {
        this.position = position;
    }

    public bool GetWalkable()
    {
        return isWalkable;
    }
    public void SetWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }

    public GameObject PlaceObject(GameObject objectToPlace, Transform parent)
    {
        if (isOccupied)
        {
            Debug.Log("Tried placing object " + objectToPlace + " on tile " + gameObject.name + ", but it was occupied");
            return null;
        }
        objectOnTile = Instantiate(objectToPlace, position.ToVector3XZ(), Quaternion.identity, parent);
        isOccupied = true;
        return objectOnTile;
    }

    public void PlaceExistingObject(GameObject objectToPlace)
    {
        if (!isOccupied)
        {
            objectOnTile = objectToPlace;
            isOccupied = true;
        }
        else
        {
            Debug.Log("Tried placing object " + objectToPlace + " on tile " + gameObject.name + ", but it was occupied");
        }
    }

    public bool GetOccupied()
    {
        return isOccupied;
    }
    /// <summary>
    /// Should ONLY be used for temporary tiles - use PlaceExistingObject for permanent tiles to include a reference to the occupying object.
    /// </summary>
    /// <param name="isOccupied"></param>
    public void SetOccupied(bool isOccupied)
    {
        this.isOccupied = isOccupied;
    }

    public GameObject GetObjectOnTile()
    {
        return objectOnTile;
    }

    public float GetTileSize()
    {
        return cellSize;
    }

    public Rect GetTileAsRect()
    {
        Rect rect = new Rect(Vector2.zero, new Vector2(cellSize, cellSize))
        {
            center = position
        };
        return rect;
    }
}
