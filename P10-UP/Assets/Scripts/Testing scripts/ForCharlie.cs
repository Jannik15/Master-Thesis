using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForCharlie : MonoBehaviour
{
    public List<string> FlashCard = new List<string>();
    public List<string> Categories = new List<string>();

    void Start()
    {
        FlashCard.Add("1");
        FlashCard.Add("2");
        FlashCard.Add("1");
        Test();
    }

    void Test()
    {

        Debug.Log(FlashCard[0]);
        Debug.Log(FlashCard[1]);
        Debug.Log(FlashCard[2]);
        for (int i = 0; i < FlashCard.Count; i++)
        {
            Debug.Log("i is now = " + i);
            for (int j = 0; j <= Categories.Count; j++)
            {
                Debug.Log("j is now = " + j);
                Debug.Log("Categories.Count is now = " + Categories.Count);
                //Debug.Log(FlashCard[i] + " vs " + Categories[j]);

                Categories.Add(FlashCard[i]);
                Debug.Log("Added " + FlashCard[i] + " to list");
                
            }
        }
        Debug.Log(Categories[0]);
        Debug.Log(Categories[1]);
        Debug.Log(Categories[2]);

    }


    void Update()
    {

       

    }
}

