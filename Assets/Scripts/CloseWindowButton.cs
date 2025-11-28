using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CloseWindowButton : MonoBehaviour
{
    [Header("要关闭的窗口根节点（默认为父节点）")]
    public GameObject windowRoot;

    [Header("点击音效（AudioSource 必须挂在 windowRoot 上）")]
    public AudioClip clickSfx;

    [Header("悬浮高亮颜色")]
    public Color hoverColor = new Color(1f, 0.9f, 0.9f, 1f);

    [Header("按钮检测 Layer（只勾 Handle 层）")]
    public LayerMask handleLayerMask;

    private SpriteRenderer sr;
    private Color originalColor;
    private bool hasOriginalColor = false;

    private AudioSource windowAudio;
    private bool clicked = false;

    private Collider2D myCollider;
    private Camera cam;

    // 当前是否在 hover 状态
    private bool isHovered = false;

    void Awake()
    {
        cam = Camera.main;
        myCollider = GetComponent<Collider2D>();

        // 默认窗口 = 父级
        if (windowRoot == null && transform.parent != null)
            windowRoot = transform.parent.gameObject;

        // 用 windowRoot 上的 AudioSource
        if (windowRoot != null)
            windowAudio = windowRoot.GetComponent<AudioSource>();

        if (windowAudio == null && clickSfx != null)
            Debug.LogWarning("CloseWindowButton: windowRoot 上没有 AudioSource，但你设置了 clickSfx。音效将无法播放。", this);

        // SpriteRenderer
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalColor = sr.color;
            hasOriginalColor = true;
        }
    }

    void Update()
    {
        if (cam == null || clicked) return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        // ? 用 RaycastAll 从鼠标位置发一条很短的射线
        //   direction 不能是 (0,0)，这里随便用 Vector2.up，长度给 0.01 就行
        Vector2 dir = Vector2.up;
        float dist = 0.01f;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            mouseWorld,
            dir,
            dist,
            handleLayerMask  // 只检测 Handle 层，Ground 层完全无视
        );

        bool hoveredThisFrame = false;

        foreach (var h in hits)
        {
            if (h.collider == myCollider)
            {
                hoveredThisFrame = true;
                break;
            }
        }

        // ---------- 悬浮高亮 ----------
        if (hoveredThisFrame != isHovered)
        {
            isHovered = hoveredThisFrame;

            if (sr != null && hasOriginalColor)
            {
                sr.color = isHovered ? hoverColor : originalColor;
            }
        }

        // ---------- 点击 ----------
        if (isHovered && Input.GetMouseButtonDown(0))
        {
            OnClicked();
        }
    }

    void OnClicked()
    {
        if (clicked) return;
        clicked = true;

        // 播放窗口音效
        if (windowAudio != null && clickSfx != null)
            windowAudio.PlayOneShot(clickSfx);

        HideWindow(windowRoot);

        // 恢复颜色（虽然 Renderer 会被关掉）
        if (sr != null && hasOriginalColor)
            sr.color = originalColor;
    }

    /// <summary>
    /// 隐藏窗口（不 SetActive(false)）
    /// </summary>
    void HideWindow(GameObject root)
    {
        if (root == null) return;

        // 1. 禁用所有 Renderer
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        // 2. 禁用所有 Collider2D（除关闭按钮本身以外）
        foreach (var col in root.GetComponentsInChildren<Collider2D>(true))
        {
            if (col.gameObject == this.gameObject)
                continue;

            col.enabled = false;
        }
    }
}
