using UnityEngine;
using UnityEngine.UI;

public class FingerPress : MonoBehaviour
{
    private void OnTriggerEnter(Collider pressable)
    {
        if (pressable.CompareTag("PressableUI"))
        {
            pressable.GetComponentInChildren<Button>()?.onClick.Invoke();
        }
    }
}
