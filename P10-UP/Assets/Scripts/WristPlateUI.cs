using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristPlateUI : MonoBehaviour
{
    public GameObject DropZone;

    private int currentCard = 0;
    private List<GameObject> keysList = new List<GameObject>();

    void Start()
    {

    }

    public void ListAdder(GameObject card)
    {
        keysList.Add(card);
    }

    public void Spawn()
    {
        if (keysList.Count > 0)
        {
            keysList[currentCard].SetActive(true);
        }
    }

    public void Forward()
    {
        keysList[currentCard].gameObject.SetActive(false);
        if (currentCard + 1 > keysList.Count - 1)
        {
            currentCard = 0;
        } 
        else
        {
            currentCard++;
        }
        keysList[currentCard].gameObject.SetActive(true);
    }

    public void Backward()
    {
        keysList[currentCard].gameObject.SetActive(false);
        if (currentCard - 1 < 0)
        {
            currentCard = keysList.Count - 1;
        }
        else
        {
            currentCard--;
        }
        keysList[currentCard].gameObject.SetActive(true);
    }

    void Update()
    {

    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Keycard"))
        {
            DropZone.SetActive(true);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Keycard"))
        {
            DropZone.SetActive(false);
        }
    }

}
