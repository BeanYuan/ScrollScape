using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制滚动窗口：根据 Handle 的位置移动 ContentRoot。
/// 支持上下、左右两个方向；你可以只开纵向。
/// </summary>
public class ScrollWindowController : MonoBehaviour
{
    [Header("引用")]
    public Transform contentRoot;       // ContentRoot
    public Transform scrollBarRoot;     // ScrollBarRoot
    public Transform handle;            // Handle（被拖动的按钮）

    [Header("内容在滚动时的位移范围（本地坐标）")]
    public Vector2 contentMinLocalPos;  // Handle 在滚动条“最小位置”时，ContentRoot 的 localPosition
    public Vector2 contentMaxLocalPos;  // Handle 在滚动条“最大位置”时，ContentRoot 的 localPosition

    [Header("是否启用方向")]
    public bool enableHorizontal = false;   // 现在可以先关掉
    public bool enableVertical = true;      // 纵向滚动

    // 供 HandleDrag 读取的可拖动范围（世界坐标）
    private Bounds scrollBounds;

    void Awake()
    {
        if (contentRoot == null)
        {
            Transform t = transform.Find("ContentRoot");
            if (t != null) contentRoot = t;
        }

        if (scrollBarRoot == null)
        {
            Transform t = transform.Find("ScrollBarRoot");
            if (t != null) scrollBarRoot = t;
        }

        if (handle == null && scrollBarRoot != null)
        {
            Transform t = scrollBarRoot.Find("Handle");
            if (t != null) handle = t;
        }

        // 用 ScrollBarRoot 上的 Renderer 或 Collider 来确定范围
        if (scrollBarRoot != null)
        {
            var sr = scrollBarRoot.GetComponentInChildren<Renderer>();
            var col2D = scrollBarRoot.GetComponentInChildren<Collider2D>();

            if (sr != null)
                scrollBounds = sr.bounds;
            else if (col2D != null)
                scrollBounds = col2D.bounds;
            else
                scrollBounds = new Bounds(scrollBarRoot.position, scrollBarRoot.localScale);
        }

        // 初始时 handle 放在 root 中心
        if (handle != null)
        {
            Vector3 pos = handle.position;
            pos.x = scrollBounds.center.x;
            pos.y = scrollBounds.center.y;
            handle.position = pos;
        }

        // 初始时内容放在两端的中间（你可以在 Inspector 手动改）
        if (contentRoot != null)
        {
            Vector3 lp = contentRoot.localPosition;
            lp.x = Mathf.Lerp(contentMinLocalPos.x, contentMaxLocalPos.x, 0.5f);
            lp.y = Mathf.Lerp(contentMinLocalPos.y, contentMaxLocalPos.y, 0.5f);
            contentRoot.localPosition = lp;
        }
    }

    /// <summary>
    /// 给 HandleDrag 调用：传入归一化位置（0~1），更新内容位置。
    /// tX、tY 分别代表在滚动条范围内的比例。
    /// </summary>
    public void SetScroll01(float tX, float tY)
    {
        if (contentRoot == null) return;

        tX = Mathf.Clamp01(tX);
        tY = Mathf.Clamp01(tY);

        Vector3 lp = contentRoot.localPosition;

        if (enableHorizontal)
        {
            lp.x = Mathf.Lerp(contentMinLocalPos.x, contentMaxLocalPos.x, tX);
        }

        if (enableVertical)
        {
            lp.y = Mathf.Lerp(contentMinLocalPos.y, contentMaxLocalPos.y, tY);
        }

        contentRoot.localPosition = lp;
    }

    /// <summary>
    /// 提供给 HandleDrag 使用的滚动条世界范围。
    /// </summary>
    public Bounds GetScrollBounds()
    {
        return scrollBounds;
    }
}

