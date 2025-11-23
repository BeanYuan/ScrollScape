using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 根据窗口范围动态裁切平台的 BoxCollider2D，
/// 用于配合 SpriteMask：视觉上被遮掉的地方，碰撞也被裁掉。
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class WindowColliderClip : MonoBehaviour
{
    [Header("窗口范围（世界空间）")]
    public BoxCollider2D viewArea;   // 拖你那个“窗口 BoxCollider2D”进来

    private BoxCollider2D col;
    private SpriteRenderer sr;

    // 记录原始碰撞体（平台完全显示时的尺寸）
    private Vector2 originalSize;
    private Vector2 originalOffset;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();

        originalSize = col.size;
        originalOffset = col.offset;
    }

    void LateUpdate()
    {
        if (viewArea == null || col == null || sr == null)
            return;

        Bounds window = viewArea.bounds;
        Bounds sprite = sr.bounds;

        // 如果完全没有交集，直接关掉碰撞体
        if (!window.Intersects(sprite))
        {
            col.enabled = false;
            return;
        }

        col.enabled = true;

        // 如果平台完全在窗口内，就用原始碰撞体，避免反复算误差
        if (window.Contains(sprite.min) && window.Contains(sprite.max))
        {
            col.size = originalSize;
            col.offset = originalOffset;
            return;
        }

        // 计算窗口与平台 sprite 的交集矩形（世界坐标）
        float minX = Mathf.Max(window.min.x, sprite.min.x);
        float maxX = Mathf.Min(window.max.x, sprite.max.x);
        float minY = Mathf.Max(window.min.y, sprite.min.y);
        float maxY = Mathf.Min(window.max.y, sprite.max.y);

        // 再做一重防御
        if (minX >= maxX || minY >= maxY)
        {
            col.enabled = false;
            return;
        }

        Vector3 worldMin = new Vector3(minX, minY, sprite.min.z);
        Vector3 worldMax = new Vector3(maxX, maxY, sprite.max.z);

        // 转成平台本身的局部坐标
        Vector3 localMin3 = transform.InverseTransformPoint(worldMin);
        Vector3 localMax3 = transform.InverseTransformPoint(worldMax);

        Vector2 localMin = localMin3;
        Vector2 localMax = localMax3;

        Vector2 newSize = localMax - localMin;
        Vector2 newCenter = (localMax + localMin) * 0.5f;

        col.size = newSize;
        col.offset = newCenter;
    }
}
