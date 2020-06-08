using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CenterButton : MonoBehaviour
{
    public GameObject nextButton, greyButton, nextButton2, greyButton2, slide, slide2;
    private int MovementType;
    public CharacterController steeringController;
    public OVRPlayerController ovrSteeringController;

    void Start()
    {
        if (steeringController == null)
        {
            //Debug.Log("Steering controller was null, attempting to find...");
            steeringController = FindObjectOfType<CharacterController>();
            ovrSteeringController = steeringController.GetComponent<OVRPlayerController>();
        }

        MovementType = PlayerPrefs.GetInt("MovementType");
        StartCoroutine(DisableSteering());
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
                nextButton2.SetActive(true);
                greyButton2.SetActive(false);
            }

        }
    }

    public void ToggleSteering(bool b)
    {
        steeringController.enabled = b;
        ovrSteeringController.enabled = b;
    }

    IEnumerator DisableSteering()
    {
        yield return new WaitForSeconds(1);

        steeringController.enabled = false;
        ovrSteeringController.enabled = false;
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
                nextButton2.SetActive(false);
                greyButton2.SetActive(true);
            }
        }
    }

    public void ExitTutorial()
    {
        PlayerPrefs.SetInt("CompletedTutorial", 1);
        SceneManager.LoadScene("Main");
    }
}
