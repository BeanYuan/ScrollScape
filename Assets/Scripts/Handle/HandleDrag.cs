using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HandleDrag : MonoBehaviour
{
    public ScrollWindowController window;
    public LayerMask handleLayerMask;

    private Camera cam;
    private bool dragging;
    private Vector3 dragOffset;

    private Vector2 handleHalfSize;
    private Collider2D myCollider;

    private Bounds scrollBounds;

    void Start()
    {
        cam = Camera.main;

        if (window == null)
            window = GetComponentInParent<ScrollWindowController>();

        myCollider = GetComponent<Collider2D>();

        // 获取 handle sprite 半尺寸
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            handleHalfSize = sr.bounds.extents;
        else
            handleHalfSize = Vector2.zero;
    }

    void OnMouseDown()
    {
        if (cam == null || window == null) return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, Mathf.Infinity, handleLayerMask);

        if (hit.collider == myCollider)
        {
            dragging = true;
            dragOffset = transform.position - (Vector3)mouseWorld;
            scrollBounds = window.GetScrollBounds();
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

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;

        // 先从当前坐标开始，按需要的轴分别更新
        Vector3 targetPos = transform.position;

        // 计算可移动范围（两轴共用）
        float minX = scrollBounds.min.x + handleHalfSize.x;
        float maxX = scrollBounds.max.x - handleHalfSize.x;
        float minY = scrollBounds.min.y + handleHalfSize.y;
        float maxY = scrollBounds.max.y - handleHalfSize.y;

        // 只在父级勾选了 horizontal 时才允许改变 X
        if (window.enableHorizontal)
        {
            float rawX = mouseWorld.x + dragOffset.x;
            targetPos.x = Mathf.Clamp(rawX, minX, maxX);
        }

        // 只在父级勾选了 vertical 时才允许改变 Y
        if (window.enableVertical)
        {
            float rawY = mouseWorld.y + dragOffset.y;
            targetPos.y = Mathf.Clamp(rawY, minY, maxY);
        }

        transform.position = targetPos;

        // 归一化值，供 ScrollWindowController 使用
        float tX = Mathf.InverseLerp(minX, maxX, targetPos.x);
        float tY = Mathf.InverseLerp(minY, maxY, targetPos.y);

        window.SetScroll01(tX, tY);
    }

    void OnMouseUp()
    {
        dragging = false;
    }
}
