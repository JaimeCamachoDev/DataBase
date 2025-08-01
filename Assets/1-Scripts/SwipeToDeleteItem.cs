using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipeToDeleteItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private LayoutElement layoutElement;
    private Vector2 originalPos;
    private bool isListening = false;
    private bool hasMoved = false;

    [Header("Swipe Settings")]
    [SerializeField] private float deleteThreshold = 30f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();

        originalPos = rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        SwipeManager.OnOnSwipePercentage += OnSwipePercentage;
        SwipeManager.OnSwipe += OnSwipeFinal;
    }

    private void OnDisable()
    {
        SwipeManager.OnOnSwipePercentage -= OnSwipePercentage;
        SwipeManager.OnSwipe -= OnSwipeFinal;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isListening = true;
        hasMoved = false;
        layoutElement.ignoreLayout = true;
        originalPos = rectTransform.anchoredPosition;

        Debug.Log($"🟢 PointerDown en {name}");
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isListening)
            return;

        Debug.Log("🔴 PointerUp: Cancelar swipe");
        rectTransform.anchoredPosition = originalPos;
        isListening = false;
    }
    
    private void OnSwipePercentage(Dictionary<string, float> percentages)
    {
        if (!isListening) return;

        float left = percentages["Left"];
        float right = percentages["Right"];
        float offset = (right - left) * 5f; // Multiplicador visual

        rectTransform.anchoredPosition = new Vector2(originalPos.x + offset, originalPos.y);

        if (Mathf.Abs(offset) > 1f) hasMoved = true;

        Debug.Log($"📈 {name} offset: {offset:F1} - posX: {rectTransform.anchoredPosition.x:F1}");
    }

    private void OnSwipeFinal(string direction)
    {
        if (!isListening || !hasMoved)
        {
            layoutElement.ignoreLayout = false;
            return;
        }

        float delta = originalPos.x - rectTransform.anchoredPosition.x;
        Debug.Log($"🏁 {name} swipe: {direction} | deltaX = {delta:F1}");

        if (direction == "Left" && delta > deleteThreshold)
        {
            Debug.Log("💥 Eliminando: " + name);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("↩️ Restaurando posición");
            rectTransform.anchoredPosition = originalPos;
            layoutElement.ignoreLayout = false;
        }

        isListening = false;
        hasMoved = false;
    }
}
