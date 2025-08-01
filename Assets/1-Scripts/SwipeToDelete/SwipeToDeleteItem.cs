﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipeToDeleteItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private LayoutElement layoutElement;
    private Vector2 startDragPos;
    private Vector2 originalPos;
    private bool dragging = false;

    [Header("Swipe Settings")]
    [SerializeField] private float deleteThreshold = 100f; // px para borrar
    [SerializeField] private float returnSpeed = 10f;

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
        Debug.Log("🟢 PointerDown - Empezar swipe");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        float deltaX = eventData.position.x - startDragPos.x;
        rectTransform.anchoredPosition = new Vector2(originalPos.x + deltaX, originalPos.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        float finalDelta = rectTransform.anchoredPosition.x - originalPos.x;

        if (Mathf.Abs(finalDelta) >= deleteThreshold)
        {
            Debug.Log("💥 Item eliminado por swipe");
            Destroy(gameObject.transform.parent.gameObject);
        }
        else
        {
            Debug.Log("↩️ Swipe cancelado, volver");
            layoutElement.ignoreLayout = false;
            StartCoroutine(AnimateReturn());
        }
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
}
