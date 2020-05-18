using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextInRoomHandler : MonoBehaviour
{
    public GameObject textParent;
    private ProceduralLayoutGeneration layout;
    private Room inRoom;
    private Renderer[] renderers;
    private Text[] texts;
    private TextMeshProUGUI[] TMPTexts;
    private Image[] images;
    void Start()
    {
        renderers = textParent.GetComponentsInChildren<Renderer>(true);
        texts = textParent.GetComponentsInChildren<Text>(true);
        TMPTexts = textParent.GetComponentsInChildren<TextMeshProUGUI>(true);
        images = textParent.GetComponentsInChildren<Image>(true);
        //* Material instantiation
        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                renderers[i].materials[j] = Instantiate(renderers[i].materials[j]);
            }
        }
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].material = Instantiate(texts[i].material);
        }
        for (int i = 0; i < TMPTexts.Length; i++)
        {
            for (int j = 0; j < TMPTexts[i].fontMaterials.Length; j++)
            {
                TMPTexts[i].fontMaterials[j] = Instantiate(TMPTexts[i].fontMaterials[j]);
            }
        }
        for (int i = 0; i < images.Length; i++)
        {
            images[i].material = Instantiate(images[i].material);
        }

        layout = FindObjectOfType<ProceduralLayoutGeneration>();
        layout.roomSwitched += OnRoomSwitch;
        inRoom = CustomUtilities.FindParentRoom(GetComponentInParent<Grid>().gameObject, layout.rooms);
    }

    void OnRoomSwitch(Room newCurrentRoom, Portal p)
    {
        if (inRoom == newCurrentRoom)
        {
            textParent.SetActive(true);
            // This is done on room switch instead of start because portal interaction changes room children render values - this offsets that change
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    renderers[i].materials[j].renderQueue = 2000;
                    renderers[i].materials[j].SetInt("_StencilValue", 0);
                }
            }
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].material.renderQueue = 2005;
                texts[i].material.SetInt("_Stencil", 0);
            }
            for (int i = 0; i < TMPTexts.Length; i++)
            {
                for (int j = 0; j < TMPTexts[i].fontMaterials.Length; j++)
                {
                    TMPTexts[i].fontMaterials[j].renderQueue = 2005;
                    TMPTexts[i].fontMaterials[j].SetInt("_Stencil", 0);
                }
            }
            for (int i = 0; i < images.Length; i++)
            {
                images[i].material.renderQueue = 2004;
                images[i].material.SetInt("_Stencil", 0);
            }
        }
        else
        {
            textParent.SetActive(false);
        }
    }

    void OnDestroy()
    {
        layout.roomSwitched -= OnRoomSwitch;
    }
}