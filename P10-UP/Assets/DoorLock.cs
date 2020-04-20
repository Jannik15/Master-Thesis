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

        maxDistance = buttonTrigger.transform.position.x;

        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(buttonTrigger.transform.position, transform.position) >= minDistance)
        {
            transform.position = originalPosition;
        }

        if (transform.position.x <= maxDistance)
        {
            transform.position = new Vector3(maxDistance, transform.position.y, transform.position.z);
        }

        Vector3 localVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
        localVelocity.y = 0;
        localVelocity.z = 0;

        GetComponent<Rigidbody>().velocity = transform.TransformDirection(localVelocity);


    }

}
