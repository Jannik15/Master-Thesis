using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    Transform player;
    void Start()
    {
        player = Camera.main.transform;
    }

    void FixedUpdate()
    {
        transform.LookAt(new Vector3(player.position.x, 0.0f, player.position.z));
    }
}
