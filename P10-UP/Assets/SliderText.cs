using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
    private TMP_Text text;
    public GameObject handler;
    private ProceduralLayoutGeneration proLG;
    void Start()
    {
        text = GetComponent<TMP_Text>();
        proLG = handler.GetComponent<ProceduralLayoutGeneration>();
    }

    // Update is called once per frame
    public void textUpdate(float value)
    {
        text.text = value + " rooms";
        proLG.roomAmount = (int)value;
    }
}
