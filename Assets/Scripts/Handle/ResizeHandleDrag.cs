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
    private float dragStartScaleFactor;  // 拖动开始时的缩放倍数

    private Collider2D myCollider;

    [Header("拖动灵敏度（一般 1 就行）")]
    public float sensitivity = 1.0f;

    void Start()
    {
        cam = Camera.main;

        if (window == null)
            window = GetComponentInParent<ResizableWindow>();

        myCollider = GetComponent<Collider2D>();
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
        // 往下拖 → 高度增加（-delta.y 或 +delta.y，取决于你的坐标习惯）
        float newWidth = dragStartWindowSize.x + delta.x * sensitivity;
        float newHeight = dragStartWindowSize.y - delta.y * sensitivity; // 往下拖变大

        // 不让宽高变成负数或 0，防御一下
        newWidth = Mathf.Max(0.01f, newWidth);
        newHeight = Mathf.Max(0.01f, newHeight);

        // 相对变化比例（基于拖动开始时的宽高）
        float factorW = newWidth / dragStartWindowSize.x;
        float factorH = newHeight / dragStartWindowSize.y;

        // 为了保持比例统一，我们取两个方向里“放大的那一侧”
        float factor = Mathf.Max(factorW, factorH);

        // 最终缩放倍数 = 初始缩放倍数 × 相对变化倍数
        float targetScaleFactor = dragStartScaleFactor * factor;

        window.ResizeWithScaleFactor(targetScaleFactor);
    }

    void OnMouseUp()
    {
        dragging = false;
    }
}
