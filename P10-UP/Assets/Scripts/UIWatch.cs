using System.Collections;
using System.Collections.Generic;
using Oculus.Platform.Samples.VrHoops;
using TMPro;
using UnityEngine;
using UnityEngine.UI;




public class UIWatch : MonoBehaviour
{

    public int currentHP;
    public TMP_Text hpAmount;

    private PlayerInteractions playInt;

    void Start()
    {

        playInt = FindObjectOfType<PlayerInteractions>();
        if (playInt != null)
        {
            currentHP = (int) playInt.health;
        }
        hpAmount.text = currentHP.ToString();
    }

    public void healthUpdate(int amount)
    {
        currentHP -= amount;
        hpAmount.text = currentHP.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHP <= 0)
        {
            hpAmount.text = "Dead";
        }
    }
}
