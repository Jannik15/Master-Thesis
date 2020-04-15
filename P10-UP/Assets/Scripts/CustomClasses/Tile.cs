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
