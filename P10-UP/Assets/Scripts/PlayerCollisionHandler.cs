using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Mathematics;

public class PlayerCollisionHandler : MonoBehaviour
{
    private Portal thisPortal;
    private ProceduralLayoutGeneration proceduralLayout;
    private TextMeshProUGUI gui;
    private bool inPortal = false;
    private Transform playerCam;

    // Start is called before the first frame update
    void Start()
    {
        proceduralLayout = FindObjectOfType<ProceduralLayoutGeneration>();
        playerCam = Camera.main.transform;
    }
    private void Update()
    {
        //*/ Portal culling based on view cone
        for (int i = 0; i < proceduralLayout.activeThroughPortals.Count; i++)
        {
            Vector3 cameraDirToPortal = (proceduralLayout.currentRoom.GetPortalsInRoom()[i].transform.position - playerCam.position).normalized;
            bool isPortalVisible = math.dot(cameraDirToPortal, proceduralLayout.currentRoom.GetPortalsInRoom()[i].transform.forward) >= 0;
            if (!inPortal)
            {
                if (isPortalVisible && !proceduralLayout.currentRoom.GetPortalsInRoom()[i].gameObject.activeSelf)
                {
                    proceduralLayout.currentRoom.GetPortalsInRoom()[i].SetActive(true);
                }
                else if (!isPortalVisible && proceduralLayout.currentRoom.GetPortalsInRoom()[i].gameObject.activeSelf)
                {
                    proceduralLayout.currentRoom.GetPortalsInRoom()[i].SetActive(false);
                }
            }

            for (int j = 0; j < proceduralLayout.activeThroughPortals[i].Count; j++)
            {
                if (isPortalVisible)
                {
                    if (!proceduralLayout.activeThroughPortals[i][j].gameObject.activeSelf)
                    {
                        proceduralLayout.activeThroughPortals[i][j].gameObject.SetActive(true);
                    }
                }
                else if (proceduralLayout.activeThroughPortals[i][j].gameObject.activeSelf)
                {
                    proceduralLayout.activeThroughPortals[i][j].gameObject.SetActive(false);
                }
            }
        }
        //*/
    }

    private void OnTriggerEnter(Collider triggerCollider)
    {
        if (triggerCollider.CompareTag("Portal"))
        {
            inPortal = true;
            thisPortal = triggerCollider.GetComponent<Portal>(); 
            thisPortal.SwitchActiveSubPortal();

            // Switch world
            proceduralLayout.SwitchCurrentRoom(thisPortal.GetConnectedRoom(), thisPortal);
        }
    }

    private void OnTriggerExit(Collider triggerCollider)
    {
        if (triggerCollider.CompareTag("Portal"))
        {
            thisPortal.SwitchActiveSubPortal();
            Vector3 offset = transform.position - triggerCollider.transform.position;
            if (Vector3.Dot(offset, triggerCollider.transform.forward) > 0.0f) // Correctly exited portal
            {
                proceduralLayout.FinalizeRoomSwitch(thisPortal);
            }
            else // Incorrectly exited the portal, revert changes made OnTriggerEnter
            {
                proceduralLayout.SwitchCurrentRoom(proceduralLayout.previousRoom, null);
            }

            inPortal = false;
        }
    }
}
