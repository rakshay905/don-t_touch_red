using UnityEngine;
using UnityEngine.EventSystems;

public class RedZoneTouch : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }
}
