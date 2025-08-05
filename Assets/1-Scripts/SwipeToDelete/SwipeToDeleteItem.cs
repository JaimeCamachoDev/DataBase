using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

public class SwipeToDeleteItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private LayoutElement layoutElement;
    private Vector2 startDragPos;
    private Vector2 originalPos;
    private bool dragging = false;
    private bool swipePerformed = false;

    [Header("Swipe Settings")]
    [SerializeField] private float deleteThreshold = 100f; // px para borrar
    [SerializeField] private float returnSpeed = 10f;

    public UnityEvent onDelete = new UnityEvent();
    public UnityEvent onComplete = new UnityEvent();
    public UnityEvent onClick = new UnityEvent();

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();

        originalPos = rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startDragPos = eventData.position;
        layoutElement.ignoreLayout = true;
        dragging = true;
        swipePerformed = false;
        Debug.Log("🟢 PointerDown - Empezar swipe");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        float deltaX = eventData.position.x - startDragPos.x;
        rectTransform.anchoredPosition = new Vector2(originalPos.x + deltaX, originalPos.y);
        if (Mathf.Abs(deltaX) > EventSystem.current.pixelDragThreshold)
        {
            swipePerformed = true;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        float finalDelta = rectTransform.anchoredPosition.x - originalPos.x;

        if (finalDelta <= -deleteThreshold)
        {
            Debug.Log("💥 Item eliminado por swipe");
            swipePerformed = true;
            onDelete.Invoke();
            Destroy(gameObject.transform.parent.gameObject);
        }
        else if (finalDelta >= deleteThreshold)
        {
            Debug.Log("✅ Item completado por swipe");
            swipePerformed = true;
            layoutElement.ignoreLayout = false;
            onComplete.Invoke();
        }
        else
        {
            Debug.Log("↩️ Swipe cancelado, volver");
            layoutElement.ignoreLayout = false;
            StartCoroutine(AnimateReturn());
        }

        swipePerformed = false;
    }

    private System.Collections.IEnumerator AnimateReturn()
    {
        Vector2 current = rectTransform.anchoredPosition;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            rectTransform.anchoredPosition = Vector2.Lerp(current, originalPos, t);
            yield return null;
        }
        rectTransform.anchoredPosition = originalPos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!swipePerformed)
        {
            onClick.Invoke();
        }
        swipePerformed = false;
    }
}
