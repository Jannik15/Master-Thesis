using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementHandler : MonoBehaviour
{
    public int MovementType = 0;
    public GameObject NextWalking, NextSteering;
    
    void Awake()
    {
        MovementType = PlayerPrefs.GetInt("MovementType");

        if (MovementType == 0) //Walking Selected
        {
            NextWalking.SetActive(true);
            NextSteering.SetActive(false);
        }
        else if (MovementType == 1) //Steering Selected
        {
            NextWalking.SetActive(false);
            NextSteering.SetActive(true);
        }
    }
}
