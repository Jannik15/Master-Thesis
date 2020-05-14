using UnityEngine;
using UnityEngine.UI;

public class MainStart : MonoBehaviour
{
    [SerializeField] private Button naturalWalkingBtn, steeringBtn;
    [SerializeField] private GameObject naturalWalkingText, naturalWalkingTextDisabled, steeringText, steeringTextDisabled;
    void Start()
    {
        if (PlayerPrefs.GetInt("MovementType", 0) == 0) // 0 = Natural Walking, 1 = Steering
        {
            naturalWalkingBtn.onClick.Invoke();
        }
        else
        {
            steeringBtn.onClick.Invoke();
        }
        Debug.Log("Started Main scene with movement type = " + PlayerPrefs.GetInt("MovementType", 0));

        if (PlayerPrefs.GetInt("Testing", 0) == 1)
        {
            naturalWalkingBtn.enabled = false;
            steeringBtn.enabled = false;
            naturalWalkingText.SetActive(false);
            naturalWalkingTextDisabled.SetActive(true);
            steeringText.SetActive(false);
            steeringTextDisabled.SetActive(true);
            Debug.Log("Started Main when Testing set to 1, disabling movement buttons...");
        }
    }
}
