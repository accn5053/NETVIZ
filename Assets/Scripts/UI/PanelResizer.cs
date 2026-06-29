using UnityEngine;
using UnityEngine.EventSystems;

public class PanelResizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform panelRect;
    public float minWidth = 200f;
    public float maxWidth = 600f;

    private bool _dragging = false;
    private float _lastMouseX;
    public bool IsDragging() => _dragging;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        // Optionally change cursor here later
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_dragging)
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RectTransform handle = GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                handle, Input.mousePosition, null, out localPoint);

            if (RectTransformUtility.RectangleContainsScreenPoint(handle, Input.mousePosition))
            {
                _dragging = true;
                _lastMouseX = Input.mousePosition.x;
            }
        }

        if (Input.GetMouseButtonUp(0))
            _dragging = false;

        if (_dragging)
        {
            float delta = _lastMouseX - Input.mousePosition.x;
            _lastMouseX = Input.mousePosition.x;

            float newWidth = Mathf.Clamp(panelRect.sizeDelta.x + delta, minWidth, maxWidth);
            panelRect.sizeDelta = new Vector2(newWidth, panelRect.sizeDelta.y);
        }
    }
}