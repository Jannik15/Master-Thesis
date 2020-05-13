using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseGameVersion : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.GetInt("TestComplete", 0) == 1)
        {
            SceneManager.LoadScene("Main");
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
