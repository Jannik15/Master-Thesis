using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainStart : MonoBehaviour
{
    [SerializeField] private Button naturalWalkingBtn, steeringBtn;
    [SerializeField] private GameObject normalMenu, TestMenu, naturalWalkingMenu, steeringMenu;

    void Awake()
    {
        if (PlayerPrefs.GetInt("CompletedTutorial", 0) == 0)
        {
            SceneManager.LoadScene("Tutorial");
        }
    }
    void Start()
    {
        if (PlayerPrefs.GetInt("MovementType", 0) == 0) // 0 = Natural Walking, 1 = Steering
        {
            StartCoroutine(DisableSteering());
        }
        else
        {
            steeringBtn.onClick.Invoke();
        }

        if (PlayerPrefs.GetInt("Testing", 0) == 1)
        {
            normalMenu.SetActive(false);
            TestMenu.SetActive(true);
            if (PlayerPrefs.GetInt("MovementType", 0) == 0)
            {
                naturalWalkingMenu.SetActive(true);
            }
            else
            {
                steeringMenu.SetActive(true);
            }
        }
        else
        {
            normalMenu.SetActive(true);
        }
    }

    IEnumerator DisableSteering()
    {
        yield return new WaitForSeconds(1);
        naturalWalkingBtn.onClick.Invoke();
    }
}
