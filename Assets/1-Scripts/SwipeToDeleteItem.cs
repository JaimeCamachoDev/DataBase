using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeToDeleteItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Vector2 originalPos;

    [Header("Swipe Settings")]
    [SerializeField] private float deleteThreshold = 30f;
    private bool isListening = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
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
        Debug.Log("🟢 PointerDown: Escuchando swipe");
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
        float offset = (right - left) * 4f;

        rectTransform.anchoredPosition = new Vector2(originalPos.x + offset, originalPos.y);
        Debug.Log("📈 Porcentaje detectado: L " + left + " / R " + right);
    }

    private void OnSwipeFinal(string direction)
    {
        if (!isListening) return;

        Debug.Log("🏁 Swipe final detectado: " + direction);

        if (direction == "Left" && rectTransform.anchoredPosition.x < originalPos.x - deleteThreshold)
        {
            Debug.Log("💥 Eliminar item");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("↩️ Volver a posición original");
            rectTransform.anchoredPosition = originalPos;
        }

        isListening = false;
    }
}
