using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GridPosition : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int x;
    [SerializeField] private int y;

    public void SetPosition(int newX, int newY)
    {
        this.x = newX;
        this.y = newY;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        TriggerClick();
    }

    private void Update()
    {
        // Fallback: If EventSystem is missing in the scene, use New Input System raycast
        if (EventSystem.current == null && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (Camera.main != null)
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    TriggerClick();
                }
            }
        }
    }

    private void TriggerClick()
    {
        Debug.Log("Click! " + x + ", " + y);
        GameManager.Instance.ClickedOnGridPosition(x, y);
    }

    public int GetX() => x;
    public int GetY() => y;
}
