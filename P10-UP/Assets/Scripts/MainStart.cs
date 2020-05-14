using System.Collections;
using System.Collections.Generic;
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
            Debug.Log("Started Main scene with movement type of 0 = Natural Walking");
        }
        else
        {
            steeringBtn.onClick.Invoke();
            Debug.Log("Started Main scene with movement type of 1 = Steering");
        }
    }
}
