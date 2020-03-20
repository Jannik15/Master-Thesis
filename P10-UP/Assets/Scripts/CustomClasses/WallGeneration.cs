using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WallGeneration
{
    private Vector2 position;
    private Vector3 scale;
    private float height;
    private Material material;

    public WallGeneration(Vector2 position, Vector3 scale)
    {
        this.position = position;
        this.scale = scale;
    }

    public void SetHeight(float height)
    {
        scale = new Vector3(scale.x, height, scale.z);
    }

    public void InstantiateWall(Transform parent, int xIndex, int yIndex)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

        gameObject.name = "Wall [" + xIndex + ", " + yIndex + "]";
        gameObject.transform.position = new Vector3(position.x, scale.y / 2.0f, position.y);
        gameObject.transform.localScale = scale;
        gameObject.transform.parent = parent;

        if (material != null)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.material = material;
        }
    }
}
