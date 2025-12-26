using UnityEngine;
using UnityEngine.EventSystems;

public class SafeZoneTap : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.OnSafeTap();
    }
}
