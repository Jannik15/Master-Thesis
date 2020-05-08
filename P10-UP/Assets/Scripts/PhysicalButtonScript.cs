using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicalButtonScript : MonoBehaviour
{

    public LayerMask layersToCollideWith;

    [Tooltip("Event to be called when we press the button with finger")]
    public UnityEvent buttonPressEvent;

    public static float continousButtonPressDelay = 0.5f;
    public static float timeAtLastButtonPress;


    public Material pressedMaterial;
    public Material releasedMaterial;

    float feedbackTimer = 0.5f;
    float activatedTimer;

    private void Start()
    {
        releasedMaterial = GetComponent<MeshRenderer>().material;
        //pressedMaterial = PlayerSingleton.Instance.pressedMaterial;
    }

    private void Update()
    {
        activatedTimer += Time.deltaTime;
        if (feedbackTimer < activatedTimer)
        {
            GetComponent<MeshRenderer>().material = releasedMaterial;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & layersToCollideWith) != 0)
        {
            if (timeAtLastButtonPress+ continousButtonPressDelay < Time.time)
            {
                buttonPressEvent.Invoke();
                timeAtLastButtonPress = Time.time;
                GetComponent<MeshRenderer>().material = pressedMaterial;
                activatedTimer = 0f;
            }
        }
    }

  

}
