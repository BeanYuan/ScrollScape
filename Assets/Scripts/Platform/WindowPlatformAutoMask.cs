using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 让挂在本物体上的 SpriteRenderer 自动适配最近的 SpriteMask：
/// - 如果向上父级链中找到某个 SpriteMask：
///     * 将本 Sprite 的 sortingLayer 设置为该 Mask 的 Layer；
///     * 将 sortingOrder 限制在 Mask Back/Front Order 范围之内；
/// - 如果找不到 Mask，则保持原样（比如放在大世界 Tilemap 时）。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class WindowPlatformAutoMask : MonoBehaviour
{
    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // 1. 向上找最近的 SpriteMask（通常在窗口的 ViewArea 或 Frame 上）
        SpriteMask mask = null;
        Transform t = transform;

        while (t != null && mask == null)
        {
            mask = t.GetComponentInChildren<SpriteMask>();
            t = t.parent;
        }

        // 如果没找到任何 Mask，就不改（可能是世界平台）
        if (mask == null) return;

        // 2. 把自己的 Sorting Layer 改成和 Mask 一样
        //    （注意 SpriteMask 有 frontSortingLayerID/backSortingLayerID）
        int layerId = mask.frontSortingLayerID;
        if (layerId == 0)
        {
            // 有时只设置了 backSortingLayerID，你也可以从那儿拿
            layerId = mask.backSortingLayerID;
        }

        if (layerId != 0) // 0 一般是 Default，如果你没用到就照旧
        {
            sr.sortingLayerID = layerId;
        }

        // 3. 把 sortingOrder 收进 Mask 的 Order 范围中
        int backOrder = mask.backSortingOrder;
        int frontOrder = mask.frontSortingOrder;

        // 防御：确保 front > back
        if (frontOrder <= backOrder)
        {
            frontOrder = backOrder + 1;
        }

        // 当前 order 挤进中间，避免和 Mask 边界重合
        int desiredOrder = Mathf.Clamp(sr.sortingOrder, backOrder + 1, frontOrder - 1);
        sr.sortingOrder = desiredOrder;
    }
}
