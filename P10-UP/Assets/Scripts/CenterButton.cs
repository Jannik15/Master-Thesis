using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterButton : MonoBehaviour
{
    public GameObject nextButton, greyButton;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            nextButton.SetActive(true);
            greyButton.SetActive(false);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            nextButton.SetActive(false);
            greyButton.SetActive(true);
        }

    }

}
