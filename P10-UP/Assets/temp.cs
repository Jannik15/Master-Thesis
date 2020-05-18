using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class temp : MonoBehaviour
{
    void Update()
    {
        Image text = GetComponent<Image>();

        if (text != null)
        {
            text.material.SetInt("_Stencil", 0);
            text.material.SetInt("_StencilComp", 8);
            text.material.renderQueue = 3000;
        }
    }

}
