using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class LockWorldPosition : MonoBehaviour
{
    [SerializeField] private float y;

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= y)
        {
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
    }
}
