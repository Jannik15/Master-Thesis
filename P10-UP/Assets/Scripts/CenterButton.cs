using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterButton : MonoBehaviour
{
    public GameObject nextButton, greyButton, nextButton2Walking, nextButton2Steering, greyButton2, slide, slide2;
    private int MovementType;

    void Start()
    {
        MovementType = PlayerPrefs.GetInt("MovementType");
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (slide.activeSelf)
            {
                nextButton.SetActive(true);
                greyButton.SetActive(false);
            }
            else if (slide2.activeSelf)
            {
                if (MovementType == 0)
                {
                    nextButton2Walking.SetActive(true);
                }
                else if (MovementType == 1)
                {
                    nextButton2Steering.SetActive(true);
                }
                greyButton2.SetActive(false);
            }

        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (slide.activeSelf)
            {
                nextButton.SetActive(false);
                greyButton.SetActive(true);
            }
            else if (slide2.activeSelf)
            {
                if (MovementType == 0)
                {
                    nextButton2Walking.SetActive(false);
                }
                else if (MovementType == 1)
                {
                    nextButton2Steering.SetActive(false);
                }
                greyButton2.SetActive(true);
            }
        }


    }

}
