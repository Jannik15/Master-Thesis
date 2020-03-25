using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ceiling
{
    private Vector2 center;
    private float height;
    private float size;

    public Ceiling(Vector2 center, float height, float size)
    {
        this.center = center;
        this.height = height;
        this.size = size;
    }

    public void SetHeight(float height)
    {
        this.height = height;
    }
    public float GetHeight()
    {
        return height;
    }


    public void InstantiateCeiling(Transform parent, int xIndex, int yIndex)
    {
        if (height <= 0)
        {
            return;
        }
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);

        gameObject.name = "Ceiling [" + xIndex + ", " + yIndex + "]";
        gameObject.transform.position = new Vector3(center.x, height, center.y);
        gameObject.transform.eulerAngles = new Vector3(270.0f, 0.0f, 0.0f);
        gameObject.transform.localScale = new Vector3(size, size, size);
        gameObject.transform.parent = parent;
    }
}
