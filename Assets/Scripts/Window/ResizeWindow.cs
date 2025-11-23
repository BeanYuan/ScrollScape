using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可缩放窗口：以 Frame 左上角为锚点，整体做等比缩放（scaleX = scaleY）。
/// 并且会对 Tag 为 "Player" 的子物体做缩放抵消（不被窗口缩放影响大小）。
/// </summary>
public class ResizableWindow : MonoBehaviour
{
    [Header("引用")]
    public SpriteRenderer frameSprite;    // 窗口背景
    public Transform contentRoot;         // 窗口内部内容（可选）
    public Transform resizeHandle;        // 右下角的缩放按钮（可选）

    [Header("缩放范围（相对于 Awake 时的 localScale）")]
    public float minScale = 0.1f;
    public float maxScale = 2.0f;

    // Awake 时的原始缩放（要求 x≈y）
    private float baseScale;

    // 当前相对缩放倍数（相对于 baseScale）
    private float currentScaleFactor = 1f;

    // 左上角锚点（世界坐标）
    private Vector2 anchorTopLeftWorld;

    void Awake()
    {
        if (frameSprite == null)
        {
            Transform t = transform.Find("Frame");
            if (t != null) frameSprite = t.GetComponent<SpriteRenderer>();
        }

        if (contentRoot == null)
        {
            Transform t = transform.Find("ContentRoot");
            if (t != null) contentRoot = t;
        }

        if (resizeHandle == null)
        {
            Transform t = transform.Find("ResizeHandle");
            if (t != null) resizeHandle = t;
        }

        // 记录 Awake 状态下的缩放作为基准
        baseScale = transform.localScale.x;
        // 如果一开始就保证 x=y，这里就 OK；
        // 不放心可以用平均值：
        // baseScale = 0.5f * (transform.localScale.x + transform.localScale.y);

        // 初始化当前缩放倍数
        currentScaleFactor = 1f;

        if (frameSprite != null)
        {
            Bounds b = frameSprite.bounds;
            // 左上角 = (min.x, max.y)
            anchorTopLeftWorld = new Vector2(b.min.x, b.max.y);
        }
        else
        {
            anchorTopLeftWorld = transform.position;
        }
    }

    /// <summary>
    /// 当前等比缩放因子（相对于 Awake 时的 baseScale）。
    /// </summary>
    public float GetCurrentScaleFactor()
    {
        return currentScaleFactor;
    }

    /// <summary>
    /// 仅在拖动开始时使用：获得当前 Frame 的 world 尺寸（方便计算相对缩放）。
    /// </summary>
    public Vector2 GetCurrentWorldSize()
    {
        if (frameSprite == null) return Vector2.zero;
        var size = frameSprite.bounds.size;
        return new Vector2(size.x, size.y);
    }

    /// <summary>
    /// 用一个 scalar 做统一缩放（x=y），并保持 Frame 左上角不动。
    /// scaleFactor 是相对 Awake 状态的倍数，比如 1=原始，0.5=一半，2=两倍。
    /// Tag = "Player" 的子物体会做缩放抵消，不随窗口缩放变形。
    /// </summary>
    public void ResizeWithScaleFactor(float scaleFactor)
    {
        if (frameSprite == null || baseScale <= 0f)
            return;

        // 1. clamp 到允许范围
        float targetFactor = Mathf.Clamp(scaleFactor, minScale, maxScale);

        // 计算本次缩放相对上一次的变化倍数
        float scaleChange = targetFactor / currentScaleFactor;
        if (Mathf.Approximately(scaleChange, 1f))
            return; // 几乎没变，就不用算了

        // 2. 应用新的缩放（等比）
        float targetScale = baseScale * targetFactor;

        Vector3 local = transform.localScale;
        local.x = targetScale;
        local.y = targetScale;
        transform.localScale = local;

        // 3. 对 Tag=Player 的子物体做反向 scale，抵消父级缩放
        //    这样它们在世界空间下大小保持不变，但仍然跟随位置移动。
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (var t in allChildren)
        {
            if (t == this.transform) continue;

            if (t.CompareTag("Player"))
            {
                Vector3 ls = t.localScale;
                ls.x /= scaleChange;
                ls.y /= scaleChange;
                t.localScale = ls;
            }
        }

        // 4. 缩放后重新计算 Frame 左上角，并对齐到锚点
        Bounds b = frameSprite.bounds;
        Vector2 newTopLeft = new Vector2(b.min.x, b.max.y);
        Vector2 offset = anchorTopLeftWorld - newTopLeft;
        transform.position += (Vector3)offset;

        // 5. 更新当前缩放倍数
        currentScaleFactor = targetFactor;
    }
}
