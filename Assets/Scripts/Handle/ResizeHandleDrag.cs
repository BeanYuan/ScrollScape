using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ResizeHandleDrag : MonoBehaviour
{
    public ResizableWindow window;   // 拖父级 ResizeWindow 进来

    private Camera cam;
    private bool dragging;

    private Vector2 dragStartMouseWorld;
    private Vector2 dragStartWindowSize;  // 拖动开始时的窗口世界宽高
    private float dragStartScaleFactor;   // 拖动开始时的缩放倍数

    private Collider2D myCollider;

    [Header("拖动灵敏度（一般 1 就行）")]
    public float sensitivity = 1.0f;

    [Header("悬浮高亮")]
    public Color hoverColor = new Color(1f, 1f, 1f, 1f);  // 鼠标悬浮时的颜色
    private SpriteRenderer sr;
    private Color originalColor;
    private bool hasOriginalColor = false;

    void Start()
    {
        cam = Camera.main;

        if (window == null)
            window = GetComponentInParent<ResizableWindow>();

        myCollider = GetComponent<Collider2D>();

        // 获取 SpriteRenderer & 原始颜色
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalColor = sr.color;
            hasOriginalColor = true;
        }
    }

    void OnMouseDown()
    {
        if (cam == null || window == null) return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);

        // 确保只点到自己才开始拖动
        if (hit.collider == myCollider)
        {
            dragging = true;
            dragStartMouseWorld = mouseWorld;
            dragStartWindowSize = window.GetCurrentWorldSize();
            dragStartScaleFactor = window.GetCurrentScaleFactor();
        }
        else
        {
            dragging = false;
        }
    }

    void OnMouseDrag()
    {
        if (!dragging || cam == null || window == null)
            return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 delta = mouseWorld - dragStartMouseWorld;

        // 以右下角为把手：
        // 往右拖 → 宽度增加（delta.x）
        // 往下拖 → 高度增加（-delta.y）
        float newWidth = dragStartWindowSize.x + delta.x * sensitivity;
        float newHeight = dragStartWindowSize.y - delta.y * sensitivity; // 往下拖变大

        // 防止宽高为 0 或负数
        newWidth = Mathf.Max(0.01f, newWidth);
        newHeight = Mathf.Max(0.01f, newHeight);

        // 相对变化比例
        float factorW = newWidth / dragStartWindowSize.x;
        float factorH = newHeight / dragStartWindowSize.y;

        // 保持等比缩放：取较大的那个
        float factor = Mathf.Max(factorW, factorH);

        float targetScaleFactor = dragStartScaleFactor * factor;

        window.ResizeWithScaleFactor(targetScaleFactor);
    }

    void OnMouseUp()
    {
        dragging = false;
    }

    // ---------- 悬浮高亮 ----------
    void OnMouseEnter()
    {
        if (sr != null && hasOriginalColor)
        {
            sr.color = hoverColor;
        }
    }

    void OnMouseExit()
    {
        if (sr != null && hasOriginalColor)
        {
            sr.color = originalColor;
        }
    }
}
