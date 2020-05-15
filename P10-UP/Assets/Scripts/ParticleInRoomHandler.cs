﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleInRoomHandler : MonoBehaviour
{
    public GameObject particleParent;
    private ProceduralLayoutGeneration layout;
    private Room inRoom;
    void Start()
    {
        layout = FindObjectOfType<ProceduralLayoutGeneration>();

        layout.roomSwitched += OnRoomSwitch;
        inRoom = CustomUtilities.FindParentRoom(GetComponentInParent<Grid>().gameObject, layout.rooms);
    }

    void OnRoomSwitch(Room newCurrentRoom, Portal p)
    {
        if (inRoom == newCurrentRoom)
        {
            particleParent.SetActive(true);
            Renderer[] particleRenderers = particleParent.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < particleRenderers.Length; i++)
            {
                for (int j = 0; j < particleRenderers[i].materials.Length; j++)
                {
                    particleRenderers[i].materials[j].renderQueue = 3000;
                    particleRenderers[i].materials[j].SetInt("_StencilValue", 0);
                }
            }
        }
        else
        {
            particleParent.SetActive(false);
        }
    }

    void OnDestroy()
    {
        layout.roomSwitched -= OnRoomSwitch;
    }
}
