using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool isWalkable, isOccupied;
    [SerializeField] TileGeneration.TileType type;
    [SerializeField] private Vector2 position;
    [SerializeField] private float cellSize;
    [SerializeField] private Material material;
    [SerializeField] private GameObject objectOnTile;

    public void AssignAllValues(bool isWalkable, TileGeneration.TileType type)
    {
        this.isWalkable = isWalkable;
        this.type = type;
        position = new Vector2(transform.position.x, transform.position.z);
        cellSize = transform.lossyScale.x;
        if (GetComponent<Renderer>() != null)
            material = GetComponent<Renderer>().sharedMaterial;
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

    public bool PlaceObject(GameObject objectToPlace)
    {
        if (isOccupied)
        {
            Debug.Log("Tried placing object " + objectToPlace + " on tile " + gameObject.name + ", but it was occupied");
            return false;
        }
        Instantiate(objectToPlace, position.ToVector3XZ(), Quaternion.identity, transform.parent);
        objectOnTile = objectToPlace;
        isOccupied = true;
        return true;
    }

    public bool GetOccupied()
    {
        return isOccupied;
    }

    public GameObject GetObjectOnTile()
    {
        return objectOnTile;
    }
}
