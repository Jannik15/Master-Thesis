using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialFinish : MonoBehaviour
{
    public void FinishedTutorial()
    {
        SceneManager.LoadScene("Main");
    }
}
