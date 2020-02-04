using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultRotation : MonoBehaviour
{
    void Awake()
    {
        transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
    }
}
