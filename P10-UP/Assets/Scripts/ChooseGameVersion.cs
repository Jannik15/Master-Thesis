using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseGameVersion : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.GetInt("TestComplete", 0) == 1)
        {
            PlayerPrefs.SetInt("MovementType", 0); // 0 = Natural Walking, 1 = Steering
            SceneManager.LoadScene("Main");
        }
    }

    public void AcceptedTest()
    {
        PlayerPrefs.SetInt("Testing", 1);
        PlayerPrefs.SetInt("MovementType", Random.Range(0, 2)); // 0 = Natural Walking, 1 = Steering
        //Debug.Log("Random movement type was: " + PlayerPrefs.GetInt("MovementType"));
        SceneManager.LoadScene("TestStart");
    }

    public void DeclinedTest()
    {
        PlayerPrefs.SetInt("Testing", 0);
        PlayerPrefs.SetInt("MovementType", 0); // 0 = Natural Walking, 1 = Steering
        SceneManager.LoadScene("Main");
    }
}
