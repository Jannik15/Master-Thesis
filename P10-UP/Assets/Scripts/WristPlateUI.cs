using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristPlateUI : MonoBehaviour
{
    public GameObject DropZone;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Keycard"))
        {
            DropZone.SetActive(true);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Keycard"))
        {
            DropZone.SetActive(false);
        }
    }

}
