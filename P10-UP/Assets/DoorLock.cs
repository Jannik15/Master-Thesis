using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLock : MonoBehaviour
{
    public bool isLocked, beenUnlocked = false;

   
    public GameObject buttonTrigger;

    private Vector3 originalPosition;

    private float minDistance, maxDistance;
    // Start is called before the first frame update
    void Awake()
    {
        minDistance = Vector3.Distance(buttonTrigger.transform.position, transform.position);

        maxDistance = buttonTrigger.transform.position.y;

        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(buttonTrigger.transform.position, transform.position) >= minDistance)
        {
            transform.position = originalPosition;
        }

        if (transform.position.y <= maxDistance)
        {
            transform.position = new Vector3(transform.position.x, maxDistance, transform.position.z);
        }
    }

}
