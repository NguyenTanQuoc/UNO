using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private RectTransform Panel;
    private Rect LastSafeArea = new(0, 0, 0, 0);

    private void Awake()
    {
        Panel = GetComponent<RectTransform>();
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        Rect safeArea = Screen.safeArea;
        if (safeArea != LastSafeArea)
        {
            ApplySafeArea(safeArea);
        }
    }

    private void ApplySafeArea(Rect r)
    {
        LastSafeArea = r;
        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        Panel.anchorMin = anchorMin;
        Panel.anchorMax = anchorMax;
    }
}