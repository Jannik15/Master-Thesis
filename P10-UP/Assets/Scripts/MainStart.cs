using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainStart : MonoBehaviour
{
    [SerializeField] private Button naturalWalkingBtn, steeringBtn;
    [SerializeField] private GameObject normalMenu, TestMenu, naturalWalkingMenu, steeringMenu;
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
        Debug.Log("Started Main scene with movement type = " + PlayerPrefs.GetInt("MovementType", 0));

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
            Debug.Log("Started Main when Testing set to 1, disabling movement buttons...");
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
