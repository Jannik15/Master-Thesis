using System.Collections;
using System.Collections.Generic;
using Oculus.Platform.Samples.VrHoops;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestEnd : MonoBehaviour
{
    [SerializeField] private GameObject naturalWalkingSSQ,
        naturalWalkingNextTestBtn,
        naturalWalkingNextTestText,
        naturalWalkingToLocomotionBtn,
        naturalWalkingToLocomotionText,

        steeringSSQ,
        steeringNextTestBtn,
        steeringNextTestText,
        steeringToLocomotionBtn,
        steeringToLocomotionText;
    void Start()
    {
        if (PlayerPrefs.GetInt("MovementType") == 0)
        {
            naturalWalkingSSQ.SetActive(true);
            if (PlayerPrefs.GetInt("ConditionsCompleted", 0) == 0)
            {
                naturalWalkingNextTestBtn.SetActive(true);
                naturalWalkingNextTestText.SetActive(true);
            }
            else
            {
                naturalWalkingToLocomotionBtn.SetActive(true);
                naturalWalkingToLocomotionText.SetActive(true);
            }
        }
        else
        {
            steeringSSQ.SetActive(true);
            if (PlayerPrefs.GetInt("ConditionsCompleted", 0) == 0)
            {
                steeringNextTestBtn.SetActive(true);
                steeringNextTestText.SetActive(true);
            }
            else
            {
                steeringToLocomotionBtn.SetActive(true);
                steeringToLocomotionText.SetActive(true);
            }
        }
    }

    public void GoToNextTest()
    {
        PlayerPrefs.SetInt("MovementType", 1 - PlayerPrefs.GetInt("MovementType"));
        PlayerPrefs.SetInt("CompletedCondition", 1);
        SceneManager.LoadScene("Main");
    }

    public void TestingCompleted()
    {
        PlayerPrefs.SetInt("Testing", 0);
        PlayerPrefs.SetInt("TestComplete", 1);
        SceneManager.LoadScene("Main");
    }
}
