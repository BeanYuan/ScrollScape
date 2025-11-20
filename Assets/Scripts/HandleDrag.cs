using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HandleDrag : MonoBehaviour
{
    public ScrollWindowController window;

    private Camera cam;
    private bool dragging;
    private Vector3 dragOffset;

    private Vector2 handleHalfSize;
    private Collider2D myCollider;

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

    void Update()
    {
        HandleMouseEvents();
    }

    void HandleMouseEvents()
    {
        if (cam == null || window == null)
            return;

        // --------------------- 按下：判断是否命中自己 -------------------
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("HandleDrag: Mouse Down");
            Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);

            // 只在命中“自己”的情况下才进入 dragging
            if (hit.collider != null && hit.collider == myCollider)
            {
                Debug.Log("HandleDrag: Hit self, start dragging");
                dragging = true;
                dragOffset = transform.position - (Vector3)mouseWorld;
            }
            else
            {
                Debug.Log("HandleDrag: Did not hit self");
                dragging = false;
            }
        }

        // --------------------- 拖动中 -------------------
        if (dragging && Input.GetMouseButton(0))
        {
            Bounds scrollBounds = window.GetScrollBounds();

            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = transform.position.z;

            Vector3 targetPos = mouseWorld + dragOffset;

            float minX = scrollBounds.min.x + handleHalfSize.x;
            float maxX = scrollBounds.max.x - handleHalfSize.x;
            float minY = scrollBounds.min.y + handleHalfSize.y;
            float maxY = scrollBounds.max.y - handleHalfSize.y;

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

            transform.position = targetPos;

            float tX = Mathf.InverseLerp(minX, maxX, targetPos.x);
            float tY = Mathf.InverseLerp(minY, maxY, targetPos.y);

            window.SetScroll01(tX, tY);
        }

        // --------------------- 松开 -------------------
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }
    }
}
