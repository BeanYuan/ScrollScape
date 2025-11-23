using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 根据窗口范围动态裁切平台的 BoxCollider2D，
/// 现在自动获取所属 ScrollWindow 的 ViewArea，不需要手动拖！
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Platform : MonoBehaviour
{
    [Header("窗口范围（世界空间） — 自动获取，不需要手动拖")]
    public BoxCollider2D viewArea;

    private BoxCollider2D col;
    private SpriteRenderer sr;

    // 保存原始碰撞体
    private Vector2 originalSize;
    private Vector2 originalOffset;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();

        originalSize = col.size;
        originalOffset = col.offset;

        AutoFindViewArea();
    }

    /// <summary>
    /// 自动在父节点中寻找名为 ViewArea 的 BoxCollider2D。
    /// </summary>
    void AutoFindViewArea()
    {
        Transform t = transform;

        // 向上查找，直到找到包含 ViewArea 的父物体
        while (t != null)
        {
            Transform found = t.Find("ViewArea");
            if (found != null)
            {
                BoxCollider2D vc = found.GetComponent<BoxCollider2D>();
                if (vc != null)
                {
                    viewArea = vc;
                    return;
                }
            }

            t = t.parent;
        }

        Debug.LogWarning($"WindowColliderClip: 无法在父级中找到 ViewArea，请检查层级结构", this);
    }

    void LateUpdate()
    {
        if (viewArea == null || col == null || sr == null)
            return;

        Bounds window = viewArea.bounds;
        Bounds sprite = sr.bounds;

        if (!window.Intersects(sprite))
        {
            col.enabled = false;
            return;
        }

        col.enabled = true;

        if (window.Contains(sprite.min) && window.Contains(sprite.max))
        {
            col.size = originalSize;
            col.offset = originalOffset;
            return;
        }

        float minX = Mathf.Max(window.min.x, sprite.min.x);
        float maxX = Mathf.Min(window.max.x, sprite.max.x);
        float minY = Mathf.Max(window.min.y, sprite.min.y);
        float maxY = Mathf.Min(window.max.y, sprite.max.y);

        if (minX >= maxX || minY >= maxY)
        {
            col.enabled = false;
            return;
        }

        Vector3 worldMin = new Vector3(minX, minY, sprite.min.z);
        Vector3 worldMax = new Vector3(maxX, maxY, sprite.max.z);

        Vector3 localMin3 = transform.InverseTransformPoint(worldMin);
        Vector3 localMax3 = transform.InverseTransformPoint(worldMax);

        Vector2 localMin = localMin3;
        Vector2 localMax = localMax3;

        col.size = localMax - localMin;
        col.offset = (localMax + localMin) * 0.5f;
    }
}
