using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 桌面图标式激活按钮：
/// - 鼠标单击时：
///     如果所有目标窗口都是关闭的 -> 启用它们；
///     如果任意一个目标窗口是开启的 -> 不做事（图标锁定，直到窗口被关闭）；
/// - 不隐藏自身 Sprite / Collider；
/// - 支持点击音效和悬浮高亮。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DesktopIconActivator : MonoBehaviour
{
    [Header("要打开的窗口（SetActive(true)）")]
    public List<GameObject> windowsToActivate = new List<GameObject>();

    [Header("点击音效")]
    public AudioClip clickSfx;      // 点击图标时播放
    public AudioSource audioSource; // 建议手动挂在同一物体或父物体上

    [Header("悬浮高亮")]
    public Color hoverColor = new Color(1f, 0.9f, 0.7f, 1f);  // 鼠标悬浮时颜色

    private Collider2D col;
    private SpriteRenderer sr;
    private Color originalColor;
    private bool hasOriginalColor = false;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            originalColor = sr.color;
            hasOriginalColor = true;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void OnMouseDown()
    {
        // 如果有任意一个窗口是激活状态，说明已经打开了 -> 本次点击无效
        if (IsAnyWindowActive())
            return;

        // 打开所有窗口
        foreach (var win in windowsToActivate)
        {
            if (win != null)
                win.SetActive(true);
        }

        // 播放点击音效
        if (audioSource != null && clickSfx != null)
        {
            audioSource.PlayOneShot(clickSfx);
        }
    }

    /// <summary>
    /// 检查是否有任意一个窗口处于激活状态
    /// </summary>
    bool IsAnyWindowActive()
    {
        foreach (var win in windowsToActivate)
        {
            if (win != null && win.activeInHierarchy)
                return true;
        }
        return false;
    }

    // ========= 悬浮高亮 =========
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
