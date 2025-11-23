using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制滚动窗口：根据 Handle 的位置移动 ContentRoot。
/// contentMinLocalPos / contentMaxLocalPos = 相对初始位置的“位移范围”，
/// 不会在点击 Handle 时重置 ContentRoot 的初始位置。
/// </summary>
public class ScrollWindowController : MonoBehaviour
{
    [Header("引用")]
    public Transform contentRoot;       // ContentRoot
    public Transform scrollBarRoot;     // ScrollBarRoot
    public Transform handle;            // Handle（被拖动的按钮）

    [Header("内容在滚动时的位移范围（相对初始位置的偏移量）")]
    public Vector2 contentMinLocalPos;  // Handle 在最小位置时，相对初始位置的偏移
    public Vector2 contentMaxLocalPos;  // Handle 在最大位置时，相对初始位置的偏移

    [Header("是否启用方向")]
    public bool enableHorizontal = false;
    public bool enableVertical = true;

    // 供 HandleDrag 读取的可拖动范围（世界坐标）
    private Bounds scrollBounds;

    // 记录 ContentRoot 的“基准位置”
    private Vector3 baseContentLocalPos;

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

        // 用 ScrollBarRoot 上的 Renderer 或 Collider 来确定 Handle 可拖范围
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

        if (contentRoot == null || handle == null) return;

        // 1）先拿到此刻你在 Scene 里摆好的 ContentRoot 位置
        Vector3 currentContentPos = contentRoot.localPosition;

        // 2）根据 Handle 当前在滚动条中的位置，算出 tInit（0~1）
        float tInitX = 0.5f;
        float tInitY = 0.5f;

        if (enableHorizontal)
        {
            tInitX = Mathf.InverseLerp(scrollBounds.min.x, scrollBounds.max.x, handle.position.x);
        }

        if (enableVertical)
        {
            tInitY = Mathf.InverseLerp(scrollBounds.min.y, scrollBounds.max.y, handle.position.y);
        }

        // 3）在 tInit 下，理论上的偏移量（由你在 Inspector 设置的范围决定）
        Vector3 initOffset = Vector3.zero;

        if (enableHorizontal)
        {
            float offX = Mathf.Lerp(contentMinLocalPos.x, contentMaxLocalPos.x, tInitX);
            initOffset.x = offX;
        }

        if (enableVertical)
        {
            float offY = Mathf.Lerp(contentMinLocalPos.y, contentMaxLocalPos.y, tInitY);
            initOffset.y = offY;
        }

        // 4）反推“基准位置”，保证：
        //    在 t = tInit 时，baseContentLocalPos + offset(tInit) = 你现在看到的 ContentRoot 位置
        baseContentLocalPos = currentContentPos - initOffset;

        // ? 不再在这里修改 contentRoot.localPosition，
        //    也不强行把 handle 归中，一切以 Scene 中的初始摆放为准。
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

        // 从“基准位置”开始，加上偏移
        Vector3 lp = baseContentLocalPos;

        if (enableHorizontal)
        {
            float offsetX = Mathf.Lerp(contentMinLocalPos.x, contentMaxLocalPos.x, tX);
            lp.x += offsetX;
        }

        if (enableVertical)
        {
            float offsetY = Mathf.Lerp(contentMinLocalPos.y, contentMaxLocalPos.y, tY);
            lp.y += offsetY;
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
