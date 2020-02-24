using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool isWalkable;
    [SerializeField] TileGeneration.TileType type;
    [SerializeField] private Vector2 position;
    [SerializeField] private float cellSize;
    [SerializeField] private Material material;

    public void AssignAllValues(bool isWalkable, TileGeneration.TileType type)
    {
        this.isWalkable = isWalkable;
        this.type = type;
        position = new Vector2(transform.position.x, transform.position.z);
        cellSize = transform.lossyScale.x;
        material = GetComponent<Renderer>().sharedMaterial;
    }

    public TileGeneration.TileType GeTileType()
    {
        return type;
    }
}
