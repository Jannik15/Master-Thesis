using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScale : ListenerResponse
{
    Dictionary<GameObject, Animator> DicButtons = new Dictionary<GameObject, Animator>();

    public void ScaleUpButton()
    {
        Animator anim = GetButtonAnimator(listener.gameEvent.EVENTOBJ);
        anim.SetBool("ScaleUp", true);
    }

    public void ResetButtonScale()
    {
        GetButtonAnimator(listener.gameEvent.EVENTOBJ).SetBool("ScaleUp", false);
    }

    Animator GetButtonAnimator(GameObject obj)
    {
        if (!DicButtons.ContainsKey(obj))
        {
            Animator animator = obj.GetComponentInChildren<Animator>();
            DicButtons.Add(obj, animator);
            return animator;
        }
        else
        {
            return DicButtons[obj];
        }
    }
}
