﻿using System.Collections;
using System.Collections.Generic;
using Oculus.Platform.Samples.VrHoops;
using UnityEngine;

public class FadeAnimator : MonoBehaviour
{
    public PlayerInteractions playInt;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void DoneFading()
    {
        playInt.OnFadeComplete();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
