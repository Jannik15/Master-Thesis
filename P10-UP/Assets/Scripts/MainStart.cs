using UnityEngine;
using UnityEngine.UI;

public class MainStart : MonoBehaviour
{
    [SerializeField] private Button naturalWalkingBtn, steeringBtn;
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
    }
}
