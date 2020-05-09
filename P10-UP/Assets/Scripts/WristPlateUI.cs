using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WristPlateUI : MonoBehaviour
{
    public GameObject DropZone;
    public TMP_Text cardAmount;
    public TMP_Text currentAmount;

    private int heldAmount;
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
        if (keysList.Count > 0)
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
    }

    public void Backward()
    {
        if (keysList.Count > 0)
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
    }

    public void TakeOutCard(GameObject card)
    {
        Debug.Log("Took out card of inventory");
        keysList.Remove(card);
        currentCard = 0;
    }


    void Update()
    {
        heldAmount = keysList.Count;
        cardAmount.text = heldAmount.ToString();

        if (keysList.Count == 0)
        {
            currentAmount.gameObject.SetActive(false);
        }
        else if (keysList.Count > 0)
        {
            currentAmount.gameObject.SetActive(true);
            currentAmount.text = (currentCard + 1).ToString();

        }
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
