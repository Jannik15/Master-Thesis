using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    void OnTriggerEnter(Collider plate)
    {
        if (plate.CompareTag("Button"))
        {
            Debug.Log("Pressure plate entered");
        }
    }
    void OnTriggerExit(Collider plate)
    {
        if (plate.CompareTag("Button"))
        {
            Debug.Log("Pressure plate exited");
        }
    }
}
