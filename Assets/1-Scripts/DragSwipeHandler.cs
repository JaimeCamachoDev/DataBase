using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragSwipeHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Range(0f, 1f)]
    public float swipeThreshold = 0.5f;

    public UnityEvent OnSwipeUp;
    public UnityEvent OnSwipeDown;
    public UnityEvent OnSwipeRight;
    public UnityEvent OnSwipeLeft;

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData data)
    {
        Vector2 dir = (data.position - data.pressPosition).normalized;

        if (OnSwipeRight != null && dir.x > swipeThreshold)
            OnSwipeRight.Invoke();

        if (OnSwipeLeft != null && dir.x < -swipeThreshold)
            OnSwipeLeft.Invoke();

        if (OnSwipeUp != null && dir.y > swipeThreshold)
            OnSwipeUp.Invoke();

        if (OnSwipeDown != null && dir.y < -swipeThreshold)
            OnSwipeDown.Invoke();
    }
}