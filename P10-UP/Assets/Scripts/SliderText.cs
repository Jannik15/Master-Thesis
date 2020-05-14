using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
    private TMP_Text text, pressurePlateText, shootTargetText, keyCardText;
    private ProceduralLayoutGeneration proLG;
    public Slider pressurePlateSlider, shootTargetSlider, keyCardSlider;
    void Start()
    {
        text = GetComponent<TMP_Text>();
        proLG = FindObjectOfType<ProceduralLayoutGeneration>();

        if (pressurePlateSlider != null)
        {
            pressurePlateText = pressurePlateSlider.GetComponentInChildren<SliderText>().GetComponent<TMP_Text>();
            shootTargetText = shootTargetSlider.GetComponentInChildren<SliderText>().GetComponent<TMP_Text>();
            keyCardText = keyCardSlider.GetComponentInChildren<SliderText>().GetComponent<TMP_Text>();
        }
    }

    public void UpdateRoomAmount(float value)
    {
        text.text = value.ToString();
        proLG.roomAmount = (int)value;
    }
    public void UpdateDoorSpawnChance(float value)
    {
        text.text = value + "%";
        proLG.doorSpawnChance = (int)value;
    }

    public void UpdateDoorEventWeightings()
    {
        int totalWeight = (int)(pressurePlateSlider.value + shootTargetSlider.value + keyCardSlider.value);
        int pressurePlateWeighting = (int)(pressurePlateSlider.value / totalWeight * 100);
        int keyCardWeighting = (int)(keyCardSlider.value / totalWeight * 100);
        int shootTargetWeighting = (int)(shootTargetSlider.value / totalWeight * 100);

        pressurePlateText.text = pressurePlateWeighting + "%";
        keyCardText.text = keyCardWeighting + "%";
        shootTargetText.text = shootTargetWeighting + "%";

        // Pressure plate chance is evaluated first, then keyCards, then shootTarget. The prior chances have to be added on to get the correct result
        proLG.pressurePlateWeighting = pressurePlateWeighting;
        proLG.keyCardWeighting = keyCardWeighting + pressurePlateWeighting;
        proLG.shootTargetWeighting = shootTargetWeighting + pressurePlateWeighting + keyCardWeighting;
    }
}
