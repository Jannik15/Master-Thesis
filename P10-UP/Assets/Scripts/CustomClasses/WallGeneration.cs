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

    public float GetHeight()
    {
        return scale.y;
    }

    public Vector2 GetPosition()
    {
        return position;
    }
    public void SetMaterial(Material material)
    {
        this.material = material;
    }

    public Material GetMaterial()
    {
        return material;
    }

    public void InstantiateWall(Transform parent, int xIndex, int yIndex)
    {
        if (scale.y <= 0) // Don't create a wall with no height
        {
            return;
        }
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

        gameObject.name = "Wall [" + xIndex + ", " + yIndex + "]";
        gameObject.transform.position = new Vector3(position.x, scale.y / 2.0f, position.y);
        gameObject.transform.localScale = scale;
        gameObject.transform.parent = parent;

        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (material != null)
        {
            Debug.Log("Setting material for " + gameObject.name + " to " + material.name);
            renderer.material = material;
        }
        else
        {
            Debug.Log("Setting material for " + gameObject.name + " to " + Resources.Load<Material>("Default").name);
            renderer.material = Resources.Load<Material>("Default");
        }
    }
}
