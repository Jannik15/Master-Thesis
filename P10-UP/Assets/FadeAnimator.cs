using System.Collections;
using System.Collections.Generic;
using Oculus.Platform.Samples.VrHoops;
using UnityEngine;

public class FadeAnimator : MonoBehaviour
{
    public PlayerInteractions playInt;

    public void WhiteFading()
    {
        playInt.OnGameComplete();
    }

    public void DoneFading()
    {
        //ToBlack
        playInt.OnFadeComplete();
    }
}
